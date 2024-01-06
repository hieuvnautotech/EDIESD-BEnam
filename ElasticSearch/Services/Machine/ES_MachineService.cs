using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using ESD_EDI_BE.Connection;
using ESD_EDI_BE.Models.Dtos;
using Microsoft.Extensions.Options;
using Nest;

namespace ESD_EDI_BE.ElasticSearch.Services.Machine
{
    public interface IES_MachineService
    {
        Task DeleteIndexIfExist(string indexName);
        Task<bool> IsExisted(MachineDto machine);
        Task Insert(MachineDto machine);
        Task BulkInsert(HashSet<MachineDto> machines);
        Task Update(MachineDto machine);
        Task BulkUpdate(HashSet<MachineDto> machines);
        Task Delete(MachineDto machine);
        Task<HashSet<MachineDto>?> Search(string searchTerm, bool? isActived);
    }

    [Extensions.ServiceExtensions.SingletonRegistration]
    public class ES_MachineService : IES_MachineService
    {
        private readonly ElasticClient _elasticClient;

        public ES_MachineService
        (
            IOptions<ConnectionModel> options
        )
        {
            var conn = options.Value.ElasticSearchConnectionString;

            var settings = new ConnectionSettings(new Uri(conn)).DefaultIndex("machine");

            _elasticClient = new ElasticClient(settings);
        }

        public async Task DeleteIndexIfExist(string indexName)
        {
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName);

            if (indexExistsResponse.IsValid && indexExistsResponse.Exists)
            {
                var deleteIndexResponse = await _elasticClient.Indices.DeleteAsync(indexName);

                if (deleteIndexResponse.IsValid)
                {
                    Console.WriteLine($"Index '{indexName}' deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to delete index '{indexName}': {deleteIndexResponse.ServerError?.Error}");
                }
            }
            else
            {
                Console.WriteLine($"Index '{indexName}' does not exist.");
            }
        }

        public async Task<bool> IsExisted(MachineDto machine)
        {
            var documentExistsResponse = await _elasticClient.DocumentExistsAsync(new DocumentExistsRequest("machine", machine.MachineId));

            if (documentExistsResponse.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task Insert(MachineDto machine)
        {
            try
            {
                var insertResponse = await _elasticClient.IndexAsync(machine, u => u
                            .Id(machine.MachineId)
                            .Refresh(Refresh.True)
                        );
                if (insertResponse.IsValid)
                {
                    Console.WriteLine("Insert machine to ElasticSearch succeeded.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Insert machine to ElasticSearch: " + e.Message);
            }
        }

        public async Task BulkInsert(HashSet<MachineDto> machines)
        {
            var bulkDescriptor = new BulkDescriptor();
            foreach (var machine in machines)
            {
                bulkDescriptor.Index<MachineDto>(op => op
                    .Document(machine)
                    .Id(machine.MachineId) // Specify the document ID
                );
            }
            var bulkResponse = await _elasticClient.BulkAsync(bulkDescriptor);

            if (bulkResponse.IsValid)
            {
                Console.WriteLine("Bulk insert successful!");
            }
            else
            {
                Console.WriteLine($"Failed to perform bulk insert: {bulkResponse.ServerError?.Error}");
            }
        }

        public async Task Update(MachineDto machine)
        {
            try
            {
                var updateResponse = await _elasticClient.UpdateAsync<MachineDto>(machine.MachineId, u => u
                            .Doc(machine)
                            .Refresh(Refresh.True)
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

        public async Task Delete(MachineDto machine)
        {
            await _elasticClient.DeleteAsync<MachineDto>(machine.MachineId, d => d.Refresh(Refresh.True));
        }

        public async Task<HashSet<MachineDto>?> Search(string searchTerm, bool? isActived)
        {
            var searchResponse = await _elasticClient.SearchAsync<MachineDto>(s => s
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
                // .Scroll("5m") // Set the scroll timeout (e.g., 5 minutes)
                .Size(1000) // Set the batch size (e.g., 1000 documents per batch)
            );

            // while (searchResponse.IsValid && searchResponse.Documents.Any())
            // {
            //     // Process the documents in the current batch


            //     // Continue scrolling
            //     searchResponse = await _elasticClient.ScrollAsync<MachineDto>("5m", searchResponse.ScrollId);
            // }

            // // Clear the scroll to release resources on the server
            // var clearScrollResponse = await _elasticClient.ClearScrollAsync(s => s.ScrollId(searchResponse.ScrollId));

            // if (!clearScrollResponse.IsValid)
            // {
            //     Console.WriteLine($"Failed to clear scroll: {clearScrollResponse.ServerError?.Error}");
            // }

            if (searchResponse.IsValid)
            {
                var result = searchResponse.Documents.ToHashSet();
                return result;
            }
            return null;
        }

        public Task BulkUpdate(HashSet<MachineDto> machines)
        {
            throw new NotImplementedException();
        }


    }
}