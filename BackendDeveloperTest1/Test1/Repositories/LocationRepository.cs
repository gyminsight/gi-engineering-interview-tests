using Dapper;
using System;
using System.Threading;
using Test1.Contracts;
using Test1.Core;
using Test1.Interfaces;
using Test1.Models;
using static Test1.Controllers.LocationsController;

namespace Test1.Repositories
{
    public class LocationRepository : IReadOnlyRepository<Location>
    {
        /// <summary>
        /// Retrieves all location records from the database.
        /// </summary>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>Enumerable collection of Location entities with all location records from the database.</returns>
        public async Task<IEnumerable<Location>> GetAllAsync(DapperDbContext dbContext)
        {
            try
            {
                const string sql = @" SELECT    l.Guid,
                                                l.UID,
                                                l.CreatedUtc,
                                                l.UpdatedUtc,
                                                l.Disabled,
                                                l.EnableBilling,
                                                l.AccountStatus,
                                                l.Name,
                                                l.Address,
                                                l.City,
                                                l.Locale,
                                                l.PostalCode

                                                FROM location l
                                                ;";

                var builder = new SqlBuilder();

                var template = builder.AddTemplate(sql);

                var rows = await dbContext.Session.QueryAsync<Location>(template.RawSql, template.Parameters, dbContext.Transaction)
                    .ConfigureAwait(false);

                return rows;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific location record from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the location to retrieve.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>Location entity with matching identifier, or null if not found.</returns>
        public async Task<Location> GetByIdAsync(Guid id, DapperDbContext dbContext)
        {
            try
            {
                const string sql = @" SELECT    l.Guid,
                                                l.UID,
                                                l.CreatedUtc,
                                                l.UpdatedUtc,
                                                l.Disabled,
                                                l.EnableBilling,
                                                l.AccountStatus,
                                                l.Name,
                                                l.Address,
                                                l.City,
                                                l.Locale,
                                                l.PostalCode

                                                FROM location l
                                                WHERE l.Guid = @Guid
                                                ;";

                var builder = new SqlBuilder();

                builder.Where("Guid = @gUid", new
                {
                    Guid = id
                });

                var template = builder.AddTemplate(sql);

                var row = await dbContext.Session.QueryFirstOrDefaultAsync<Location>(template.RawSql, template.Parameters, dbContext.Transaction)
                    .ConfigureAwait(false);

                return row;
            }
            catch
            {
                throw;
            }
        }
    }
}
