using Microsoft.AspNetCore.Mvc;
using Test1.Models;
using Test1.Repository;

namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberRepository _memberRepository;

        public MembersController(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        // GET: api/members
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var members = await _memberRepository.ListMembers(cancellationToken);
            return Ok(members);
        }

        // POST: api/members
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Member member, CancellationToken cancellationToken)
        {
            var created = await _memberRepository.CreateMember(member, cancellationToken);

            return CreatedAtAction(
                nameof(List),
                new { uid = created.Uid },
                created);
        }

        // DELETE: api/members/{uid}
        [HttpDelete("{uid:int}")]
        public async Task<IActionResult> Delete(int uid, CancellationToken cancellationToken)
        {
            var deleted = await _memberRepository.DeleteMember(uid, cancellationToken);

            if (deleted == 0)
                return NotFound();

            return NoContent();                 
        }
    }
}