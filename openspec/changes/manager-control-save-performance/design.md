## Context

See `proposal.md` — Why. All changes target the synchronous work done during a Manager Control approve/save: the nested `alt_digitalformverification` update re-enters `PreUpdateDigitalFormVerification` + `SystemPostUpdateDigitalFormVerification`, which run `ManagerControlChangeTrackingBL.TrackChanges` / `HasRevalntFieldsChanged` / `MoveLastAuthorizationManagementBack` and `AuthorizationManagementBL` workflow resolution.

Verified constraints from the codebase:

- `.NET Framework` classic MSBuild solution; no `dotnet build`, no offline unit-test suite. Tests are integration tests against a live org (`Test/Alt.Test.CrmApi`, MSTest).
- In-process assemblies target v4.6.2 and are ILMerged into each plugin package; `Alt.Framework`, `Alt.DataAccessLayer.Crm`, `Alt.BusinessLogicLayer.Crm` are statically linked into every deployable plugin/custom-API/workflow assembly.
- `GlobalContext.GetEntityMetadata(string)` (`GlobalContext.cs:221`) calls the SDK `GetEntityMetadata` extension — one `RetrieveEntity` per call, uncached.
- `CacheManager` (`Cache/CacheManager.cs`) already has a process-wide `static ConcurrentDictionary<string, CacheItem>` with TTL. `GetGlobalParameter<T>` caches the raw JSON **string** and calls `value.TryParseValue<T>()` on every call.
- `ManagerApprovalExcludedFields` production configuration is known and confirmed complete for every current automated `alt_digitalformverification` writer (see prior analysis). These changes must not weaken that.
- Change-tracking behaviour depends on a registered **PreImage** on the plugin steps. If `preImage == null`, `MoveLastAuthorizationManagementBack` skips `HasRevalntFieldsChanged` entirely and `TrackChanges` treats every update as a creation. This design does not alter that; it assumes the current (working) image registration.

## Goals / Non-Goals

**Goals:**

- Eliminate the repeated uncached `GetEntityMetadata` calls, the `GetCurrentControlStage` N+1, the per-call JSON re-parse, the per-call `TimeZoneInfo` lookup, and the per-field metadata scan.
- Keep every externally observable outcome identical: same change-log entries (content and formatting), same "return to Manager Control" decisions, same file advancement.
- One reviewable, independently revertible Git commit per change.

**Non-Goals:**

- No plugin step re-registration (sync/async, rank, filtering attributes, images).
- No `ManagerApprovalExcludedFields` / `TeamsCodes` edits.
- No change to `MoveLastAuthorizationManagementBack` decision logic, the append-only `alt_ChangesAfterManagerApproval` column, or the sync/async split across entities.
- No new environment variable or global parameter (avoids a config deployment step).

## Decisions

### Commit order

`5 → 3 → 7 → 4 → 1 → 9 → 8 → 2`. Rationale: pure-mechanical, zero-behaviour changes first (5, 3, 7, 4), then the caching layer (1), then the change-tracking refactors ordered so each builds on the last (9 adds the early-out, 8 changes the comparison, 2 removes the now-redundant second pass). Each commit builds and is shippable on its own.

### Change 5 — `TimeZoneInfo` resolved once

`private static readonly TimeZoneInfo IsraelStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");` in `ManagerControlChangeTrackingBL`. `ConvertUtcToIsraelTime` uses the field. Identical output. Alternative (cache in `CacheManager`) rejected — a `static readonly` is simpler and the value cannot change within a process.

### Change 3 — `GetCurrentControlStage` N+1

`AuthorizationManagementBL.GetCurrentControlStage`: retrieve the team once into a local `int? teamCode = teamDal.Get(controlStageTeamId.Id, [alt_TeamCodeInt]).alt_TeamCodeInt;` then `return teamsCodes.FirstOrDefault(x => x.Value == teamCode).Key;`. Same result set, same `FirstOrDefault` semantics, 1 retrieve instead of 1–N. Apply the identical transform to the inline pattern in `DigitalFormVerificationBL.SetFormStatusCode` (line ~206). No other call sites have this shape (`ManagerControlChangeTrackingBL.GetTeamCode` already retrieves once; `GetTeamName` does a pure in-memory `FirstOrDefault`).

### Change 7 — attribute-metadata dictionary

`ManagerControlChangeTrackingBL.GetChangedFields`: build `var attrByName = entityMetadata.Attributes.ToDictionary(a => a.LogicalName, a => a, StringComparer.OrdinalIgnoreCase);` once, then `attrByName.TryGetValue(fieldName, out var attributeMetadata)` in the loop. `ToDictionary` throws on duplicate keys — entity attribute logical names are unique, so this is safe; if defensive handling is wanted, use a grouped/first-wins build. Lookup semantics (`OrdinalIgnoreCase` on `LogicalName`) are identical to the current `FirstOrDefault`.

