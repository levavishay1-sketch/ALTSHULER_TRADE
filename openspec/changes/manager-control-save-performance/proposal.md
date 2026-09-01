## Why

Approving and saving a Manager Control record (`alt_authorizationmanagement`) has become slow. The approval issues a nested update to `alt_digitalformverification`, which re-enters `PreUpdateDigitalFormVerification` + `SystemPostUpdateDigitalFormVerification`. Those plugins run change-tracking and "return to Manager Control" logic that repeatedly performs uncached, synchronous platform round-trips inside the user's save: the same `alt_digitalformverification` entity metadata is retrieved ~4 times per save (uncached), the current control-stage team is retrieved once per `TeamsCodes` entry (N+1), cached global parameters are re-deserialized on every call, `TimeZoneInfo` is re-resolved in loops, and attribute metadata is linearly scanned per changed field.

The fix removes this wasted work **without changing any externally observable behavior**. The system's decisions — what triggers a return to Manager Control, what is written to the change log, when a file advances — must remain identical.

## What Changes

Eight independent changes, each committed separately so it can be reviewed and tested in isolation:

1. **Cache entity metadata.** `GlobalContext.GetEntityMetadata` and `CommonDAL.RetrieveLookupValues` route through a process-level cache keyed by entity logical name with a bounded TTL. Metadata is read-only reference data that changes only on solution publish.
2. **Remove the duplicate `TrackChanges` call** from `SystemPostUpdateDigitalFormVerification`; keep the identical call already made by `PreUpdateDigitalFormVerification`. The Post call cannot persist its result for the `alt_digitalformverification` target (it mutates an entity that is already committed) and otherwise repeats work the Pre call already did. `MoveLastAuthorizationManagementBack` stays in the Post plugin unchanged.
3. **Fix the `GetCurrentControlStage` N+1.** Retrieve the control-stage team once into a local, then look its code up in the `TeamsCodes` dictionary, instead of calling `teamDal.Get(...)` inside a LINQ predicate that re-evaluates it per dictionary entry. Same fix for the identical pattern in `DigitalFormVerificationBL.SetFormStatusCode`.
4. **Cache the deserialized global-parameter dictionaries.** `CacheManager.GetGlobalParameter<T>` currently caches the raw JSON string and re-parses it into `T` on every call. Add an opt-in typed cache so `ManagerApprovalExcludedFields` and `TeamsCodes` are deserialized once per TTL window. Existing `GetGlobalParameter<T>` callers are untouched.
5. **Resolve `TimeZoneInfo` once.** Replace the per-call `TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time")` in `ManagerControlChangeTrackingBL.ConvertUtcToIsraelTime` with a `static readonly` field.
6. *(not in scope)*
7. **Dictionary lookup for attribute metadata.** In `ManagerControlChangeTrackingBL.GetChangedFields`, build one case-insensitive `Dictionary<string, AttributeMetadata>` per call instead of `entityMetadata.Attributes.FirstOrDefault(...)` per changed field.
8. **Make `HasRevalntFieldsChanged` metadata-free.** Compare attribute values by type and identity (`OptionSetValue.Value`, `EntityReference.Id`, `Money.Value`, `DateTime`, primitives, null/absent) instead of formatting both sides to a display string and comparing the strings. This removes the entity-metadata dependency from the "did a relevant field change" decision and makes the comparison stricter (a lookup change is detected by id even when the display names match; an unchanged lookup is not falsely flagged when only `.Name` differs on one side). The set of fields considered (post-exclusion) is unchanged.
9. **Filter the write delta by field name before any retrieve.** In `TrackChanges` and `HasRevalntFieldsChanged`, first intersect `target.Attributes.Keys` with the non-excluded set; if nothing relevant remains, return early with the same result as today (`changes.Count == 0` / `return false`) before calling `GetEntityMetadata` or `RetrieveLookupValues`.

Non-goals: no change to plugin step registration (sync/async, rank, images, filtering attributes), no change to `ManagerApprovalExcludedFields` / `TeamsCodes` configuration, no change to the `MoveLastAuthorizationManagementBack` decision logic, no async/sync restructuring, no change to the append-only `alt_ChangesAfterManagerApproval` column.

## Capabilities

### New Capabilities
- `manager-control-change-tracking`: The behavioural contract for post-manager-approval change tracking and the "return to Manager Control" mechanism — what is detected as a relevant change, what is written to the change log and when, and which authorization record is sent back. This capability has no spec today; it is introduced here as the contract these performance changes must preserve, with one intentional refinement recorded (change 8: relevant-change detection compares values by type/identity rather than by formatted display string).

### Modified Capabilities
<!-- none: no existing specs -->

## Impact

- **Framework:** `Shared/Framework/Alt.Framework/GlobalContext.cs`, `Shared/Framework/Alt.Framework/Cache/CacheManager.cs`, `Shared/Framework/Alt.Framework/Extensions/EntityExtensions.cs` (possibly a new comparison helper).
- **DAL:** `DataAccessLayer/Crm/Alt.DataAccessLayer.Crm/CommonDAL.cs`.
- **BL:** `BusinessLogicLayer/Crm/Alt.BusinessLogicLayer.Crm/ManagerControlChangeTrackingBL.cs`, `.../AuthorizationManagementBL.cs`, `.../DigitalFormVerificationBL.cs`.
- **Plugins:** `CrmEntryPoints/Plugins/Alt.Crm.Plugins.DigitalFormVerification/SystemPostUpdateDigitalFormVerification.cs`.
- **Assemblies redeployed:** `Alt.Framework`, `Alt.DataAccessLayer.Crm`, `Alt.BusinessLogicLayer.Crm`, and every ILMerged plugin/custom-API/workflow assembly that statically links them (i.e. a full plugin redeploy). The `.External` variants are only affected if changes 1/4 touch shared framework code they link — noted per task.
- **Runtime:** in-process Dynamics sandbox (plugins, custom APIs, workflow activities). Out-of-process WebJobs / Web API pick up framework changes 1 and 4 only if built from the same `Alt.Framework`.
- **Data / config:** none.
