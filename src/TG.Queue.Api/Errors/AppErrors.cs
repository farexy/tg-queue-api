using TG.Core.App.OperationResults;

namespace TG.Queue.Api.Errors
{
    public static class AppErrors
    {
        public static readonly ErrorResult NotFound = new ErrorResult("not_found", "Not found");
        public static readonly ErrorResult TestServerNotAllowed = new ErrorResult("test_server_not_allowed", "Test Server not allowed");
        public static readonly ErrorResult UserNotEnoughMoney = new ErrorResult("user_not_enough_money", "Not enough money");
    }
}