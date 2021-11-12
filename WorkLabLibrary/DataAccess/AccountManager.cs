using WorkLabLibrary.Connections;
using WorkLabLibrary.Models;
using Dapper;
using System.Data;
using System.Threading.Tasks;
using System;

namespace WorkLabLibrary.DataAccess
{
    public static class AccountManager
    {
        public static async Task<int> UserSignUp(User user)
        {
            var parameters = new
            {
                user.FirstName,
                user.LastName,
                user.UserName,
                user.EmailAddress,
                user.PasswordText
            };

            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<int>("spUserSignUp", parameters, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task UserExternalSignUp(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spUserExternalSignUp", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<User> UserSignIn(string emailAddress, string passwordText)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.QuerySingleOrDefaultAsync<User>("spUserSignIn", new { emailAddress, passwordText }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<bool> IsUniqueEmailAddress(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<bool>("spValidateEmailAddress", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<bool> IsUniqueUserName(string userName)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<bool>("spValidateUserName", new { userName }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<Guid?> GetForgotPasswordToken(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<Guid?>("spGetForgotPasswordToken", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task ResetPassword(string passwordText, string token)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spResetPassword", new { passwordText, token }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }
    }
}