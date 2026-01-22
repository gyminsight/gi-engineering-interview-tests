using Microsoft.AspNetCore.Mvc;
using Test1.Models;
using Test1.Repository;

namespace Test1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountsController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        // GET: api/accounts
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var accounts = await _accountRepository.ListAccounts(cancellationToken);
            return Ok(accounts);
        }

        // GET: api/accounts/{uid}
        [HttpGet("{uid:int}")]
        public async Task<IActionResult> Get(int uid, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAccount(uid, cancellationToken);

            if (account == null)
                return NotFound();

            return Ok(account);
        }

        // POST: api/accounts
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] Account account,
            CancellationToken cancellationToken)
        {
            var created = await _accountRepository.CreateAccount(account, cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { uid = created!.Uid },
                created);
        }

        // PUT: api/accounts/{uid}
        [HttpPut("{uid:int}")]
        public async Task<IActionResult> Update(
            int uid,
            [FromBody] Account account,
            CancellationToken cancellationToken)
        {
            if (uid != account.Uid)
                return BadRequest("UID mismatch");

            var updated = await _accountRepository.UpdateAccount(account, cancellationToken);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE: api/accounts/{uid}
        [HttpDelete("{uid:int}")]
        public async Task<IActionResult> Delete(int uid, CancellationToken cancellationToken)
        {
            var deleted = await _accountRepository.DeleteAccount(uid, cancellationToken);

            if (deleted == 0)
                return NotFound();

            return NoContent();
        }

        // GI-Interview-Test Task 3: Get members by account GUID
        [HttpGet("{accountGuid:Guid}/members")]
        public async Task<IActionResult> GetMembers(Guid accountGuid, CancellationToken cancellationToken)
        {
            var members = await _accountRepository.GetMembers(accountGuid, cancellationToken);
            return Ok(members);
        }

        // DELETE: api/accounts/{accountUid}/members
[HttpDelete("{accountUid:int}/members")]
public async Task<IActionResult> DeleteAllNonPrimaryMembers(int accountUid, CancellationToken cancellationToken)
{
    var deletedCount = await _accountRepository.DeleteAllExceptPrimary(accountUid, cancellationToken);

    if (deletedCount == 0)
        return NotFound("No non-primary members found to delete.");

    return Ok(new { DeletedCount = deletedCount });
}


        
    }
}
