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

        // GET: api/<MembersController>
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

        // GET api/<MembersController>/5
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

        // POST api/<MembersController>
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

        // DELETE api/<MembersController>/5
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
