# Gym Management API

A REST web application for managing gym locations, accounts, and members using SQLite and Dapper.

## Quick Start

```bash
# Run the application
cd Test1
dotnet run

# The API will be available at http://localhost:8080
```

## API Endpoints

### Accounts (`/api/accounts`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/accounts` | List all accounts |
| GET | `/api/accounts/{id}` | Get account by GUID |
| POST | `/api/accounts` | Create new account |
| PUT | `/api/accounts/{id}` | Update account |
| DELETE | `/api/accounts/{id}` | Delete account (cascades to members) |
| GET | `/api/accounts/{id}/members` | Get all members for account |
| DELETE | `/api/accounts/{id}/members/non-primary` | Delete non-primary members |

### Members (`/api/members`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/members` | List all members |
| GET | `/api/members/{id}` | Get member by GUID |
| POST | `/api/members` | Create member |
| DELETE | `/api/members/{id}` | Delete member (promotes next if primary) |

### Locations (`/api/locations`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/locations` | List with active account counts |
| GET | `/api/locations/{id}` | Get location by GUID |
| POST | `/api/locations` | Create location |
| DELETE | `/api/locations/{id}` | Delete location |

### Infrastructure

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |

## Business Rules

### Members
- **First member is always primary**: When creating the first member on an account, they automatically become primary regardless of the request.
- **One primary per account**: Cannot create a second primary member. Request is rejected with `400 Bad Request`.
- **Cannot delete last member**: An account must always have at least one member.
- **Primary promotion**: When deleting the primary member, the oldest remaining member (by `CreatedUtc`) is promoted.

### Accounts
- **Active account count**: Status < `CANCELLED` (values 0, 1, 2) counts as active.
- **Cascade delete**: Deleting an account removes all associated members.

## Testing

```bash
# Run all tests
cd Test1.Tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
Test1/
├── Controllers/          # API endpoints
├── Middleware/           # Global exception handling
├── Models/               # DTOs, enums, response types
├── Contracts/            # Interfaces
├── Core/                 # Database infrastructure
├── data/                 # SQLite database
└── Program.cs            # Application startup
```

## Documentation

- **[RECOMMENDATIONS.md](RECOMMENDATIONS.md)** - Index and code improvement recommendations (Tasks 6 & 7)
- **[CODE_REVIEW_AND_TRADEOFFS.md](CODE_REVIEW_AND_TRADEOFFS.md)** - Detailed implementation analysis

---

## Original Assessment Instructions

For this project, you'll want Visual Studio Code (or full Visual Studio, if you have it) and Postman.

You should fork the project to your own Github account, and perform the changes/additions on that fork.

Once completed, you can initiate a pull request back to this repository. We will then setup a follow-up video interview to screen-share the project, and review your changes with you.

### Overview
This project provides the skeleton of a "Gym Management" REST web application using SQLite and Dapper. The database is prepopulated with the following entities:
* __location__ - A table containing gym locations.
* __account__ - A table containing gym membership accounts. There is a many-to-one relationship between accounts and locations.
* __member__ - A table containing the members of accounts. There is a many-to-one relationship between members and accounts.

Verify your endpoints work by using Postman (https://www.postman.com/downloads/). The app will listen on port 8080.

### Tasks to complete

1. ✅ Create an accounts controller with list, create, read, update and delete endpoints.
2. ✅ Enhance the location's list endpoint to return the number of non-cancelled accounts (where the `account.Status` < [CANCELLED](Test1/Models/AccountStatusType.cs)) for each location.
3. ✅ Add an endpoint to the accounts controller that will return all members for a specified account (using the account's Guid)
4. ✅ Create a members controller that will list, create, and delete members.
   * ✅ The create endpoint should only allow for one primary member per account.
   * ✅ The delete endpoint should make the next member on the account the primary member, when the primary member is deleted for an account. The endpoint should not allow deletion of the last member on the account.
5. ✅ Add an endpoint to the accounts controller that will delete all members of a specified account except the "primary" member.
6. ✅ Document any recommendations on table indexes. → See [RECOMMENDATIONS.md](RECOMMENDATIONS.md)
7. ✅ Document any recommendations on improving the structure or maintainability of the code. → See [RECOMMENDATIONS.md](RECOMMENDATIONS.md)
