using Microsoft.AspNetCore.Mvc;
using Test1.DTOs;
using Test1.Exceptions;
using Test1.Interfaces;
using Test1.Services;

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
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET api/<MembersController>/5
        [HttpGet("{gUid:guid}")]
        public async Task<ActionResult<MemberReadDto>> GetById(Guid gUid, CancellationToken cancellationToken)
        {
            var result = await _memberService.GetMemberByIdAsync(gUid, cancellationToken);

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        // POST api/<MembersController>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] MemberCreateDto member, CancellationToken cancellationToken)
        {
            try 
            {
                if (member == null)
                {
                    return BadRequest();
                }

                var created = await _memberService.CreateMemberAsync(member, cancellationToken);
                if (!created)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                return Created();
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

        // DELETE api/<MembersController>/5
        [HttpDelete("{gUid:guid}")]
        public async Task<ActionResult> DeleteById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _memberService.DeleteMemberAsync(gUid, cancellationToken);
                if (!deleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (LastAccountMemberException ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
