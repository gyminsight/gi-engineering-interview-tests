# Task 6: Recommendations on Table Indexes

## Considerations

- Users interact with entities via GUIDs
- Internal logic uses integer UIDs

Upon my research SQLite does not automatically index foreign keys or non‑PK columns, so we should add the right indexes to improve performance.

## Indexes by table

### Location Table

**Recommended**
- Guid. Because this is the Public identifier for API calls. The solution exposes GUIDs to users and the system will perform many search lookups using this field.

**Optional**
- Name, Disabled, and AccountStatus could be also good candidates if we need to filter lists by them.

### Account Table

**Recommended**
- Guid. Because this is the Public identifier for API calls. The solution exposes GUIDs to users and the system will perform many search lookups using this field.
- LocationUid. Foreign key used in joins and filtering accounts by location.

**Optional**
- Status and NextBillingUtc. Could be used in filtering accounts and could be used frequently in scheduled tasks.

### Member Table

**Recommended**
- Guid. Because this is the Public identifier for API calls. The solution exposes GUIDs to users and the system will perform many search lookups using this field.
- AccountUid. Foreign key used in joins and filtering members by account.
- LocationUid. Foreign key used in joins and filtering members by location.

**Optional**
- Cancelled. If we need to perform queries frequently based on the member status.
- LastName. If we need to search members by name frequently.

## Final thoughts

Indexes are really good to increase query performance, but we need to use them with control. Too many indexes can produce slow write speed, increased storage usage, and harder maintenance.