using Microsoft.AspNetCore.Http;

namespace ESD_EDI_BE.Extensions
{
    public static class AutoId
    {
        public static long AutoGenerate()
        {
            //var d = DateTime.UtcNow;
            //var toStr = d.ToString("yyMMddHHmmssf");
            //var rd = new Random().Next(100, 999).ToString();
            //return Int64.Parse(string.Concat(toStr, rd));

            var dateBegin = new DateTime(2000, 1, 1);

            var now = DateTime.UtcNow;

            var diffTimespan = now - dateBegin;
            return (long)((diffTimespan.Ticks / 100) + Math.Floor(new Random().NextDouble() * 10000000));
        }
    }
}
