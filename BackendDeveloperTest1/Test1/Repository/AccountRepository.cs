using Dapper;
using Test1.Contracts;
using Test1.Models;

namespace Test1.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ISessionFactory _sessionFactory;

        public AccountRepository(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        // =========================
        // LIST
        // =========================
        public async Task<IEnumerable<Account>> ListAccounts(CancellationToken cancellationToken)
        {
            await using var db = await _sessionFactory.CreateContextAsync(cancellationToken);

            const string sql = @"
SELECT
    UID,
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
FROM account;";

            var result = await db.Session.QueryAsync<Account>(
                sql,
                transaction: db.Transaction);

            db.Commit();
            return result;
        }

        // =========================
        // GET
        // =========================
        public async Task<Account?> GetAccount(int uid, CancellationToken cancellationToken)
        {
            await using var db = await _sessionFactory.CreateContextAsync(cancellationToken);

            const string sql = @"
SELECT
    UID,
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
FROM account
WHERE UID = @UID;";

            var result = await db.Session.QueryFirstOrDefaultAsync<Account>(
                sql,
                new { UID = uid },
                db.Transaction);

            db.Commit();
            return result;
        }

        // =========================
        // CREATE
        // =========================
        public async Task<Account> CreateAccount(Account account, CancellationToken cancellationToken)
{
    await using var db = await _sessionFactory.CreateContextAsync(cancellationToken);

    const string insertSql = @"
INSERT INTO account (
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
) VALUES (
    @LocationUid,
    @Guid,
    @CreatedUtc,
    @UpdatedUtc,
    @Status,
    @EndDateUtc,
    @AccountType,
    @PaymentAmount,
    @PendCancel,
    @PendCancelDateUtc,
    @PeriodStartUtc,
    @PeriodEndUtc,
    @NextBillingUtc
);";


    account.Guid = Guid.NewGuid();
    account.CreatedUtc = DateTime.UtcNow;

    var parameters = new
    {
        account.LocationUid,
        account.Guid,
        account.CreatedUtc,
        account.UpdatedUtc,
        account.Status,
        account.EndDateUtc,
        account.AccountType,
        account.PaymentAmount,
        PendCancel = account.PendCancel ? 1 : 0,
        account.PendCancelDateUtc,
        account.PeriodStartUtc,
        account.PeriodEndUtc,
        account.NextBillingUtc
    };

    await db.Session.ExecuteAsync(insertSql, parameters, db.Transaction);

    // SQLite: get generated UID
    account.Uid = await db.Session.ExecuteScalarAsync<int>(
        "SELECT last_insert_rowid();",
        transaction: db.Transaction);

    db.Commit();
    return account;
}


        // =========================
        // UPDATE
        // =========================
        public async Task<Account> UpdateAccount(Account updatedAccount, CancellationToken cancellationToken)
{
    await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);

    const string sql = @"
UPDATE account SET
    LocationUid = @LocationUid,
    Guid = @Guid,
    CreatedUtc = @CreatedUtc,
    UpdatedUtc = @UpdatedUtc,
    Status = @Status,
    EndDateUtc = @EndDateUtc,
    AccountType = @AccountType,
    PaymentAmount = @PaymentAmount,
    PendCancel = @PendCancel,
    PendCancelDateUtc = @PendCancelDateUtc,
    PeriodStartUtc = @PeriodStartUtc,
    PeriodEndUtc = @PeriodEndUtc,
    NextBillingUtc = @NextBillingUtc
WHERE UID = @UID;";

    var parameters = new
    {
        UID = updatedAccount.Uid,
        LocationUid = updatedAccount.LocationUid,
        Guid = updatedAccount.Guid,
        CreatedUtc = updatedAccount.CreatedUtc,
        UpdatedUtc = updatedAccount.UpdatedUtc,
        Status = updatedAccount.Status,
        EndDateUtc = updatedAccount.EndDateUtc,
        AccountType = updatedAccount.AccountType,
        PaymentAmount = updatedAccount.PaymentAmount,
        PendCancel = updatedAccount.PendCancel ? 1 : 0,
        PendCancelDateUtc = updatedAccount.PendCancelDateUtc,
        PeriodStartUtc = updatedAccount.PeriodStartUtc,
        PeriodEndUtc = updatedAccount.PeriodEndUtc,
        NextBillingUtc = updatedAccount.NextBillingUtc
    };

    await dbContext.Session.ExecuteAsync(sql, parameters, dbContext.Transaction);

    dbContext.Commit();

    // Return the updated object so the interface is satisfied
    return updatedAccount;
}


        // =========================
        // DELETE
        // =========================
        public async Task<int> DeleteAccount(int uid, CancellationToken cancellationToken)
        {
            await using var db = await _sessionFactory.CreateContextAsync(cancellationToken);

            await db.Session.ExecuteAsync(
                "DELETE FROM member WHERE AccountUid = @UID;",
                new { UID = uid },
                db.Transaction);

            var count = await db.Session.ExecuteAsync(
                "DELETE FROM account WHERE UID = @UID;",
                new { UID = uid },
                db.Transaction);

            db.Commit();
            return count;
        }

        // GI-Interview-Test Task 3: Get members by account GUID
        public async Task<IEnumerable<Member>> GetMembers(Guid accountGuid, CancellationToken cancellationToken)
{
    await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
        .ConfigureAwait(false);

    const string sql = @"
SELECT m.*
FROM member m
INNER JOIN account a ON m.AccountUid = a.UID
WHERE a.Guid = @AccountGuid;";

    var members = await dbContext.Session.QueryAsync<Member>(
        sql,
        new { AccountGuid = accountGuid },
        dbContext.Transaction
    ).ConfigureAwait(false);

    dbContext.Commit();

    return members;
}

public async Task<int> DeleteAllExceptPrimary(int accountUid, CancellationToken cancellationToken)
{
    await using var db = await _sessionFactory.CreateContextAsync(cancellationToken)
        .ConfigureAwait(false);

    const string sql = @"
DELETE FROM member
WHERE AccountUid = @AccountUid
AND ""Primary"" = false;";

    var deletedCount = await db.Session.ExecuteAsync(sql, new { AccountUid = accountUid }, db.Transaction);

    db.Commit();

    return deletedCount;
}


    }
}
