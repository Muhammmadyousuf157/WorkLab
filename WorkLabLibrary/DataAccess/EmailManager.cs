using WorkLabLibrary.Connections;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace WorkLabLibrary.DataAccess
{
    public static class EmailManager
    {
        public static async Task<Guid> GetConfirmationToken(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<Guid>("spGetConfirmationToken", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<bool> IsEmailConfirmed(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<bool>("spIsEmailConfirmed", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task VerifyEmailAddress(string token)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spVerifyEmailAddress", new { token }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }
    }
}