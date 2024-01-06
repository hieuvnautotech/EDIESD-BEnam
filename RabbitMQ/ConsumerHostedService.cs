using ESD_EDI_BE.Extensions;

namespace ESD_EDI_BE.RabbitMQ
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IConsumerService _consumerService;
        private readonly IAutonsiConsumerService _autonsiConsumerService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ConsumerHostedService(
            IWebHostEnvironment webHostEnvironment
            , IConsumerService consumerService
            , IAutonsiConsumerService autonsiConsumerService
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _consumerService = consumerService;
            _autonsiConsumerService = autonsiConsumerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queues = new List<string> {
                    CommonConst.QUEUE_MESSAGE
                    , CommonConst.QUEUE_PPORTAL_QUAL02_INFO
                };

            var autonsiQueues = new List<string> {
                    CommonConst.AUTONSI_QUEUE_MACHINE
                    , CommonConst.AUTONSI_QUEUE_MENU
                    , CommonConst.AUTONSI_QUEUE_MENU_PERMISSION
                    , CommonConst.AUTONSI_QUEUE_DOCUMENT
                };

            foreach (var queue in queues)
            {
                await _consumerService.ReadMessages(queue);
            }

            if (_webHostEnvironment.EnvironmentName == CommonConst.PRODUCTION)
            {
                foreach (var queue in autonsiQueues)
                {
                    await _autonsiConsumerService.ReadMessages(queue);
                }
            }
        }
    }
}
