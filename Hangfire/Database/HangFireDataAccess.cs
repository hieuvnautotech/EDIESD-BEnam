using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Hangfire.Database
{
    public interface IHangfireDataAccess
    {

        #region Raw Query
        Task<IEnumerable<T>> ReadDataUsingRawQuery<T>(string connectionString, string rawQuery, DynamicParameters? parameters = null);
        Task<string> WriteDataUsingRawQuery<T>(string connectionString, string rawQuery, DynamicParameters? parameters = null);
        #endregion
    }

    [SingletonRegistration]
    public class HangfireDataAccess : IHangfireDataAccess
    {

        #region Raw Query
        //Used for getting gatas (select query) from database
        public async Task<IEnumerable<T>> ReadDataUsingRawQuery<T>(string connectionString, string rawQuery, DynamicParameters? parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(connectionString);

            return await dbConnection.QueryAsync<T>(rawQuery, parameters, transaction: null, commandTimeout: 20, commandType: CommandType.Text);
        }

        public async Task<string> WriteDataUsingRawQuery<T>(string connectionString, string rawQuery, DynamicParameters? parameters = null)
        {
            string result = string.Empty;
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                if (dbConnection.State == ConnectionState.Closed) dbConnection.Open();
                using IDbTransaction tran = dbConnection.BeginTransaction();
                try
                {
                    await dbConnection.ExecuteAsync(rawQuery, parameters, transaction: tran, commandTimeout: 20, commandType: CommandType.Text);
                    tran.Commit();
                    result = parameters.Get<string?>("@output") ?? string.Empty;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    tran.Rollback();
                }
            }

            return result;
        }
        #endregion
    }
}