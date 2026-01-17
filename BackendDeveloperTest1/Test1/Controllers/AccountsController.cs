using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using Test1.DTOs;
using Test1.Interfaces;

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
            try
            {
                var results = await _accountService.GetAllAccountsAsync(cancellationToken);
                return Ok(results);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET: api/accounts
        [HttpGet("membersByAccounts/{accountGuid:Guid}")]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> List(Guid accountGuid, CancellationToken cancellationToken)
        {
            try
            {
                var results = await _getmembersService.GetAllMembersByAccountAsync(accountGuid,cancellationToken);
                return Ok(results);
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
            var result = await _accountService.GetAccountByIdAsync(gUid, cancellationToken);

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        // POST api/<AccountsController>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AccountCreateDto account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return BadRequest();
            }

            var created = await _accountService.CreateAccountAsync(account, cancellationToken);
            if (!created)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created();

            //return CreatedAtAction(nameof(GetById), new { id = account.Uid }, account);
        }

        // PUT api/<AccountsController>/5
        [HttpPut("{gUid:Guid}")]
        public async Task<ActionResult> Update(Guid gUid, [FromBody] AccountUpdateDto account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return BadRequest();
            }

            //if (!string.Equals(id, account.Uid, StringComparison.OrdinalIgnoreCase))
            //{
            //    return BadRequest("Route id and customer.CustomerID must match.");
            //}
            if (gUid != account.Guid)
            {
                return BadRequest("Route id and account uUid must match.");
            }

            var updated = await _accountService.UpdateAccountAsync(gUid, account, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("{gUid:Guid}")]
        public async Task<ActionResult<AccountReadDto>> DeleteById(Guid gUid, CancellationToken cancellationToken)
        {
            var deleted = await _accountService.DeleteAccountAsync(gUid, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
