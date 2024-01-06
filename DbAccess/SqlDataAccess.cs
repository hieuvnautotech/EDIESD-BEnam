using Dapper;
using ESD_EDI_BE.Cache;
using ESD_EDI_BE.Connection;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.DbAccess
{
    public interface ISqlDataAccess
    {

        #region Stored Procedure
        Task<IEnumerable<T>> LoadDataUsingStoredProcedure<T>(string storedProcedure, DynamicParameters? parameters = null);
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> LoadMultiDataSetUsingStoredProcedure<T1, T2>(string storedProcedure, DynamicParameters? parameters = null);
        Task<string> SaveDataUsingStoredProcedure<T>(string storedProcedure, DynamicParameters parameters);
        #endregion

        #region Raw Query
        Task<IEnumerable<T>> LoadDataUsingRawQuery<T>(string rawQuery, DynamicParameters? parameters = null);
        #endregion
    }

    [SingletonRegistration]
    public class SqlDataAccess : ISqlDataAccess
    {
        //private readonly IConfiguration _configuration;
        // private static readonly string connectionString = ConnectionString.CONNECTIONSTRING;

        private readonly string connectionString;

        public SqlDataAccess(
            // IConfiguration configuration,
            IOptions<ConnectionModel> options
        )
        {
            //    _configuration = configuration;

            //// connection trong file appsetting.json
            connectionString = options.Value.SQLConnectionString;

            //// connection trong biến môi trường của Windows - tạo biến môi trường cho từng máy
            // connectionString = Environment.GetEnvironmentVariable("HANLIM_CONNECTION");
        }

        #region Stored Procedure
        //Used for getting gatas (select query) from database
        public async Task<IEnumerable<T>> LoadDataUsingStoredProcedure<T>(string storedProcedure, DynamicParameters? parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(connectionString);
            try
            {
                return await dbConnection.QueryAsync<T>(storedProcedure, parameters, transaction: null, commandTimeout: 20, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> LoadMultiDataSetUsingStoredProcedure<T1, T2>(string storedProcedure, DynamicParameters? parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(connectionString);

            using var multi = await dbConnection.QueryMultipleAsync(storedProcedure, parameters, transaction: null, commandTimeout: 20, commandType: CommandType.StoredProcedure);
            var result1 = await multi.ReadAsync<T1>();
            var result2 = await multi.ReadAsync<T2>();
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(result1, result2);
        }

        //Used for saving datas (insert/update/delete) into database
        public async Task<string> SaveDataUsingStoredProcedure<T>(string storedProcedure, DynamicParameters parameters)
        {
            string result = string.Empty;
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                if (dbConnection.State == ConnectionState.Closed) dbConnection.Open();
                using IDbTransaction tran = dbConnection.BeginTransaction();
                try
                {
                    await dbConnection.ExecuteAsync(storedProcedure, parameters, transaction: tran, commandTimeout: 20, commandType: CommandType.StoredProcedure);
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

        #region Raw Query
        //Used for getting gatas (select query) from database
        public async Task<IEnumerable<T>> LoadDataUsingRawQuery<T>(string rawQuery, DynamicParameters? parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(connectionString);

            return await dbConnection.QueryAsync<T>(rawQuery, parameters, transaction: null, commandTimeout: 20, commandType: CommandType.Text);
        }
        #endregion
    }
}
