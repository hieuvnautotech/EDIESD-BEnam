namespace ESD_EDI_BE.Models.Dtos.Common
{
    public class ResponseModel<T>
    {
        public int HttpResponseCode { get; set; }
        public T? Data { get; set; } = default;
        public int? TotalRow { get; set; }
        public string? ResponseMessage { get; set; }

        public ResponseModel()
        {
            HttpResponseCode = 200;
            ResponseMessage = string.Empty; ;
            Data = default;
            TotalRow = 0;
        }
    }
}
