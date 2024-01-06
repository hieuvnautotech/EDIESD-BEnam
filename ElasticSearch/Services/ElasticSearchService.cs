using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Elastic.Clients.Elasticsearch;
//using Elastic.Clients.Elasticsearch.QueryDsl;
using ESD_EDI_BE.Connection;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Models;
using ESD_EDI_BE.Models.Dtos;
using Microsoft.Extensions.Options;
using Nest;

using static ESD_EDI_BE.Extensions.ServiceExtensions;

namespace ESD_EDI_BE.ElasticSearch.Services
{
    public interface IElasticSearchService
    {
        Task IndexMachine(MachineDto machine);
        Task DeleteMachine(MachineDto machine);
        Task<List<MachineDto>?> SearchMachines(string searchTerm, bool? isActived);
    }

    [SingletonRegistration]
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticClient _elasticClient;
        // private readonly Uri elasticsearchUri = new("http://elastic_search:9200");

        public ElasticSearchService
        (
            IOptions<ConnectionModel> options
        )
        {
            Uri elasticsearchUri = new(options.Value.ElasticSearchConnectionString);
            // var settings = new ConnectionSettings(elasticsearchUri)
            //     .DefaultIndex("machine"); // Elasticsearch index name

            _elasticClient = new ElasticClient(elasticsearchUri);
        }

        public async Task DeleteMachine(MachineDto machine)
        {
            await _elasticClient.DeleteAsync<MachineDto>(machine.MachineId, d => d
                .Index("machine")
            );
        }
        public async Task IndexMachine(MachineDto machine)
        {
            try
            {
                var updateResponse = await _elasticClient.UpdateAsync<MachineDto>(machine.MachineId, u => u
                            .Index("machine")
                            .Doc(machine)
                            .DocAsUpsert(true) // If the document doesn't exist, treat it as an insert
                        );
                if (updateResponse.IsValid)
                {
                    Console.WriteLine("Update document succeeded.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("connect to elastic search: " + e.Message);
            }
        }

        public async Task<List<MachineDto>?> SearchMachines(string searchTerm, bool? isActived)
        {
            //var searchRequest = new SearchRequest("machine")
            //{
            //    Query = new TermQuery("machineCode") { Value = searchTerm },
            //};

            var searchResponse = await _elasticClient.SearchAsync<MachineDto>(s => s
                .Index("machine")
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m
                            .Term(t => t.isActived, isActived)
                            , m => m
                            .MultiMatch(mt => mt
                                .Fields(fields => fields
                                    .Field(f => f.MachineCode)
                                    .Field(f => f.MachineName)
                                )
                                .Query(searchTerm)
                                .Fuzziness(Fuzziness.Auto)
                            )
                        )
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var result = searchResponse.Documents.ToList();
                return result;
            }
            return null;
        }
    }
}