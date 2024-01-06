using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using static System.Net.Mime.MediaTypeNames;

namespace ESD_EDI_BE.Services.Common
{
    public interface IExpoTokenService
    {
        Task<ResponseModel<ExpoTokenDto>> Create(ExpoTokenDto model);
        Task<ResponseModel<IEnumerable<ExpoTokenDto>>> GetActive();
        Task<ResponseModel<VersionAppDto>> PushExpoNotification();
    }

    [SingletonRegistration] //dùng trong function firebase push notification
    [ScopedRegistration]
    public class ExpoTokenService : IExpoTokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly IVersionAppService _versionAppService;
        public ExpoTokenService(
             ISqlDataAccess sqlDataAccess
            , IHttpClientFactory httpClientFactory
            , IVersionAppService versionAppService
            )
        {
            _httpClientFactory = httpClientFactory;
            _sqlDataAccess = sqlDataAccess;
            _versionAppService = versionAppService;
        }

        public async Task<ResponseModel<ExpoTokenDto>> Create(ExpoTokenDto model)
        {
            var returnData = new ResponseModel<ExpoTokenDto>();
            string proc = "sysUsp_ExpoToken_Create";
            var param = new DynamicParameters();
            param.Add("@ExpoToken", model.ExpoToken);
            param.Add("@DeviceName", model.DeviceName);
            param.Add("@DeviceModelName", model.DeviceModelName);
            param.Add("@DeviceBrand", model.DeviceBrand);
            param.Add("@DeviceType", model.DeviceType);
            param.Add("@output", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);//luôn để DataOutput trong stored procedure

            returnData.ResponseMessage = await _sqlDataAccess.SaveDataUsingStoredProcedure<string>(proc, param);
            returnData.Data = new ExpoTokenDto()
            {
                ExpoToken = model.ExpoToken,
                isActived = true,
            };
            return returnData;

        }

        public async Task<ResponseModel<IEnumerable<ExpoTokenDto>>> GetActive()
        {

            var returnData = new ResponseModel<IEnumerable<ExpoTokenDto>>();
            string proc = "sysUsp_ExpoToken_GetActive";
            var data = await _sqlDataAccess.LoadDataUsingStoredProcedure<ExpoTokenDto>(proc);

            if (!data.Any())
            {
                returnData.HttpResponseCode = 204;
                returnData.ResponseMessage = StaticReturnValue.NO_DATA;
            }
            else
            {
                returnData.Data = data;
            }
            return returnData;
        }

        public async Task<ResponseModel<VersionAppDto>> PushExpoNotification()
        {
            var responseString = "general.success";
            var app = await _versionAppService.GetAppByCode("SOLUM_PDA");
            app.ResponseMessage = responseString;

            if (app != null && app.Data != null)
            {
                HttpClient _httpClient = _httpClientFactory.CreateClient("expoToken");

                var postData = new ExpoPushNotificationData();

                postData.data.body.update_type = app.Data.update_type;

                if(app.Data.update_type == 0)
                {
                    postData.data.message = "Quick Update";
                }

                var expoTokenList = await GetActive();

                if (expoTokenList.Data != null)
                {
                    foreach (var item in expoTokenList.Data)
                    {
                        postData.registration_ids.Add(item.ExpoToken);
                    }
                }

                var postDataJson = new StringContent(
                                        JsonSerializer.Serialize(postData),
                                        Encoding.UTF8,
                                        Application.Json); // using static System.Net.Mime.MediaTypeNames;

                postDataJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using var httpResponseMessage = await _httpClient.PostAsync("/fcm/send", postDataJson);

                try
                {
                    httpResponseMessage.EnsureSuccessStatusCode();
                    //app.Data.app_version = app.Data.CHPlay_version;
                    //responseString = await _versionAppService.Modify(app.Data);
                    app.ResponseMessage = responseString;

                    // Handle success
                }
                catch (HttpRequestException)
                {
                    // Handle failure
                    responseString = "general.system_error";
                    app.ResponseMessage = responseString;
                }
                finally { httpResponseMessage.Dispose(); }
            }
            return app;

            //// VERSION 1
            //HttpClient _httpClient = _httpClientFactory.CreateClient("expoToken");

            //var postData = new ExpoPushNotificationData();

            //var expoTokenList = await GetActive();

            //if (expoTokenList.Data != null)
            //{
            //    foreach (var item in expoTokenList.Data)
            //    {
            //        postData.registration_ids.Add(item.ExpoToken);
            //    }
            //}

            //var postDataJson = new StringContent(
            //                        JsonSerializer.Serialize(postData),
            //                        Encoding.UTF8,
            //                        Application.Json); // using static System.Net.Mime.MediaTypeNames;

            //postDataJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //await _httpClient.PostAsync("/fcm/send", postDataJson);


            //// VERSION 2
            //var httpResponseMessage = await client.PostAsync("/send", postData);
            //using var httpResponseMessage = await _httpClient.PostAsync("/fcm/send", postDataJson);

            //var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

            //var result = httpResponseMessage.Result;

            //httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}
