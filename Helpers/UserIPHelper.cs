using System.Net;

namespace ESD_EDI_BE.Helpers
{
    public static class UserIPHelper
    {
        public static HttpContext? httpContext { get; }
        public static string UserIp
        {
            get
            {
                var result = string.Empty;
                if (httpContext != null)
                {
                    result = GetWebClientIp();
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = GetLanIp();
                }

                return result;
            }
        }

        private static string? GetWebRemoteIp()
        {
            if (httpContext?.Connection?.RemoteIpAddress == null)
                return string.Empty;
            var ip = httpContext?.Connection?.RemoteIpAddress.ToString();
            if (httpContext == null)
                return ip;
            if (httpContext.Request.Headers.ContainsKey("X-Real-IP"))
            {
                ip = httpContext.Request.Headers["X-Real-IP"].ToString();
            }

            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = httpContext.Request.Headers["X-Forwarded-For"].ToString();
            }

            return ip;
        }

        private static string GetWebClientIp()
        {
            var ip = GetWebRemoteIp();
            foreach (var hostAddress in Dns.GetHostAddresses(ip))
            {
                if (hostAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return hostAddress.ToString();
                }
            }

            return string.Empty;
        }


        private static string GetLanIp()
        {
            foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return hostAddress.ToString();
                }
            }

            return string.Empty;
        }
    }
}
