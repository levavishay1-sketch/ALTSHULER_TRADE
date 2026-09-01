## Purpose

Defines the observable behaviour of post-manager-approval change tracking on the joining-form control (`alt_digitalformverification`) and its related entities: which edits are recorded in the human-readable change log, and which edits cause an in-flight authorization record to be sent back to Manager Control. This capability is introduced as the contract that the Manager Control save-performance changes must preserve.

## ADDED Requirements

### Requirement: Relevant-change detection is based on attribute values, not their formatted display strings

The system SHALL decide whether "a relevant field changed" on an update by comparing each non-excluded changed attribute's value between the pre-image and the target by type and identity: option-set by numeric value, lookup by target id and logical name, currency by decimal amount, and date/time, boolean, string and numeric types by value. The system SHALL NOT base this decision on a locale- or metadata-dependent rendering of the values.

The set of attributes considered SHALL be exactly the attributes present on the update minus the fields configured in `ManagerApprovalExcludedFields` for that entity (case-insensitive), unchanged from current behaviour.

#### Scenario: Excluded-only update is not a relevant change

- **WHEN** an update to `alt_digitalformverification` writes only fields listed in `ManagerApprovalExcludedFields["alt_digitalformverification"]` (for example the fields written by the approval workflow, by a deposit, or by a Shenhav callback)
- **THEN** relevant-change detection returns "no relevant change"
- **AND** no change-log entry is written and no authorization record is sent back to Manager Control

#### Scenario: Non-excluded value change is a relevant change

- **WHEN** an update changes a non-excluded field on `alt_digitalformverification`, `alt_accountholder`, `alt_kyc` or `alt_moneylaunderingcalculation` to a value that differs from the pre-image
- **THEN** relevant-change detection returns "relevant change"

#### Scenario: Lookup re-pointed to a different record with the same name

- **WHEN** a non-excluded lookup attribute is changed from one record to a different record that happens to have the same primary-name value
- **THEN** relevant-change detection returns "relevant change" (the target id differs)

#### Scenario: Lookup written with the same id but no name populated

- **WHEN** a non-excluded lookup attribute is present on the update with the same target id as the pre-image, differing only in whether `.Name` is populated
- **THEN** relevant-change detection returns "no relevant change"

### Requirement: Change-log entries are written once per triggering update

The system SHALL append a human-readable entry to `alt_ChangesAfterManagerApproval` for each relevant change made after a manager approval has been recorded (`alt_LastManagerApprovalDate` is set) while the form is in an authorization status that allows tracking. Each triggering update SHALL produce exactly one set of change-log entries; the same change SHALL NOT be recorded twice for a single update.

#### Scenario: Direct edit to the control form after approval

- **WHEN** a user changes a non-excluded, reviewable field directly on `alt_digitalformverification` after `alt_LastManagerApprovalDate` is set and the form status is InAuthorizationProcess or AwaitingForDeposit
- **THEN** exactly one change-log entry per changed field is appended to `alt_ChangesAfterManagerApproval`, showing the field, old value, new value, the acting user, and the timestamp in Israel local time
- **AND** the entry content and formatting are identical to the pre-change implementation

#### Scenario: Edit to a related entity after approval

- **WHEN** a user changes a non-excluded field on a related `alt_accountholder`, `alt_kyc`, or `alt_moneylaunderingcalculation` record under the same conditions
- **THEN** exactly one change-log entry per changed field is appended to the parent form's `alt_ChangesAfterManagerApproval`

#### Scenario: New related record created after approval

- **WHEN** a related `alt_accountholder`, `alt_kyc`, or `alt_moneylaunderingcalculation` record is created under the same conditions
- **THEN** exactly one "record created" entry is appended to the parent form's `alt_ChangesAfterManagerApproval`, identical in content to the pre-change implementation

### Requirement: Return to Manager Control is unchanged

The system SHALL send the most recently created active authorization record for a form back to Manager Control (`alt_ControlStageStatusCode = BackManagerBackControl`) when, and only when, a relevant change is detected on a form that (a) has a recorded manager approval, (b) is in form status InAuthorizationProcess or AwaitingForDeposit, and (c) is currently held by the Operational Control or Money-Laundering Control team. The trigger conditions, the record selected, and the resulting workflow SHALL be identical to the pre-change implementation.

#### Scenario: Reviewable change while in Operational or Money-Laundering control

- **WHEN** a user makes a relevant change (per the detection rules above) to a form or related record that meets conditions (a), (b) and (c)
- **THEN** the most recently created active authorization record for that form is set to `BackManagerBackControl` and the file returns to Manager Control

#### Scenario: Automated / workflow update does not trigger a return

- **WHEN** the approval workflow, a deposit, a Shenhav callback, or a related-entity recalculation writes only excluded fields to `alt_digitalformverification`
- **THEN** no authorization record is sent back to Manager Control

### Requirement: Entity metadata and configuration reads do not change results

The system MAY cache entity metadata and deserialized global-parameter values to avoid repeated platform round-trips. Any such cache SHALL be transparent: for a given published solution version and global-parameter value, every consumer SHALL observe the same metadata and configuration it would observe without the cache, within a bounded staleness window after a solution publish or parameter edit.

#### Scenario: Metadata cache reflects a bounded staleness window

- **WHEN** entity metadata or a global parameter changes (solution publish or parameter edit)
- **THEN** callers observe the new value no later than the configured cache lifetime after the change
- **AND** callers never observe a value that was never configured
