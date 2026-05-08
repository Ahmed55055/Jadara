# 16. Use Immutable Employee Snapshots for Reward Temporal Integrity

Date: 2026-01-XX

## Context

The Reward slice currently maintains a live reference to the Employee slice to access data such as Salary, Grade, and Department. This architecture creates two primary technical challenges:

1. **Loss of Temporal Integrity**: Rewards are calculated based on an employee's state at a specific point in time. When employee data is updated in the source slice, historical rewards lose their context. Recalculating or auditing a past reward using "live" data results in incorrect values, as the system cannot reconstruct the state that existed at the time of calculation.
2. **Referential Fragility**: The Reward slice depends on the physical existence of the Employee entity. If an employee record is hard-deleted from the system, the associated reward history becomes orphaned or is lost, violating financial record-keeping and audit requirements.

## Decision

We will implement a **Versioned Immutable Snapshot** pattern within the Reward slice. Instead of referencing live data, the Reward slice will persist a local, read-only "mirror" of the specific employee attributes used at the time of the transaction.

The decision involves:

- **Denormalized Snapshots**: Each Reward will be linked to a unique `EmployeeSnapshot` record instead of the live Employee entity.
- **Point-in-Time Capture**: A snapshot will be generated at the moment of reward creation or during any lifecycle event that triggers a recalculation.
- **Schema Isolation**: The snapshot will store only the subset of fields required for the reward (e.g., Salary, NationalNumber), ensuring storage efficiency.
- **Immutable Versioning**: Once a reward is finalized, its snapshot version is locked. Subsequent changes to the employee in the source system will not propagate to finalized historical records.

## Consequences

### Positive

- **Historical Accuracy**: Past rewards always reflect the employee data that existed at the moment of calculation, ensuring audit reliability.
- **Domain Autonomy**: The Reward slice becomes decoupled from the Employee lifecycle; rewards remain queryable even if the original employee record is deleted.
- **Improved Read Performance**: Eliminates the need for cross-slice joins or API calls to the Employee slice during reporting.
- **Audit Compliance**: Provides a permanent, versioned record of the factors that led to a specific financial outcome.

### Negative

- **Storage Overhead**: Data redundancy increases as multiple versions of employee attributes are stored across different snapshots.
- **Data Duplication**: Identical employee data may be duplicated across multiple rewards if snapshots are not shared efficiently.
- **Eventual Consistency**: Changes to employee data must be explicitly synchronized if a "draft" reward requires updating before its finalization.

### Neutral

- **Write Complexity**: The process of creating a reward now requires an additional step to capture and persist the employee state.
- **Maintenance**: Changes to the required employee fields for specific reward types will necessitate updates to the snapshot schema logic.