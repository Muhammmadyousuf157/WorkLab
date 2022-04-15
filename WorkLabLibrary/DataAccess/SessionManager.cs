using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WorkLabLibrary.Connections;
using WorkLabLibrary.Models;

namespace WorkLabLibrary.DataAccess
{
	public class SessionManager
	{
        public static async Task<SessionFile> SaveSession(string emailAddress, string startDateTime, string sessionKey, string fileName, string type)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.QuerySingleOrDefaultAsync<SessionFile>("spSaveSessionFile", new { emailAddress, startDateTime, sessionKey, fileName, type }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task SaveParticipant(string userName, string sessionKey)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spSaveParticipant", new { userName, sessionKey }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task EndSession(string emailAddress, string endDateTime, string sessionKey)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spEndSession", new { emailAddress, endDateTime, sessionKey }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<List<Session>> GetSessions(string emailAddress)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return (await db.QueryAsync<Session>("spGetSessions", new { emailAddress }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false)).ToList();
        }

        public static async Task UpdateFileTitle(int fileId, string fileTitle)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spUpdateFileTitle", new { fileId, fileTitle }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task DeleteSession(int sessionId)
        {
            using IDbConnection db = DbConnection.GetConnection();

            await db.ExecuteAsync("spDeleteSession", new { sessionId }, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<SessionFile> GetSessionFile(int fileId)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.QuerySingleOrDefaultAsync<SessionFile>("spGetSessionFile", new { fileId}, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

        public static async Task<string> GetSessionFileTitle(string sessionKey)
        {
            using IDbConnection db = DbConnection.GetConnection();

            return await db.ExecuteScalarAsync<string>("spGetSessionFileTitle", new { sessionKey}, commandType: CommandType.StoredProcedure)
                .ConfigureAwait(false);
        }

    }
}