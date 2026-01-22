using Test1.Models;

namespace Test1.Repository
{
    public interface IAccountRepository
    {
        
        Task<IEnumerable<Account>> ListAccounts(CancellationToken cancellationToken); 

        Task<Account> GetAccount(int uid, CancellationToken cancellationToken);

        Task<Account> CreateAccount(Account newAccount, CancellationToken cancellationToken);

        Task<Account> UpdateAccount(Account updatedAccount, CancellationToken cancellationToken);
        
        Task<int> DeleteAccount(int uid, CancellationToken cancellationToken);

        // GI-Interview-Test Task 3: Add method to get members by account GUID
        Task<IEnumerable<Member>> GetMembers(Guid accountGuid, CancellationToken cancellationToken);

        Task<int> DeleteAllExceptPrimary(int accountUid, CancellationToken cancellationToken);
    }
}