using ESD_EDI_BE.Models.Dtos.Common;
namespace ESD_EDI_BE.Models.Dtos
{
    public class MachineDto : BaseModel
    {
        public long MachineId { get; set; }
        public string MachineCode { get; set; }
        public string MachineName { get; set; }
        public string RabbitMQType { get; set; }

    }
}