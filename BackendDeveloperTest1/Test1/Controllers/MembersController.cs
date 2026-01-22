using Microsoft.AspNetCore.Mvc;
using Test1.DTOs;
using Test1.Exceptions;
using Test1.Interfaces;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {

        private readonly IMemberService _memberService;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// Retrieves all members.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with list of all members, 400 Bad Request on error.</returns>
        // GET api/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var results = await _memberService.GetAllMembersAsync(cancellationToken);
                return Ok(results); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a specific member by its unique identifier.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with member details, 404 Not Found if member does not exist, 400 Bad Request on error.</returns>
        // GET api/members/{memberId}
        [HttpGet("{memberId:Guid}")]
        public async Task<ActionResult<MemberReadDto>> GetById(Guid memberId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _memberService.GetMemberByIdAsync(memberId, cancellationToken);

                if (result == null)
                    return NotFound(); 
                return Ok(result); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        /// <summary>
        /// Creates a new member.
        /// </summary>
        /// <param name="member">The member data transfer object containing member information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>201 Created on success, 400 Bad Request if member is null, creation fails, or primary member exception occurs.</returns>
        // POST api/members
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] MemberCreateDto member, CancellationToken cancellationToken)
        {
            try 
            {
                if (member == null)
                    return BadRequest("Member model is empty"); //400 Bad Request

                var created = await _memberService.CreateMemberAsync(member, cancellationToken);
                if (created)
                    return Created(); 

                return BadRequest("Member could not be created."); // 400 Bad Request

            }
            catch (PrimaryMemberException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }

        }

        /// <summary>
        /// Deletes a member by its unique identifier.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if member does not exist, 400 Bad Request if member is the last account member or on error.</returns>
        // DELETE api/members/{memberId}
        [HttpDelete("{memberId:Guid}")]
        public async Task<ActionResult> DeleteById(Guid memberId, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _memberService.DeleteMemberAsync(memberId, cancellationToken);
                if (deleted)
                    return NoContent(); 

                return NotFound(); 
            }
            catch (LastAccountMemberException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }

        }
    }
}
