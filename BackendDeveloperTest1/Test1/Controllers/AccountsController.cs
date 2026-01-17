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

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with list of accounts, 204 No Content if empty, 400 Bad Request on error.</returns>
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
                    return NoContent(); 
                }
                return Ok(results); 
            }
            catch(Exception ex)
            {
                //Log.Error(ex, "Error in Account Controller List: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message); 
            }

        }

        /// <summary>
        /// Retrieves all members belonging to a specific account.
        /// </summary>
        /// <param name="accountGuid">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with list of members, 204 No Content if empty, 400 Bad Request on error.</returns>
        // GET: api/accounts
        [HttpGet("membersByAccounts/{accountGuid:Guid}")]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> List(Guid accountGuid, CancellationToken cancellationToken)
        {
            try
            {
                var results = await _getmembersService.GetAllMembersByAccountAsync(accountGuid,cancellationToken);
                if (results == null || !results.Any())
                {
                    return NoContent(); 
                }
                return Ok(results); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Retrieves a specific account by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with account details, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // GET api/accounts/5
        [HttpGet("{gUid:Guid}")]
        public async Task<ActionResult<AccountReadDto>> GetById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _accountService.GetAccountByIdAsync(gUid, cancellationToken);

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
        /// Creates a new account.
        /// </summary>
        /// <param name="account">The account data transfer object containing account information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>201 Created on success, 400 Bad Request if account is null or creation fails.</returns>
        // POST api/<AccountsController>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AccountCreateDto account, CancellationToken cancellationToken)
        {
            try
            {
                if (account == null)
                    return BadRequest("Account model is empty"); 

                var created = await _accountService.CreateAccountAsync(account, cancellationToken);
                if (created)
                    return Created(); 
                return BadRequest("Account could not be created."); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        /// <summary>
        /// Updates an existing account.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account to update.</param>
        /// <param name="account">The account data transfer object containing updated account information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
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
                    return NotFound(); 

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        /// <summary>
        /// Deletes an account and its associated data.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // DELETE api/<AccountsController>/5
        [HttpDelete("{gUid:Guid}")]
        public async Task<ActionResult<AccountReadDto>> DeleteById(Guid gUid, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _accountService.DeleteAccountAsync(gUid, cancellationToken);
                if (deleted)
                    return NoContent(); 

                return NotFound(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        /// <summary>
        /// Deletes all non-primary members from an account.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // DELETE api/<AccountsController>/5
        [HttpDelete("delete/nonprimary/{gUid:Guid}")]
        public async Task<ActionResult> DeleteNonPrimary(Guid gUid, CancellationToken cancellationToken)
        {

            try
            {
                var deleted = await _accountService.DeleteNonPrimaryMembersAsync(gUid, cancellationToken);
                if (deleted)
                    return NoContent();
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }

        }

    }
}
