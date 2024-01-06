using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Dapper;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.ElasticSearch.Services;
using ESD_EDI_BE.ElasticSearch.Services.Machine;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace ESD_EDI_BE.ElasticSearch.Host
{
    public class InitializeElasticSearchService : IHostedService
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly ESD_DBContext _esdContext;
        private readonly IES_MachineService _esMachineService;
        private readonly ISqlDataAccess _sqlDataAccess;

        public InitializeElasticSearchService(
            IServiceProvider serviceProvider
            , IES_MachineService esMachineService
            , ESD_DBContext eSD_DBContext
            , ISqlDataAccess sqlDataAccess

        )
        {
            _serviceProvider = serviceProvider;
            _esdContext = eSD_DBContext;
            _esMachineService = esMachineService;
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                await IndexDataToElasticsearch();
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task IndexDataToElasticsearch()
        {
            // Thực hiện lập chỉ mục dữ liệu vào Elasticsearch
            // Ví dụ: Indexing mọi ESDMachine từ cơ sở dữ liệu vào Elasticsearch
            await _esMachineService.DeleteIndexIfExist("machine");

            var allMachines = await GetAllMachinesFromDatabase();
            try
            {
                if (allMachines.Any())
                {
                    // var machines = new HashSet<MachineDto>();
                    // var updateMachines = new HashSet<MachineDto>();

                    // Asynchronously check existence in parallel
                    // var tasks = allMachines.Select(async machine => new
                    // {
                    //     Machine = machine,
                    //     Exists = await _esMachineService.IsExisted(machine)
                    // });

                    // var results = await Task.WhenAll(tasks);

                    // foreach (var result in allMachines)
                    // {
                    //     if (!result.Exists)
                    //     {
                    //         insertMachines.Add(result.Machine);
                    //     }
                    //     else
                    //     {
                    //         updateMachines.Add(result.Machine);
                    //     }
                    // }

                    // if (insertMachines.Any())
                    // {
                    //     await _esMachineService.BulkInsert(insertMachines);
                    // }
                    // if (updateMachines.Any())
                    // {
                    //     await _esMachineService.BulkUpdate(updateMachines);
                    // }

                    await _esMachineService.BulkInsert(allMachines);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("connect to elastic search: " + e.Message);
            }
        }

        private async Task<HashSet<MachineDto>?> GetAllMachinesFromDatabase()
        {
            // return await _esdContext.ESDMachine.ToListAsync();
            var proc = $"Usp_Machine_Get";
            var param = new DynamicParameters();
            param.Add("@page", 1);
            param.Add("@pageSize", int.MaxValue);
            param.Add("@keyword", null);
            param.Add("@isActived", null);
            param.Add("@totalRow", 0, DbType.Int32, ParameterDirection.Output);
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<MachineDto>(proc, param);

            return data.ToHashSet();
        }
    }
}