namespace ESD_EDI_BE.Models
{
    public class CreateBinTranferRequestRequestDetail
    {

        public long PalletId { get; set; }
        public long FromBinId { get; set; }
        public long ToBinId { get; set; }
    }
    public class CreateBinTranferRequestRequest
    {
        public string Remark { get; set; }
        public CreateBinTranferRequestRequestDetail[] Details { get; set; }
    }

}