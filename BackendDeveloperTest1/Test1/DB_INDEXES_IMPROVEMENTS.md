# Database/Table Index Recommendations

## Things I've Noticed

Right now the database only has indexes on the UID columns (the auto-increment primary keys). 

From what I understand, this means every time we look up an account by GUID or join tables together, the database has to scan through every row to find what it's looking for.

This seems fine with the current 70 accounts but I think it might get slow if we had thousands of records.


## Recommended Indexes

**GUID columns**

Every API call uses GUIDs to find things, like when you do `GET /api/accounts/{guid}`. 

Without an index, the database probably checks every row until it finds a match. 

Could add unique indexes on:
- location.Guid
- account.Guid
- member.Guid


**Foreign key columns**

We join on these columns frequently, so indexing them could help with performance:
- account.LocationUid (for joining to locations)
- member.AccountUid (for joining to accounts)
- member.LocationUid (for joining to locations)



**Status column**

The location list filters by status when counting non-cancelled accounts. An index on `account.Status` might speed that up.


**Primary member lookups**

When creating or deleting members, the code checks if a primary member exists. An index on `member.AccountUid` and `member.Primary` together could make that check faster.


## Impact

Without indexes, finding an account by GUID probably checks all 70+ rows every time and joining accounts to locations seems to scan both full tables. This could get worse as more data gets added.

With indexes, GUID lookups would be faster since the index points directly to the row and JOINs could use indexed foreign keys instead of scanning. Performance might stay more consistent with larger datasets.


## Note on GUIDs


From my experience working with database migration, I've seen that GUIDs are good for security and making sure IDs don't collide across different systems. 

The downside is they're random instead of sequential which can make indexes less efficient than regular auto-incrementing IDs.

For this application, since everything uses GUIDs, having proper indexes on them is important to keep lookups fast.