namespace ESD_EDI_BE.Connection
{
    public class ConnectionModel
    {
        public string RedisConnectionString { get; set; }
        public string SQLConnectionString { get; set; }
        public string ESD_ConnectionStr { get; set; }
        public string AUTONSI_ConnectionStr { get; set; }
        public string ElasticSearchConnectionString { get; set; }

        public ConnectionModel()
        {
            RedisConnectionString = string.Empty;
            SQLConnectionString = string.Empty;
            ESD_ConnectionStr = string.Empty;
            AUTONSI_ConnectionStr = string.Empty;
            ElasticSearchConnectionString = string.Empty;
        }
    }
}
