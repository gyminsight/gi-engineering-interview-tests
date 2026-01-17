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
        /// Retrieves a specific account by its unique identifier.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with account details, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // GET api/accounts/{accountId}
        [HttpGet("{accountId:Guid}")]
        public async Task<ActionResult<AccountReadDto>> GetById(Guid accountId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _accountService.GetAccountByIdAsync(accountId, cancellationToken);

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
        // POST api/accounts
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
        /// <param name="accountId">The unique identifier of the account to update.</param>
        /// <param name="account">The account data transfer object containing updated account information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // PUT api/accounts/{accountId}
        [HttpPut("{accountId:Guid}")]
        public async Task<ActionResult> Update(Guid accountId, [FromBody] AccountUpdateDto account, CancellationToken cancellationToken)
        {
            try
            {
                if (account == null)
                    return BadRequest("Account model is empty");

                if (accountId != account.Guid)
                    return BadRequest("Route id and account Guid must match.");

                var updated = await _accountService.UpdateAccountAsync(accountId, account, cancellationToken);
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
        /// <param name="accountId">The unique identifier of the account to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // DELETE api/accounts/{accountId}
        [HttpDelete("{accountId:Guid}")]
        public async Task<ActionResult<AccountReadDto>> DeleteById(Guid accountId, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _accountService.DeleteAccountAsync(accountId, cancellationToken);
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
        /// Retrieves all members belonging to a specific account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>200 OK with list of members, 204 No Content if empty, 400 Bad Request on error.</returns>
        // GET api/accounts/{accountId}/members
        [HttpGet("{accountId:Guid}/members")]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> GetMembers(Guid accountId, CancellationToken cancellationToken)
        {
            try
            {
                var results = await _getmembersService.GetAllMembersByAccountAsync(accountId, cancellationToken);
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
        /// Deletes all non-primary members from an account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>204 No Content on success, 404 Not Found if account does not exist, 400 Bad Request on error.</returns>
        // DELETE api/accounts/{accountId}/members/non-primary
        [HttpDelete("{accountId:Guid}/members/non-primary")]
        public async Task<ActionResult> DeleteNonPrimaryMembers(Guid accountId, CancellationToken cancellationToken)
        {

            try
            {
                var deleted = await _accountService.DeleteNonPrimaryMembersAsync(accountId, cancellationToken);
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
