using Test1.DTOs;
using Test1.Models;

namespace Test1.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountReadDto>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<AccountReadDto> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> CreateAccountAsync(AccountCreateDto account, CancellationToken cancellationToken);
        Task<bool> UpdateAccountAsync(Guid id, AccountUpdateDto account, CancellationToken cancellationToken);
        Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid id, CancellationToken cancellationToken);
    }
}
