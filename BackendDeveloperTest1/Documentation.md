
# Documentation
This file documents notes and requested recommendations for the different tasks involved in this interview assessment project.

## Task 1: 
Create an accounts controller with list, create, read, update and delete endpoints.
	- UID of locations was not an element in the LocationDto. Accounts are associated to locations by the location's UID. This causes an issue when creating a new account, as the location's UID is required but can't be easily accessed. Added UID as a field in the LocationDto class as a workaround.
	- It is unclear what the accounts PendCancel field is or how it's used.

## Task 2:
Enhance the location's list endpoint to return the number of non-cancelled accounts (where the `account.Status` < [CANCELLED](Test1/Models/AccountStatusType.cs)) for each location.
	- Enhancing the list endpoint's output involved modifying the LocationDto, which necessitated a similar enhancement for the read endpoint (getByID) in order to avoid showing an incorrect "accounts: 0" in the results.

