## 0. Baseline (do once, before any change)

- [ ] 0.1 On a test org, enable **Plugin Trace Log** = All (Settings → Administration → System Settings → Customization). Verify trace records appear after a save.
- [ ] 0.2 Pick a **re-approval** case: a joining form (`alt_digitalformverification`) that has already had one manager approval (`alt_LastManagerApprovalDate` populated), currently sitting at Management Control with an open `alt_authorizationmanagement` record. Record its id and current `alt_ChangesAfterManagerApproval` text.
- [ ] 0.3 Approve that Manager Control record (set `alt_ControlStageStatusCode = Approval`, save). In Plugin Trace Log, open the trace for the `SystemPostUpdateAuthorizationManagement` step and record: **Execution Duration**, and the count of `GetEntityMetadata` / entity-`Get` `LogEntry` lines for `alt_digitalformverification` and `team`. Note the wall-clock save time seen in the UI. This is the **before** number for every "provides value" check below.
- [ ] 0.4 Capture the same baseline for a **direct form edit**: change one reviewable, non-excluded field on a post-approval form that is in Operational Control, save, record trace duration and the resulting `alt_ChangesAfterManagerApproval` entry and whether the file returned to Manager Control.
- [ ] 0.5 Run the `Alt.Test.CrmApi` integration suite with a valid `CRMConnectionString` and record the pass/fail baseline.

## 1. Change 5 — resolve TimeZoneInfo once

