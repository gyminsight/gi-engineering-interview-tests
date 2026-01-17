using Test1.Core;
using Test1.DTOs;
using Test1.Models;

namespace Test1.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<bool> ExistingPrimaryMemberByAccountValidation(Guid accountGuid, DapperDbContext dbContext);
        Task<bool> LastAccountMemberValidation(Guid accountGuid, DapperDbContext dbContext);
        Task<IEnumerable<Member>> GetAllMembersByAccountAsync(Guid accountGuid, DapperDbContext dbContext);

    }
}
