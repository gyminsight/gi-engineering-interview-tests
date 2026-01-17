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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> List(CancellationToken cancellationToken)
        {
            try
            {
                var results = await _memberService.GetAllMembersAsync(cancellationToken);
                return Ok(results); //200 OK
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);//400 Bad Request
            }
        }

        /// <summary>
        /// Retrieves a specific member by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with member details, 404 Not Found if member does not exist, 400 Bad Request on error.</returns>
        [HttpGet("{gUid:guid}")]
        public async Task<ActionResult<MemberReadDto>> GetById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _memberService.GetMemberByIdAsync(gUid, cancellationToken);

                if (result == null)
                    return NotFound(); //404 Not Found
                return Ok(result); //200 OK
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); //400 Bad Request
            }
        }

        /// <summary>
        /// Creates a new member.
        /// </summary>
        /// <param name="member">The member data transfer object containing member information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>201 Created on success, 400 Bad Request if member is null, creation fails, or primary member exception occurs.</returns>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] MemberCreateDto member, CancellationToken cancellationToken)
        {
            try 
            {
                if (member == null)
                    return BadRequest("Member model is empty"); //400 Bad Request

                var created = await _memberService.CreateMemberAsync(member, cancellationToken);
                if (created)
                    return Created(); //201 Created

                return BadRequest("Member could not be created."); // 400 Bad Request

            }
            catch (PrimaryMemberException ex)
            {
                return BadRequest(ex.Message); //400 Bad Request
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); //400 Bad Request
            }

        }

        /// <summary>
        /// Deletes a member by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if member does not exist, 400 Bad Request if member is the last account member or on error.</returns>
        [HttpDelete("{gUid:guid}")]
        public async Task<ActionResult> DeleteById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _memberService.DeleteMemberAsync(gUid, cancellationToken);
                if (deleted)
                    return NoContent(); //204 No Content

                return NotFound(); //404 Not Found
            }
            catch (LastAccountMemberException ex)
            {
                return BadRequest(ex.Message); //400 Bad Request
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); //400 Bad Request
            }

        }
    }
}
