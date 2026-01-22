using Test1.DTOs;

namespace Test1.Interfaces
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberReadDto>> GetAllMembersAsync(CancellationToken cancellationToken);
        Task<MemberReadDto> GetMemberByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> CreateMemberAsync(MemberCreateDto member, CancellationToken cancellationToken);
        Task<bool> DeleteMemberAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken);

    }
}
