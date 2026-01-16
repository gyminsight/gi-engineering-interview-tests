using Test1.DTOs;

namespace Test1.Interfaces
{
    public interface IGetMembersByAccountService
    {
        public Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken);
    }
}
