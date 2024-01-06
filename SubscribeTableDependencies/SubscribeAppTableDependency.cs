using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Models;
using TableDependency.SqlClient.Base.EventArgs;
using TableDependency.SqlClient;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.SubscribeTableDependencies
{
    //dùng trong function firebase push notification, mở comment
    public class SubscribeAppTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<sysTbl_Application> tableDependency;
        private readonly SignalRHub _signalRHub;
        private readonly IExpoTokenService _expoTokenService;
        public SubscribeAppTableDependency(
            SignalRHub signalRHub
            , IExpoTokenService expoTokenService
            )
        {
            _signalRHub = signalRHub;
            _expoTokenService = expoTokenService;
        }

        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<sysTbl_Application>(connectionString);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(sysTbl_Application)} SqlTableDependency error: {e.Error.Message}");
        }

        private async void TableDependency_OnChanged(object sender, RecordChangedEventArgs<sysTbl_Application> e)
        {
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                var app = await _expoTokenService.PushExpoNotification();

                //if (app.ResponseMessage == "general.success")
                //    await _signalRHub.SendAppVersion(app);
            }
        }
    }
}
