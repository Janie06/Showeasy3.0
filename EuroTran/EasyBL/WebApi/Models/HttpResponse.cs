using Newtonsoft.Json;

namespace EasyBL.WebApi.Models
{
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public string Info { get; set; }
    }

    public class TokenResult : HttpResponse
    {
        public TicketAuth Result
        {
            get
            {
                if (StatusCode == (int)StatusCodeEnum.Success)
                {
                    return JsonConvert.DeserializeObject<TicketAuth>(Data.ToString());
                }

                return null;
            }
        }
    }
}