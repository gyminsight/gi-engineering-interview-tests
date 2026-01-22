using Test1.Models;

namespace Test1.Repository
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> ListMembers(CancellationToken cancellationToken); 

        Task<Member> CreateMember(Member newMember, CancellationToken cancellationToken);

        Task<int> DeleteMember(int uid, CancellationToken cancellationToken);
    }
}