using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using Test1.DTOs;
using Test1.Interfaces;
using Serilog;

namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IMemberService _getmembersService;

        /// <summary>
        /// Constructor. 
        /// </summary>
        public AccountsController(IAccountService accountService, IMemberService getmembersService)
        {
            _accountService = accountService;
            _getmembersService = getmembersService;
        }

        // GET: api/accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountReadDto>>> List(CancellationToken cancellationToken)
        {
            //Log.Information("Invoking {Name} on {Controller}",nameof(List),nameof(AccountsController));
            try
            {
                var results = await _accountService.GetAllAccountsAsync(cancellationToken);
                if (results == null || !results.Any())
                {
                    return NoContent(); //204 No Content
                }
                return Ok(results); //200 OK
            }
            catch(Exception ex)
            {
                //Log.Error(ex, "Error in Account Controller List: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message); //400 Bad Request
            }

        }

        // GET: api/accounts
        [HttpGet("membersByAccounts/{accountGuid:Guid}")]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> List(Guid accountGuid, CancellationToken cancellationToken)
        {
            try
            {
                var results = await _getmembersService.GetAllMembersByAccountAsync(accountGuid,cancellationToken);
                if (results == null || !results.Any())
                {
                    return NoContent(); //204 No Content
                }
                return Ok(results); //200 OK
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET api/accounts/5
        [HttpGet("{gUid:Guid}")]
        public async Task<ActionResult<AccountReadDto>> GetById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _accountService.GetAccountByIdAsync(gUid, cancellationToken);

                if (result == null)
                    return NotFound(); // 404 Not Found
                return Ok(result); // 200 OK
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<AccountsController>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AccountCreateDto account, CancellationToken cancellationToken)
        {
            try
            {
                if (account == null)
                    return BadRequest("Account model is empty"); // 400 Bad Request

                var created = await _accountService.CreateAccountAsync(account, cancellationToken);
                if (created)
                    return Created(); // 201 Created
                return BadRequest("Account could not be created."); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  // 400 Bad Request
            }
        }

        // PUT api/<AccountsController>/5
        [HttpPut("{gUid:Guid}")]
        public async Task<ActionResult> Update(Guid gUid, [FromBody] AccountUpdateDto account, CancellationToken cancellationToken)
        {
            try
            {
                if (account == null)
                    return BadRequest("Account model is empty");

                if (gUid != account.Guid)
                    return BadRequest("Route id and account uUid must match."); // 400 Bad Request

                var updated = await _accountService.UpdateAccountAsync(gUid, account, cancellationToken);
                if (!updated)
                    return NotFound(); // 404 Not Found

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("{gUid:Guid}")]
        public async Task<ActionResult<AccountReadDto>> DeleteById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _accountService.DeleteAccountAsync(gUid, cancellationToken);
                if (deleted)
                    return NoContent(); // 204 No Content

                return NotFound(); // 404 Not Found
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("delete/nonprimary/{gUid:Guid}")]
        public async Task<ActionResult> DeleteNonPrimary(Guid gUid, CancellationToken cancellationToken)
        {

            try
            {
                var deleted = await _accountService.DeleteNonPrimaryMembersAsync(gUid, cancellationToken);
                if (deleted)
                    return NoContent();//   204 No Content
                return NotFound();// 404 Not Found
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }

        }

    }
}
