namespace Test1.Tests;

public class UnitTest1
{
    // LocationsController.List()
    [Fact]
    public async Task LocationsControllerList_ReturnsOkWithLocations_WhenLocationsExist()
    { }

    // LocationsController.List()
    [Fact]
    public async Task LocationsControllerList_ReturnsOkWithEmpty_WhenNoLocationsExist()
    { }

    // LocationsController.GetById(Guid id)
    [Fact]
    public async Task LocationsControllerGetByID_ReturnsOkWithLocation_WhenLocationExists()
    { }

    // LocationsController.GetById(Guid id)
    [Fact]
    public async Task LocationsControllerGetByID_ReturnsBadRequest_WhenLocationNotExists()
    { }

    // LocationsController.Create(LocationDto model)
    [Fact]
    public async Task LocationsControllerCreate_ReturnsOk_WhenModelValid()
    { }

    // LocationsController.Create(LocationDto model)
    [Fact]
    public async Task LocationsControllerCreateReturnsBadRequest_WhenModelNotValid()
    { }

    // LocationsController.DeleteById(Guid id)
    [Fact]
    public async Task LocationsControllerDeleteById_ReturnsOk_WhenLocationExists()
    { }

    // LocationsController.DeleteById(Guid id)
    [Fact]
    public async Task LocationsControllerDeleteById_ReturnsBadRequest_WhenLocationNotExists()
    { }

    // AccountsController.List()
    [Fact]
    public async Task AccountsControllerList_ReturnsOkWithAccounts_WhenAccountsExist()
    { }

    // AccountsController.List()
    [Fact]
    public async Task AccountsControllerList_ReturnsOkWithEmpty_WhenNoAccountsExist()
    { }

    // AccountsController.GetById(Guid id)
    [Fact]
    public async Task AccountsControllerGetById_ReturnsOkWithAccount_WhenAccountExists()
    { }

    // AccountsController.GetById(Guid id)
    [Fact]
    public async Task AccountsControllerGetById_ReturnsBadRequest_WhenAccountNotExists()
    { }

    // AccountsController.Create(AccountDto model)
    [Fact]
    public async Task AccountsControllerCreate_ReturnsOk_WhenModelValid()
    { }

    // AccountsController.Create(AccountDto model)
    [Fact]
    public async Task AccountsControllerCreate_ReturnsBadRequest_WhenModelInvalid()
    { }

    // AccountsController.DeleteById(Guid id)
    [Fact]
    public async Task AccountsControllerDeleteById_ReturnsOk_WhenAccountExists()
    { }

    // AccountsController.DeleteById(Guid id)
    [Fact]
    public async Task AccountsControllerDeleteById_ReturnsBadRequest_WhenAccountNotExists()
    { }

    // AccountsController.UpdateByID(Guid id, AccountDto model)
    [Fact]
    public async Task AccountsControllerUpdateByID_ReturnsOk_WhenAccountExistsAndModelValid()
    { }

    // AccountsController.UpdateByID(Guid id, AccountDto model)
    [Fact]
    public async Task AccountsControllerUpdateByID_ReturnsBadRequest_WhenAccountNotExists()
    { }

    // AccountsController.UpdateByID(Guid id, AccountDto model)
    [Fact]
    public async Task AccountsControllerUpdateByID_ReturnsBadRequest_WhenModelNotValid()
    { }

    // AccountsController.ListMembers(Guid id)
    [Fact]
    public async Task AccountsControllerListMembers_ReturnsOkWithMembers_WhenAccountMembersExist()
    { }

    // AccountsController.ListMembers(Guid id)
    [Fact]
    public async Task AccountsControllerListMembers_ReturnsOkWithEmpty_WhenNoAccountMembersExist()
    { }

    // AccountsController.ListMembers(Guid id)
    [Fact]
    public async Task AccountsControllerListMembers_ReturnsBadRequest_WhenAccountNotExists()
    { }

    // AccountsController.DeleteMembers(Guid id)
    [Fact]
    public async Task AccountsControllerDeleteMembers_ReturnsOk_WhenAccountExists()
    { }

    // AccountsController.DeleteMembers(Guid id)
    [Fact]
    public async Task AccountsControllerDeleteMembers_ReturnsBadRequest_WhenAccountNotExists()
    { }

    // MembersController.List()
    [Fact]
    public async Task MembersControllerList_ReturnsOkWithMembers_WhenMembersExist()
    { }

    // MembersController.List()
    [Fact]
    public async Task MembersControllerList_ReturnsOkWithEmpty_WhenNoMembersExist()
    { }

    // MembersController.Create(MemberDto model)
    [Fact]
    public async Task MembersControllerCreate_ReturnsOkAndSetsPrimary1_WhenModelValidAndNoPrimaryMemberExistsOnAccount()
    { }

    // MembersController.Create(MemberDto model)
    [Fact]
    public async Task MembersControllerCreate_ReturnsOkAndSetsPrimary0_WhenModelValidAndPrimaryMemberExistsOnAccount()
    { }

    // MembersController.Create(MemberDto model)
    [Fact]
    public async Task MembersControllerCreate_ReturnsBadRequest_WhenModelNotValid()
    { }

    // MembersController.DeleteById(Guid id)
    [Fact]
    public async Task MembersControllerDeleteById_ReturnsOk_WhenMemberExistsAndIsNotPrimary()
    { }

    // MembersController.DeleteById(Guid id)
    [Fact]
    public async Task MembersControllerDeleteById_ReturnsOk_WhenMemberExistsAndIsPrimaryAndNotLastMemberOnAccount()
    { }

    // MembersController.DeleteById(Guid id)
    [Fact]
    public async Task MembersControllerDeleteById_ReturnsBadRequest_WhenMemberExistsAndIsPrimaryAndLastMemberOnAccount()
    { }

    // MembersController.DeleteById(Guid id)
    [Fact]
    public async Task MembersControllerDeleteById_ReturnsBadRequest_WhenMemberNotExists()
    { }
}
