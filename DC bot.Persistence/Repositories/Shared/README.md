# Shared

Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.

## Overview

- PostgreSqlConcurrencyHelper retries supported concurrency conflicts. TransactionCleanup attempts rollback with a fresh five-second deadline even if the caller was cancelled.
- The original operation exception remains primary. A rollback failure is attached under DC_bot.TransactionRollbackFailure and prevents another transaction retry.

## Classes

### PostgreSqlConcurrencyHelper

**File:** `DC bot.Persistence/Repositories/Shared/PostgreSqlConcurrencyHelper.cs`

**Purpose:** Recognizes supported PostgreSQL conflicts and bounds retries while preserving transaction ownership.

**Methods:**

- `ExecuteInSerializableTransactionWithRetryAsync` — Runs the database callback in a fresh serializable transaction per attempt, retrying recognized conflicts only after successful owned rollback.

```csharp
internal static async Task<T> ExecuteInSerializableTransactionWithRetryAsync<T>(
    IDbContextFactory<BotDbContext> dbContextFactory,
    Func<BotDbContext, CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken,
    int maxAttempts = DefaultMaxAttempts);
```

- `ExecuteInSerializableTransactionWithRetryAsync` — Runs the database callback in a fresh serializable transaction per attempt, retrying recognized conflicts only after successful owned rollback.

```csharp
internal static async Task ExecuteInSerializableTransactionWithRetryAsync(
    IDbContextFactory<BotDbContext> dbContextFactory,
    Func<BotDbContext, CancellationToken, Task> operation,
    CancellationToken cancellationToken,
    int maxAttempts = DefaultMaxAttempts);
```

- `ExecuteWithUniqueViolationRetryAsync` — Retries the supplied operation for recognized unique conflicts with bounded attempts and cancellable backoff.

```csharp
internal static async Task ExecuteWithUniqueViolationRetryAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken,
    int maxAttempts = DefaultMaxAttempts);
```

- `IsRetriable` — Checks nested PostgreSQL errors for supported unique or serializable-concurrency conflicts.

```csharp
internal static bool IsRetriable(Exception exception);
```

- `IsUniqueViolation` — Checks nested PostgreSQL errors for a unique-constraint violation.

```csharp
internal static bool IsUniqueViolation(DbUpdateException exception);
```

- `IsUniqueViolation` — Checks nested PostgreSQL errors for a unique-constraint violation.

```csharp
internal static bool IsUniqueViolation(Exception exception);
```

**Additional methods:** `SaveChangesIgnoringUniqueViolationAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
    dbContextFactory,
    operation,
    cancellationToken,
    maxAttempts);
```

---

### TransactionCleanup

**File:** `DC bot.Persistence/Repositories/Shared/TransactionCleanup.cs`

**Purpose:** Rolls back a failed operation with a fresh deadline while preserving its original exception.

**Methods:**

- `TryRollbackAsync` — Attempts owned rollback independently of caller cancellation and records cleanup failure on the original exception.

```csharp
internal static async Task<bool> TryRollbackAsync(IDbContextTransaction transaction, Exception operationException);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await TransactionCleanup.TryRollbackAsync(transaction, operationException);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