### Change 4 — parsed-config cache keyed by content

New tiny helper `Alt.Framework.Cache.ParsedJsonCache`: `static T GetOrParse<T>(string json, Func<string, T> parser)` backed by a `static ConcurrentDictionary<(Type, string), object>`. `GetGlobalParameter<T>` is left untouched. In `ManagerControlChangeTrackingBL.GetExcludedFields` / `GetTeamsCodes` and `DigitalFormVerificationBL.SetFormStatusCode`, the sequence becomes: get the cached **string** via the existing `GetGlobalParameter<string>` path, then `ParsedJsonCache.GetOrParse(json, s => JsonSerializer.Deserialize<...>(s))`. Because the cache key includes the exact JSON string, a config edit produces a new key and a fresh parse; a parser exception surfaces exactly as today. No TTL needed — the string it is keyed on already has one, and stale entries are harmless and negligible in size.

Alternative (add `GetGlobalParameterTyped<T>` to `CacheManager` using `GetCachedItem<T>`) rejected: `RetrieveCacheItem` throws when the callback returns `null`, which would convert a legitimately-empty parameter from `default(T)` into an exception — a behaviour change.

### Change 1 — entity-metadata cache

New `Alt.Framework.Cache.EntityMetadataCache`: `static EntityMetadata GetOrAdd(string logicalName, Func<EntityMetadata> factory)` backed by `static ConcurrentDictionary<string, CacheEntry>` where `CacheEntry` holds the metadata and a UTC timestamp; entries older than a compiled-in TTL (`30 minutes`) are refreshed via the factory. Concurrency: `ConcurrentDictionary` + a per-key `Lazy<EntityMetadata>` (or double-checked lock) so the factory runs once per key per TTL window.

Call sites routed through it:
- `GlobalContext.GetEntityMetadata(string)` — wraps `this.OrganizationService.GetEntityMetadata(entityName)`.
- `CommonDAL.RetrieveLookupValues` — the `GlobalContext.GetEntityMetadata(targetReference.LogicalName)` call (line ~109).

`GlobalContext.GetEntityMetadata` is called from contexts where `CacheManager` may be absent; the new cache is a standalone static with no `GlobalContext`/`ILog` dependency, so it is safe there. The factory result is only cached when non-null. The SDK call still throws for an unknown entity exactly as today (never reaches the cache).

TTL rationale: metadata changes only on solution publish; a 30-minute worst-case staleness for option-set labels / new attributes in the audit log is acceptable and matches the spirit of the existing `alt_*CacheLifeTimeInMinutes` values. Not configurable to avoid a new environment variable; a `const` is trivially adjustable in a later change if needed.

Alternative (route through `CacheManager.GetCachedItem`) rejected: it needs a non-null `GlobalContext` and an environment-variable TTL lookup, adding coupling and a config dependency to a low-level framework method.

### Change 9 — name-filter early-out

In `TrackChanges` and `HasRevalntFieldsChanged`, before calling `GetEntityMetadata` / `RetrieveLookupValues`:

```
var relevant = target.Attributes.Keys
    .Where(k => !string.IsNullOrWhiteSpace(k))
    .Where(k => !excludedLower.Contains(k.ToLowerInvariant()))
    .ToList();
if (relevant.Count == 0) { /* same as changes.Count == 0 / return false */ }
```

This reproduces the current terminal outcome (`TrackChanges` returns without writing; `HasRevalntFieldsChanged` returns `false`) for the all-excluded case, which is exactly the case the current code reaches only *after* the retrieves. `GetExcludedFields` is already called in both methods; #9 just moves it before the metadata work and adds the count check. The primary-key attribute (`alt_digitalformverificationid` etc.) is in the excluded config, so it does not keep the list non-empty; if any environment lacks it in config, add an explicit skip of `target.LogicalName + "id"` and the standard system fields — but per the confirmed production config this is not required.

For `TrackChanges` specifically: the early-out must sit **after** the existing guard clauses that can legitimately act on an all-excluded update — i.e. after the `preImage == null → SaveCreation` branch (creation logging inspects the record, not the delta) and after the `alt_LastManagerApprovalDate` / status guards. It replaces only the work between "we know a real diff is needed" and `GetChangedFields`.

### Change 8 — value-based comparison in `HasRevalntFieldsChanged`

Replace the `GetDisplayValue(...)`-string comparison with a typed comparison helper `AttributeValuesEqual(object oldValue, object newValue)`:

| Type | Comparison |
|---|---|
| both null / attribute absent on both | equal |
| one has a value, other has none | not equal |
| `OptionSetValue` | `.Value` (int) |
| `OptionSetValueCollection` | same set of `.Value` |
| `EntityReference` | `.Id` **and** `.LogicalName` |
| `Money` | `.Value` (decimal `==`) |
| `DateTime` | equal at **whole-second precision** (preserves the current `dd/MM/yyyy HH:mm:ss` display comparison) |
| `bool`, `int`, `long`, `decimal`, `double`, `string`, `Guid` | value equality |
| fallback | `object.Equals` |

