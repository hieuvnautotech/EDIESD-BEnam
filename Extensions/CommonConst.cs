namespace ESD_EDI_BE.Extensions
{
    public static class CommonConst
    {
        /// <summary>
        /// Redis keys
        /// </summary>
        public const string COMPANY = "ESD";
        public const string NON_EXIST_TOKEN = "NON_EXIST_TOKEN";
        public const string CACHE_KEY_POLICIES = $"{COMPANY}_POLICIES";
        public const string CACHE_AVAILABLE_TOKENS = $"{COMPANY}_AVAILABLE_TOKENS";
        public const string CACHE_KEY_ROLE_MENU = $"{COMPANY}_ROLE_MENU";
        public const string CACHE_KEY_ROLE_PERMISSIONS = $"{COMPANY}_ROLE_PERMISSIONS";
        public const string CACHE_KEY_ROLE_MISSING_MENU_PERMISSION = $"{COMPANY}_ROLE_MISSING_MENU_PERMISSION";

        /// <summary>
        /// Environment
        /// </summary>
        public const string DEVELOPMENT = @"Development";
        public const string STAGING = @"Staging";
        public const string PRODUCTION = @"Production";

        /// <summary>
        /// SignalR
        /// </summary>
        public const string GROUP_ROLE = $"{COMPANY}_GROUP_ROLE";

        /// <summary>
        /// EDI folders
        /// </summary>
        public const string EDI = "EDI";
        public const string DATA = "DATA";
        public const string HMI = "HMI";
        public const string LOADCELL = "LoadCellData";
        public const string VISION = "VISION";
        public const string BACKUP = "BACKUP";
        public const string LOG = "LOG";

        /// <summary>
        /// RabbitMQ
        /// </summary>
        public const string RABBITMQ_TYPE_CREATE = "CREATE";
        public const string RABBITMQ_TYPE_UPDATE = "UPDATE";
        public const string RABBITMQ_TYPE_DELETE = "DELETE";

        public const string EXCHANGE = $"{COMPANY}_EDI";
        public const string QUEUE_MESSAGE = $"{COMPANY}_EDI_MESSAGE_QUEUE";
        public const string QUEUE_PPORTAL_QUAL02_INFO = $"{COMPANY}_EDI_PPORTAL_QUAL02_INFO_QUEUE";

        public const string AUTONSI_EXCHANGE = $"AUTONSI_{COMPANY}_EDI";
        public const string AUTONSI_QUEUE_MACHINE = $"AUTONSI_{COMPANY}_EDI_QUEUE_MACHINE";
        public const string AUTONSI_QUEUE_MENU = $"AUTONSI_{COMPANY}_EDI_QUEUE_MENU";
        public const string AUTONSI_QUEUE_MENU_PERMISSION = $"AUTONSI_{COMPANY}_EDI_QUEUE_MENU_PERMISSION";
        public const string AUTONSI_QUEUE_DOCUMENT = $"AUTONSI_{COMPANY}_EDI_QUEUE_DOCUMENT";

        /// <summary>
        /// Nothing
        /// </summary>
        public static string[] ENTITY_ASSEMBLY_NAME = new string[] { "Magic.Core", "Magic.Application", "Magic.FlowCenter" };
    }
}
