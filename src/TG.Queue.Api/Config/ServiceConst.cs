namespace TG.Queue.Api.Config
{
    public static class ServiceConst
    {
        public const string ServiceName = "queue";
        public const string ProjectName = "TG.Queue.Api";

        public const string RoutePrefix = ServiceName + "/v{version:apiVersion}/[controller]";
        public const string InternalRoutePrefix = "internal/" + ServiceName + "/v{version:apiVersion}/[controller]";
    }
}