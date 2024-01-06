using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESD_EDI_BE.Connection;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Hangfire.Database;
using ESD_EDI_BE.Hangfire.Models;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.Hangfire.Services
{
    public interface IHangfireService
    {
        Task<string> DeleteExpiredTokens();
        Task<string> UpdateDocumentFromAutonsi();
    }

    [ScopedRegistration]
    public class HangfireService : IHangfireService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string autonsiConnectionStr;
        private readonly string esdConnectionStr;
        private readonly IHangfireDataAccess _hangfireDataAccess;
        private readonly HangfireAutonsiContext _autonsiContext;
        private readonly ESD_DBContext _esdContext;
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly ISysCacheService _sysCacheService;

        public HangfireService(
            IWebHostEnvironment webHostEnvironment
            , IOptions<ConnectionModel> options
            , IHangfireDataAccess hangfireDataAccess
            , HangfireAutonsiContext autonsiContext
            , ESD_DBContext esdContext
            , ISqlDataAccess sqlDataAccess
            , ISysCacheService sysCacheService
        )
        {
            _webHostEnvironment = webHostEnvironment;
            autonsiConnectionStr = options.Value.AUTONSI_ConnectionStr;
            esdConnectionStr = options.Value.ESD_ConnectionStr;

            _autonsiContext = autonsiContext;
            _esdContext = esdContext;
            _hangfireDataAccess = hangfireDataAccess;
            _sqlDataAccess = sqlDataAccess;
            _sysCacheService = sysCacheService;
        }

        public async Task<string> DeleteExpiredTokens()
        {
            string result;
            int rowsDeleted = 0;
            try
            {
                string proc = "sysUsp_RefreshToken_DeleteExpiredTokens";
                var param = new DynamicParameters();
                param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);
                param.Add("@rowsDeleted", dbType: DbType.Int32, direction: ParameterDirection.Output, size: int.MaxValue);
                result = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
                rowsDeleted = param.Get<int?>("@rowsDeleted") ?? 0;
                await _sysCacheService.SetAvailableTokensToRedis();
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            // TODO: Save log before return
            if (rowsDeleted > 0 || result != StaticReturnValue.SUCCESS)
            {
                string deleteExpiredTokensLog = Path.Combine(_webHostEnvironment.WebRootPath, "HangfireLog", "DeleteExpiredTokens.txt");

                string logMessage = $"{DateTime.Now:yyyy-MM-dd(HH:mm:ss)}***Số lượng token đã xóa: {rowsDeleted}***{result}";
                CommonFunction.SaveHangfireLog(deleteExpiredTokensLog, logMessage);
            }

            return result;
        }

        // public async Task<string> DeleteExpiredTokens()
        // {
        //     string result = StaticReturnValue.SUCCESS;

        //     try
        //     {
        //         // Use LINQ to filter records based on the condition
        //         var recordsToDelete = await _esdContext.sysTbl_RefreshToken
        //             .Where(x => x.expiredDate < DateTime.Now)
        //             .ToListAsync(); // Materialize the query to a list

        //         if (recordsToDelete.Any())
        //         {
        //             // Remove the matching records from the DbSet
        //             _esdContext.sysTbl_RefreshToken.RemoveRange(recordsToDelete);

        //             // Save changes to the database
        //             await _esdContext.SaveChangesAsync();
        //             await _sysCacheService.SetAvailableTokensToRedis();
        //         }
        //     }
        //     catch (Exception e)
        //     {

        //         result = e.Message;
        //     }
        //     return result;
        // }

        // public async Task<string> DeleteExpiredTokens()
        // {
        //     string result;
        //     try
        //     {
        //         string query = @"
        //                             DELETE FROM dbo.sysTbl_RefreshToken
        //                                 WHERE expiredDate < GETDATE();
        //                             SET @output = 'general.success';
        //                         ";
        //         var param = new DynamicParameters();
        //         param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue, value: null);
        //         await _hangfireDataAccess.WriteDataUsingRawQuery<string>(autonsiConnectionStr, query, param);
        //         await _sysCacheService.SetAvailableTokensToRedis();

        //         result = param.Get<string?>("@output") ?? string.Empty;

        //     }
        //     catch (Exception e)
        //     {
        //         result = e.Message;
        //     }
        //     return result;
        // }

        public async Task<string> UpdateDocumentFromAutonsi()
        {
            string result = StaticReturnValue.SUCCESS;
            if (_webHostEnvironment.EnvironmentName == CommonConst.PRODUCTION)
            {
                var recordsToUpdate = await _autonsiContext.Document
                        .Where(x => x.transToCustomer == false)
                        .ToListAsync(); // Materialize the query to a list
                try
                {
                    // Use LINQ to filter records based on the condition
                    if (recordsToUpdate.Any())
                    {
                        List<sysTbl_Document> insertList = new();
                        List<sysTbl_Document> updateList = new();
                        foreach (var item in recordsToUpdate)
                        {
                            var data = await _esdContext.sysTbl_Document.FirstOrDefaultAsync(x => x.documentId == item.documentId);
                            if (data != null)
                            {
                                updateList.Add(data);
                            }
                            else
                            {
                                insertList.Add(AutoMapperConfig<Document, sysTbl_Document>.Map(item));
                            }
                            item.transToCustomer = true;
                        }
                        // Remove the matching records from the DbSet
                        _esdContext.sysTbl_Document.UpdateRange(updateList);
                        _esdContext.sysTbl_Document.AddRange(insertList);

                        // Save changes to the database
                        await _esdContext.SaveChangesAsync();
                        await _autonsiContext.SaveChangesAsync();

                        // var tasks = recordsToUpdate.Select(async item =>
                        // {
                        //     var data = await _esdContext.sysTbl_Document.FirstOrDefaultAsync(x => x.documentId == item.documentId);
                        //     if (data != null)
                        //     {
                        //         updateList.Add(data);
                        //     }
                        //     else
                        //     {
                        //         insertList.Add(AutoMapperConfig<Document, sysTbl_Document>.Map(item));
                        //     }
                        //     item.transToCustomer = true;
                        // });

                        // await Task.WhenAll(tasks);

                        // // Batch updates
                        // _esdContext.sysTbl_Document.UpdateRange(updateList);
                        // _esdContext.sysTbl_Document.AddRange(insertList);

                        // // Save changes to the database
                        // await _esdContext.SaveChangesAsync();
                        // await _autonsiContext.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    result = e.Message;
                }

                // TODO: Save log before return
                if (recordsToUpdate.Count > 0 || result != StaticReturnValue.SUCCESS)
                {
                    string updateDocumentFromAutonsiLog = Path.Combine(_webHostEnvironment.WebRootPath, "HangfireLog", "UpdateDocumentFromAutonsi.txt");

                    string logMessage = $"{DateTime.Now:yyyy-MM-dd(HH:mm:ss)}***Số lượng record thay đổi: {recordsToUpdate.Count}***{result}";
                    CommonFunction.SaveHangfireLog(updateDocumentFromAutonsiLog, logMessage);
                }
            }

            return result;
        }
    }
}