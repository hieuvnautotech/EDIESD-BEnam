using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Models;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace ESD_EDI_BE.SubscribeTableDependencies
{
    public class SubscribeMenuTableDependency : ISubscribeTableDependency
    {

        SqlTableDependency<sysTbl_Menu> tableDependency;
        private readonly SignalRHub _signalRHub;
        public SubscribeMenuTableDependency(SignalRHub signalRHub)
        {
            _signalRHub = signalRHub;
        }

        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<sysTbl_Menu>(connectionString);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(sysTbl_Menu)} SqlTableDependency error: {e.Error.Message}");
        }

        private async void TableDependency_OnChanged(object sender, RecordChangedEventArgs<sysTbl_Menu> e)
        {
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                // await _signalRHub.SendMenuUpdate();
            }
        }
    }
}