"No value" means the attribute key is absent **or** the value is `null` — matching `GetDisplayValue`, which returns `string.Empty` in both cases. The set of attributes iterated (target keys minus excluded) is unchanged. `EnrichLookups` / `RetrieveLookupValues` are **not** used by this method today and remain unused.

`GetChangedFields` (the log builder) is deliberately **not** changed to use the helper — it still needs `GetDisplayValue` to render old/new values for humans and to resolve option-set labels, so it keeps its `entityMetadata` parameter and its per-field `GetDisplayValue` calls. Only the boolean "did it change" question moves to the helper. (`GetChangedFields` still gains #7's dictionary and sits behind #9's early-out.)

### Change 2 — remove the Post `TrackChanges`

Delete line 16 (`managerControlChangeTrackingBL.TrackChanges(targetDigitalFormVerification, preDigitalFormVerification);`) from `SystemPostUpdateDigitalFormVerification.cs`. Keep line 17 (`MoveLastAuthorizationManagementBack`). `PreUpdateDigitalFormVerification` already calls `TrackChanges` with the same target/pre-image; for the `alt_digitalformverification` target the log is applied by mutating `target` (`UpdateDigitalFormVerificationTargetChangesLog`), which persists only from a Pre plugin. The Post call currently either no-ops (target mutation after commit) or, if it takes the `UpdateDigitalFormVerificationChangesLog` branch, issues a redundant second `Update`. This change is last so it can be verified against the behaviour established by 9 and 8.

The child-entity plugins (`SystemPostUpdateAccountHolder`, `SystemPostUpdateMoneyLaunderingCalculation`, `SystemAsyncUpdateKYC`, and the `*Create*` variants) are **not** touched — there `TrackChanges` runs in a single (Post/Async) plugin and correctly takes the `OrganizationService.Update` branch.

## Risks / Trade-offs

- **[Change 8 — lookup false-positive removed]** Today, because `HasRevalntFieldsChanged` compares `GetDisplayValue`, a plugin `Target` lookup (which usually has no `.Name`) compared against a PreImage lookup (which has `.Name`) reports "changed" **even when the lookup id is unchanged**. After change 8, re-writing a non-excluded lookup to the same target id is correctly "not changed". → Mitigation: this only changes the outcome when the *sole* non-excluded field on an update is a same-valued lookup; any real field change still triggers. Covered by a dedicated regression scenario (tasks.md, change 8). If any production flow depends on a no-op lookup write forcing a return to Manager Control, it must be identified during review — none is known.
- **[Change 8 — Money scale]** `1000m` vs `1000.00m` was "changed" (string `"1000"` ≠ `"1000.00"`), now "not changed" (`decimal ==`). → These are numerically equal; not a real change. Documented; low risk.
- **[Change 1 — stale metadata]** Up to 30 min after a solution publish, option-set labels / newly added attributes may be rendered from cached metadata in the change log. → No decision logic depends on labels; only log text. Acceptable, matches existing cache lifetimes. TTL is a one-line `const`.
- **[Change 1 — shared static across tenants]** The cache is keyed by entity logical name only. In a single-tenant sandbox worker this is correct. If the same worker ever served multiple orgs with divergent schemas this would be wrong — not the case for this deployment. → Documented; key can be prefixed with org id later if ever needed.
- **[Change 4 — unbounded static dict]** `ParsedJsonCache` never evicts. → One entry per distinct JSON value per type; effectively two entries total for this use. Negligible.
- **[Change 2 — sole audit writer is now the Pre plugin]** If the Pre step's image or registration differs from the Post step's (e.g. Post has a PreImage the Pre step lacks), removing the Post call could drop a log entry. → Verify both steps' images during review; the regression scenarios exercise the exact paths. Revertible in one commit.
- **[All changes — no local build/test]** Cannot compile or run tests in this environment. → Each commit is small and isolated; the user builds in Visual Studio and runs the integration suite + the manual scenarios in tasks.md before deploying. Commits are ordered so a bisect is meaningful.

## Migration Plan

1. Land the 8 commits on a branch in the stated order.
2. User builds `Altshuler.sln` in Visual Studio (Debug), runs `nuget restore` first if needed.
3. Deploy the rebuilt plugin/custom-API/workflow assemblies (full set — they all ILMerge the three shared assemblies) plus, if WebJobs/Web API are redeployed from this build, those too.
4. Run the manual verification scenarios in `tasks.md` per change, and the `Alt.Test.CrmApi` integration suite.
5. Rollback: revert the specific commit(s) and redeploy. No data or config migration, so rollback is code-only and safe at any point.

## Open Questions

None that block implementation. The one item to confirm during review (not a blocker): that the Pre and Post `alt_digitalformverification` update steps carry equivalent PreImages, so change 2 cannot drop a log entry.
