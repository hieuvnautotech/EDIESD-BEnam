namespace ESD_EDI_BE.DbAccess
{
    public static class ConnectionString
    {
        // public const string CONNECTIONSTRING = $"data source=118.69.130.73,1435;initial catalog=ESD_EDI;persist security info=True;user id=sa;password=YFemGoN1mCoYP7hrfUY5Re5z1YIe0hVG2R76R4pj5O7k2YQnM3;MultipleActiveResultSets=True;";

        // public const string CONNECTIONSTRING = $"data source=192.168.2.19,1433;initial catalog=ESD_EDI;persist security info=True;user id=sa;password=Ixo5zHG51H6QBJsVre7BC4mjdTLcy18jdBzs90CCcfKAdcMusR;MultipleActiveResultSets=True;";

        public static readonly string SECRET = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiOGIyMTJjZi03ZjY4LTRmNjktYTA1Zi1hY2NhZjU4MjQxMWYiLCJpYXQiOjE2NTEyMjE3NDAsInN1YiI6Ikp3dEF1dGhlbnRpY2F0aW9uU2VydmljZUFjY2Vzc1Rva2VuIiwibmFtZWlkIjoibmFtIiwicm9sZSI6IlJPT1QiLCJuYmYiOjE2NTEyMjE3NDAsImV4cCI6MTY1MTIyMTgzMCwiaXNzIjoiSnd0QXV0aGVudGljYXRpb25TZXJ2ZXIiLCJhdWQiOiJKd3RBdXRoZW50aWNhdGlvblNlcnZpY2VQb3N0bWFuQ2xpZW50In0.yzey7IxENVJwilmvSneL8Ftf6a2QAjeDpXTsgiRXUDM";
        public const string ISSUER = "JwtAuthenticationServer";
        public const string AUDIENCE = "JwtAuthenticationServicePostmanClient";

    }
}
