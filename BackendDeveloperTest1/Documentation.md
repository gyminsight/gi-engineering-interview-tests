
# Documentation
This file documents notes and requested recommendations for the different tasks involved in this interview assessment project.

## Task 1: 
Create an accounts controller with list, create, read, update and delete endpoints.
	- UID of locations was not an element in the LocationDto. Accounts are associated to locations by the location's UID. This causes an issue when creating a new account, as the location's UID is required but can't be easily accessed. Added UID as a field in the LocationDto class as a workaround.
	- It is unclear what the account.PendCancel field is or how it's used. I recommend explaining it in documentation.

## Task 2:
Enhance the location's list endpoint to return the number of non-cancelled accounts (where the `account.Status` < [CANCELLED](Test1/Models/AccountStatusType.cs)) for each location.
	- Enhancing the list endpoint's output involved modifying the LocationDto, which necessitated a similar enhancement for the read endpoint (getByID) in order to avoid showing an incorrect "accounts: 0" in the results.

## Task 3:
Add an endpoint to the accounts controller that will return all members for a specified account (using the account's Guid)
	- Creating a Members controller and MemberDto class to make handling members easier
	- Note that member.AccountUID and member.LocationUID are different types than account.UID and location.UID (unsigned int vs integer). This might cause problems if there are a very large amount of accounts or locations.

## Task 4:
Create a members controller that will list, create, and delete members. 
   * The create endpoint should only allow for one primary member per account.
   * The delete endpoint should make the next member on the account the primary member, when the primary member is deleted for an account.  The endpoint should not allow deletion of the last member on the account.
	- Keeping track of whether a member is primary inside each member is prone to mistakes and overhead. I recommend making a new relationship table to keep track of what the primary member is for each account.
	- Keeping redundant data about the location inside each member entry *and* each account entry might lead to possible source-of-truth conflicts if a member's location and the member's account's location become different (unless that is somehow intentional). Unless it's intentional, I recommend keeping track of the information only in one place and accessing it by joining queries. If it *is* intentional, I recommend explaining it in documentation.
	- Using "Primary" as a key in the database is cumbersome and prone to mistakes because "Primary" is a keyword and needs to be escaped every time in order to access the value of the member."Primary" field. I recommend changing the name to something else.

## Task 5:
Add an endpoint to the accounts controller that will delete all members of a specified account except the "primary" member.
	- No notes.