- [x] 1.1 In `ManagerControlChangeTrackingBL`, add `private static readonly TimeZoneInfo IsraelStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");` and make `ConvertUtcToIsraelTime` use it. Verify by inspection that no other `FindSystemTimeZoneById` call remains in the class.
- [ ] 1.2 Build `Altshuler.sln` (Debug) in Visual Studio; verify 0 new warnings/errors in the three affected projects.
- [ ] 1.3 **No-regression test:** repeat baseline 0.3 and 0.4. Check the new `alt_ChangesAfterManagerApproval` entries: the `תאריך שינוי` / `תאריך אישור מנהל` timestamps must be in Israel local time and match what the old build produced for the same UTC instant (compare against a manually converted value). Confirm identical formatting (`dd/MM/yyyy HH:mm:ss`).
- [ ] 1.4 **Provides-value check:** in the Plugin Trace Log for the save, confirm the header/among-rows timezone work is no longer repeated — there is no measurable per-row cost; the check is simply that behaviour is unchanged. (This change's value is micro; it is bundled for completeness.)
- [ ] 1.5 `git add` the one file and commit: `perf(change-tracking): resolve Israel TimeZoneInfo once as a static field`.

## 2. Change 3 — fix GetCurrentControlStage N+1

- [x] 2.1 In `AuthorizationManagementBL.GetCurrentControlStage`, retrieve the team once into a local `int? teamCode`, then `return teamsCodes.FirstOrDefault(x => x.Value == teamCode).Key;`. Verify no `teamDal.Get(` remains inside a LINQ predicate in that method.
- [x] 2.2 Apply the same transform to the inline `teamsCodesParameter.FirstOrDefault(x => x.Value == teamDAL.Get(...).alt_TeamCodeInt)` in `DigitalFormVerificationBL.SetFormStatusCode` (~line 206). Verify by inspection.
- [x] 2.3 Grep the solution for `FirstOrDefault(` combined with `.Get(` on the same line to confirm no other instance of this pattern was missed. (Only the two sites above matched.)
- [ ] 2.4 Build; verify no new warnings/errors.
- [ ] 2.5 **No-regression test — approval routing:** run a full approval chain on a test form: Joining Control → (manager required) Management Control → approve → Money-Laundering Control → approve → Operational Control → approve → SendToOpenPortfolioInShenhav. At each stage confirm the file advances to the **same** next stage as the current production build (compare `alt_ControlStageTeamId` and the created `alt_authorizationmanagement` names). Also run a path where manager verification is **not** required (Joining → Operational directly).
- [ ] 2.6 **No-regression test — deposit form-status transition:** on a form in Operational Control with `alt_InitialDepositCode = AwaitinglDeposit` and status InAuthorizationProcess, trigger the deposit path and confirm `alt_FormStatusCode` moves to `AwaitingForDeposit` exactly as before (this exercises the `SetFormStatusCode` team lookup).
- [ ] 2.7 **Provides-value check:** in Plugin Trace Log for the approval save, count `team` retrieves in the `SystemPostUpdateAuthorizationManagement` and `PreUpdateAuthorizationManagement` traces. Expect the count to drop from ~N per `GetCurrentControlStage` call to 1 per call (compare to baseline 0.3).
- [x] 2.8 Commit (2 files): `perf(auth-mgmt): retrieve control-stage team once instead of per TeamsCodes entry`. (commit 5ca41d8)

## 3. Change 7 — attribute-metadata dictionary lookup

- [x] 3.1 In `ManagerControlChangeTrackingBL.GetChangedFields`, build `var attrByName = entityMetadata.Attributes.ToDictionary(a => a.LogicalName, a => a, StringComparer.OrdinalIgnoreCase);` once before the loop; replace the per-field `entityMetadata.Attributes.FirstOrDefault(...)` with `attrByName.TryGetValue(fieldName, out var attributeMetadata)`. Keep the `attributeMetadata == null` / not-found `continue` behaviour.
- [ ] 3.2 Build; verify no new warnings/errors.
- [ ] 3.3 **No-regression test:** repeat baseline 0.4 with a form edit that changes **several** reviewable fields of different types at once (an option set, a text field, a lookup, a date, a currency, a two-option field). Confirm the resulting `alt_ChangesAfterManagerApproval` block lists exactly the same fields, in the same order, with the same old/new display values and the same `שדה:` (field display name) labels as the production build.
- [ ] 3.4 **No-regression test — unknown attribute:** if feasible, craft an update whose target contains an attribute not in entity metadata (rare; e.g. a removed field via SDK) and confirm it is still silently skipped.
- [x] 3.5 **Provides-value check:** confirmed by code review (O(fields) dictionary build + O(1) lookups vs O(fields × attributes) scan); no trace metric needed.
- [x] 3.6 Commit (1 file): `perf(change-tracking): index attribute metadata by name in GetChangedFields`. (commit 0f401d7)

## 4. Change 4 — cache deserialized global-parameter dictionaries — SKIPPED (deferred by request)

- [ ] 4.1 Add `Alt.Framework.Cache.ParsedJsonCache` with `static T GetOrParse<T>(string json, Func<string,T> parser)` backed by `static ConcurrentDictionary<(Type,string),object>`. Unit-invariant: same `json` string returns a value equal to `parser(json)`; a different string re-parses.
- [ ] 4.2 In `ManagerControlChangeTrackingBL.GetExcludedFields` and `GetTeamsCodes`, keep the existing `GetGlobalParameter<string>` call to fetch the cached JSON string, then parse via `ParsedJsonCache.GetOrParse(json, s => JsonSerializer.Deserialize<...>(s))`. Same null/empty handling as today (empty/whitespace → `Array.Empty`/`null` before touching the cache).
- [ ] 4.3 In `DigitalFormVerificationBL.SetFormStatusCode`, route the inline `JsonSerializer.Deserialize<Dictionary<string,int>>(...)` for `TeamsCodes` through `ParsedJsonCache` the same way.
- [ ] 4.4 Confirm `CacheManager.GetGlobalParameter<T>` itself is **unchanged** (grep for other callers to prove no behaviour shift).
- [ ] 4.5 Build; verify no new warnings/errors.
- [ ] 4.6 **No-regression test — config change is picked up:** edit `TeamsCodes` global parameter on the test org (e.g. add a whitespace char to the JSON). Within the global-parameter cache lifetime (`alt_GlobalParameterCacheLifeTimeInMinutes`), confirm the new value is used (approval routing still resolves control stages correctly). Then revert the parameter.
- [ ] 4.7 **No-regression test:** repeat 0.3 and 0.4 — change log entries and routing identical.
- [ ] 4.8 **No-regression test — malformed config:** temporarily set `ManagerApprovalExcludedFields` to invalid JSON on a scratch org; confirm the failure mode (exception surfaced to the plugin) is the same as the current build. Revert.
- [ ] 4.9 **Provides-value check:** enable Plugin Trace Log and confirm the `GetExcludedFields` / `GetTeamsCodes` `LogEntry` lines no longer show repeated deserialization work within one save (trace shows the parse happening once per distinct JSON string per worker). Compare `SystemPostUpdateDigitalFormVerification` execution duration to baseline.
- [ ] 4.10 Commit (2–3 files): `perf(config): parse ManagerApprovalExcludedFields and TeamsCodes once per value`.

## 5. Change 1 — cache entity metadata — SKIPPED (deferred by request)

- [ ] 5.1 Add `Alt.Framework.Cache.EntityMetadataCache` with `static EntityMetadata GetOrAdd(string logicalName, Func<EntityMetadata> factory)`: `ConcurrentDictionary<string,CacheEntry>`, `CacheEntry { EntityMetadata Value; DateTime RetrievedUtc; }`, `const int TtlMinutes = 30`, factory run once per key per TTL window (`Lazy<EntityMetadata>` or double-checked lock), only non-null results cached.
- [ ] 5.2 Route `GlobalContext.GetEntityMetadata(string)` through `EntityMetadataCache.GetOrAdd(name, () => this.OrganizationService.GetEntityMetadata(name))`.
- [ ] 5.3 Route the `GlobalContext.GetEntityMetadata(targetReference.LogicalName)` call in `CommonDAL.RetrieveLookupValues` through the same cache (it already calls `GlobalContext.GetEntityMetadata`, so 5.2 covers it — verify no other direct SDK `GetEntityMetadata` call exists on the hot path; leave `CrmBaseDAL.GetEntityMetadata()` (the no-arg `RetrieveEntityRequest` variant) as-is unless it is also on this path).
- [ ] 5.4 Build; verify no new warnings/errors. Confirm `Alt.Framework` still ILMerges cleanly into a sample plugin project.
- [ ] 5.5 **No-regression test — option-set labels in the log:** repeat 0.4 changing an **option-set** field; confirm the `ערך ישן` / `ערך חדש` show the correct localized option labels (this path uses `entityMetadata` for label resolution and must still work through the cache).
- [ ] 5.6 **No-regression test — lookup enrichment:** repeat 0.4 changing a **lookup** field on a related entity (so `RetrieveLookupValues` runs); confirm the log shows the lookup's display name correctly.
- [ ] 5.7 **No-regression test — metadata staleness bound:** publish a customization that adds a new option to an option set used on the form. Within 30 minutes confirm the new option's label resolves in the change log (before 30 min it may show the numeric value — acceptable and documented).
- [ ] 5.8 **No-regression test — other consumers:** run any flow that uses `GlobalContext.GetEntityMetadata` outside change tracking (e.g. TemplateParser `IsActivity`, `CrmBaseDAL.GetPrimeryAttributeValue`) and confirm unchanged results.
- [ ] 5.9 **Provides-value check:** Plugin Trace Log for the re-approval save (baseline 0.3) — the `RetrieveEntity` / `GetEntityMetadata` calls for `alt_digitalformverification` should drop from ~4 to 1 (first save after a worker recycle) and to 0 on subsequent saves within the TTL. Record the `SystemPostUpdateAuthorizationManagement` execution duration and compare to baseline — expect the largest single drop here.
- [ ] 5.10 Commit (2 files): `perf(framework): cache entity metadata per worker with a 30-minute TTL`.

## 6. Change 9 — name-filter early-out before metadata/lookup retrieves

- [x] 6.1 In `HasRevalntFieldsChanged`: after computing `excludedFieldsLower`, compute `relevant = target.Attributes.Keys` minus blanks minus excluded; if `relevant.Count == 0` return `false` **before** `GlobalContext.GetEntityMetadata`. (After change 8 this method no longer calls `GetEntityMetadata`; the early-out still avoids the loop and, with change 8, is the only pre-work.)
- [x] 6.2 In `TrackChanges`: keep all existing guard clauses (`digitalFormVerification == null`, `IsAllowedAuthorizationStatus`, `GetControlStageName`, `alt_LastManagerApprovalDate` check, `preImage == null → SaveCreation`). **After** those, compute the same `relevant` set and, if empty, `return;` **before** `GlobalContext.GetEntityMetadata(target.LogicalName)` and `GetChangedFields`. This must land exactly where the code currently proceeds to metadata retrieval.
- [ ] 6.3 Build; verify no new warnings/errors.
- [ ] 6.4 **No-regression test — all-excluded nested update:** run the re-approval (0.3). Confirm: (a) the file advances normally; (b) `alt_ChangesAfterManagerApproval` gets ONLY the "manager approval header" block from `BuildManagerApprovalInformation`, no spurious field rows (same as baseline); (c) the file is NOT sent back to Manager Control.
- [ ] 6.5 **No-regression test — deposit / Shenhav callback:** trigger the automatic-deposit DFV update and the Shenhav open-portfolio response on a post-approval form in Operational Control. Confirm no change-log rows are added and no return to Manager Control (identical to baseline).
- [ ] 6.6 **No-regression test — real user edit still tracked:** repeat 0.4 (reviewable field change on a post-approval form in Operational Control). Confirm the change IS logged and the file IS returned to Manager Control — the early-out must not suppress this.
- [ ] 6.7 **No-regression test — creation logging:** create a new `alt_kyc` / `alt_accountholder` under a post-approval form; confirm the "נוצרה רשומה חדשה" entry still appears (the `preImage == null` branch must run before the early-out).
- [ ] 6.8 **Provides-value check:** Plugin Trace Log for the re-approval (0.3) — `SystemPostUpdateDigitalFormVerification` and `PreUpdateDigitalFormVerification` traces should now show NO `GetEntityMetadata` / `RetrieveLookupValues` calls on the all-excluded nested update. Compare execution duration to the post-change-1 measurement.
- [x] 6.9 Commit (1 file): `perf(change-tracking): skip metadata/lookup retrieval when no non-excluded field changed`. (commit 4357cad)

## 7. Change 8 — value-based comparison in HasRevalntFieldsChanged

- [ ] 7.1 Add `AttributeValuesEqual(object oldValue, object newValue)` (in `EntityExtensions` or as a private helper in `ManagerControlChangeTrackingBL`) per the table in design.md — null/absent equivalence, `OptionSetValue.Value`, `OptionSetValueCollection` set-equality, `EntityReference` id+logicalname, `Money.Value`, `DateTime` at whole-second precision, primitives by value, `object.Equals` fallback.
- [ ] 7.2 Rewrite `HasRevalntFieldsChanged`'s loop to iterate the `relevant` set from change 9 and compare `preImage`/`target` values via `AttributeValuesEqual` (treating a missing key as "no value"). Remove the `GlobalContext.GetEntityMetadata` call and the `entityMetadata` local from this method only. Do **not** touch `GetChangedFields`.
- [ ] 7.3 Build; verify no new warnings/errors.
- [ ] 7.4 **No-regression test — matrix of field types.** On a post-approval form in Operational Control, make each of these single-field edits (one save each) and confirm the file returns to Manager Control AND the log records it, exactly as the production build:
  - option-set field → new value
  - text field → new text
  - currency field → new amount
  - date field → new date
  - two-option (bool) field → toggle
  - lookup field → point to a genuinely different record
- [ ] 7.5 **No-regression test — no-op saves do NOT trigger.** On the same form, save with no field change, and separately save re-selecting the SAME value in an option set and the SAME record in a lookup. Confirm: no return to Manager Control, no log entry. (Under the old build the same-lookup re-save may have falsely triggered — record whether production behaves differently here; this is the one intended refinement and must be signed off.)
- [ ] 7.6 **No-regression test — lookup, same display name, different record.** Create two records with identical primary names; on a post-approval form change a non-excluded lookup from one to the other and confirm the change IS detected (return + log). (Old build may have missed this.)
- [ ] 7.7 **No-regression test — currency scale.** If reproducible, set a currency field to a value that differs only in trailing zeros from the pre-image; confirm this is treated as "no change" (acceptable refinement).
- [ ] 7.8 **No-regression test — related entities.** Repeat 7.4's option-set and lookup cases on `alt_accountholder`, `alt_kyc`, `alt_moneylaunderingcalculation` edits and confirm identical trigger + log behaviour.
- [ ] 7.9 **No-regression test — `SetFormStatusCode` gate.** `SetFormStatusCode` also calls `HasRevalntFieldsChanged` (line 187). Verify the deposit/operational form-status transitions (2.6) and the `AcceptedDepositForApproval` → `InAuthorizationProcess` transition still behave identically — this consumer of the method must not change.
- [ ] 7.10 **Provides-value check:** Plugin Trace Log — `HasRevalntFieldsChanged` no longer triggers a `GetEntityMetadata` call anywhere (search the trace for `RetrieveEntity` on `alt_digitalformverification` during a `MoveLastAuthorizationManagementBack` / `SetFormStatusCode` call). Combined with change 1, the metadata retrieves for the whole save should now be 0–1.
- [ ] 7.11 Commit (1–2 files): `perf(change-tracking): compare attribute values by type/identity instead of formatted strings`.

## 8. Change 2 — remove duplicate TrackChanges from the Post plugin

- [ ] 8.1 Confirm (with the CRM customization UI or `pluginassembly`/`sdkmessageprocessingstep` query) the exact image registration on both the `PreUpdateDigitalFormVerification` and `SystemPostUpdateDigitalFormVerification` steps. Record the PreImage attribute lists. If the Post step has a PreImage covering attributes the Pre step does not, raise it before proceeding.
- [ ] 8.2 In `SystemPostUpdateDigitalFormVerification.cs`, delete the `managerControlChangeTrackingBL.TrackChanges(targetDigitalFormVerification, preDigitalFormVerification);` line. Keep the `MoveLastAuthorizationManagementBack` line and the `new ManagerControlChangeTrackingBL(...)` construction.
- [ ] 8.3 Build; verify no new warnings/errors.
- [ ] 8.4 **No-regression test — direct form edit logging.** Repeat 0.4 and 7.4. Confirm every change-log entry that the production build wrote is still written (same fields, same content, same single occurrence — not doubled, not missing).
- [ ] 8.5 **No-regression test — `alt_ChangeAfterManagerApprovalDate`.** Confirm this field is still stamped on the form after a tracked change (it is set by `UpdateDigitalFormVerificationTargetChangesLog` in the Pre path).
- [ ] 8.6 **No-regression test — related-entity edits.** Edit a reviewable field on `alt_accountholder` / `alt_kyc` / `alt_moneylaunderingcalculation` under a post-approval form; confirm the parent form's log still gets the entry (this path uses the child plugins' `TrackChanges`, untouched — but the nested DFV update it triggers no longer double-logs).
- [ ] 8.7 **No-regression test — return to Manager Control still fires.** Confirm 7.4's "file returns to Manager Control" outcomes are unchanged (this is driven by `MoveLastAuthorizationManagementBack`, still present in the Post plugin).
- [ ] 8.8 **Provides-value check:** Plugin Trace Log for a tracked form edit — `TrackChanges` should now appear **once** (Pre) not twice; `SystemPostUpdateDigitalFormVerification` execution duration should drop by roughly the cost of one `TrackChanges` pass (metadata + lookup + diff) versus the post-change-1 measurement. No second `Update` to `alt_digitalformverification` from `UpdateDigitalFormVerificationChangesLog` in the trace.
- [ ] 8.9 Commit (1 file): `perf(change-tracking): stop running TrackChanges twice per DigitalFormVerification update`.

## 9. Full regression pass (after all 8 commits)

- [ ] 9.1 Run the entire `Alt.Test.CrmApi` integration suite; compare to baseline 0.5 — no new failures.
- [ ] 9.2 End-to-end onboarding happy path on a fresh test form: create → Joining Control → Management Control → approve → Money-Laundering → approve → Operational → approve → deposit → SendToOpenPortfolioInShenhav → OpenedPortfolioInShenhav. Confirm the file reaches Shenhav exactly as on the production build, with the same authorization records and the same `alt_ChangesAfterManagerApproval` content for any post-approval edits made along the way.
- [ ] 9.3 "Change after manager approval" path: approve at Management Control, advance to Operational, make a reviewable edit, confirm return to Manager Control, re-approve, confirm advance again. Full cycle must match the production build.
- [ ] 9.4 Measure the re-approval save time (baseline 0.3) on the final build and record the improvement (target: from ~20 s toward ~3 s or better, matching the earlier `Depth == 1` experiment but without its behavioural risk).
- [ ] 9.5 Update `openspec/changes/manager-control-save-performance` status and run `openspec validate --change manager-control-save-performance --strict`.
