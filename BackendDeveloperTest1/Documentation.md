
# Documentation
This file documents notes and requested recommendations for the different tasks involved in this interview assessment project.

## Task 1: 
Create an accounts controller with list, create, read, update and delete endpoints.
Notes:
	- UID of locations was not an element in the LocationDto. Accounts are associated to locations by the location's UID. This causes an issue when creating a new account, as the location's UID is required but can't be easily accessed. Added UID as a field in the LocationDto class as a workaround.
	- It is unclear what the account.PendCancel field is or how it's used. I recommend explaining it in documentation.

## Task 2:
Enhance the location's list endpoint to return the number of non-cancelled accounts (where the `account.Status` < [CANCELLED](Test1/Models/AccountStatusType.cs)) for each location.
Notes:
	- Enhancing the list endpoint's output involved modifying the LocationDto, which necessitated a similar enhancement for the read endpoint (getByID) in order to avoid showing an incorrect "accounts: 0" in the results.

## Task 3:
Add an endpoint to the accounts controller that will return all members for a specified account (using the account's Guid)
Notes:
	- Creating a Members controller and MemberDto class to make handling members easier
	- Note that member.AccountUID and member.LocationUID are different types than account.UID and location.UID (unsigned int vs integer). This might cause problems if there are a very large amount of accounts or locations.

## Task 4:
Create a members controller that will list, create, and delete members. 
   * The create endpoint should only allow for one primary member per account.
   * The delete endpoint should make the next member on the account the primary member, when the primary member is deleted for an account. The endpoint should not allow deletion of the last member on the account.
Notes:
	- Keeping track of whether a member is primary inside each member is prone to mistakes and overhead. I recommend making a new relationship table to keep track of what the primary member is for each account.
	- Keeping redundant data about the location inside each member entry *and* each account entry might lead to possible source-of-truth conflicts if a member's location and the member's account's location become different (unless that is somehow intentional). Unless it's intentional, I recommend keeping track of the information only in one place and accessing it by joining queries. If it *is* intentional, I recommend explaining it in documentation.
	- Using "Primary" as a key in the database is cumbersome and prone to mistakes because "Primary" is a keyword and needs to be escaped every time in order to access the value of the member."Primary" field. I recommend changing the name to something else.

## Task 5:
Add an endpoint to the accounts controller that will delete all members of a specified account except the "primary" member.
Notes:
	- No notes.

## Task 6:
Document any recommendations on table indexes.
Notes:
	- Note that account.LocationUid, member.AccountUid, and member.LocationUid are different types than account.UID and location.UID (unsigned int vs integer). This might cause problems if there are a very large amount of accounts or locations. I recommend going through all the foreign keys and ensuring that they have the same type on both sides of the relationship.
	- It's unclear why the locations table has an AccountStatus field.
	- It is unclear what the account.PendCancel field is or how it's used. I recommend explaining it in documentation.
	- Using "Primary" as a key in the database is cumbersome and prone to mistakes because "Primary" is a keyword and needs to be escaped every time in order to access the value of the member."Primary" field. I recommend changing the name to something else like "MainMember".
	- Keeping track of whether each member is primary inside that member is prone to mistakes. I recommend making a new relationship table to keep track of what the primary member is for each account.
	- Keeping redundant data about the location inside each member entry *and* each account entry might lead to possible source-of-truth conflicts if a member's location and the member's account's location become different (unless that is somehow intentional). Unless it's intentional, I recommend keeping track of the information only in one place (either solely in the account, or solely in each member, depending on the intent) and accessing it by joining queries. If it *is* intentional, I recommend explaining the intention in documentation.

## Task 7:
Document any recommendations on improving the structure or maintainability of the code.
Notes:
	- It would be easier to ensure that the endpoints are working properly with a full collection of unit tests. I provided a collection of empty tests in the Test1.Tests project set in an appropriately descriptive naming standard. As the Test1.Tests project project did not come equipped with packages specific to the main Test1 project, I assumed that writing tests was not a part of this interview assessment, and did not spend time going deep and writing them. In a real-life scenario where I had more time and was not competing with a number of other candidates, it would be better practice to write the tests first in order to help ensure the code complies with the design. I would recommend completing the empty unit test that I set out.

