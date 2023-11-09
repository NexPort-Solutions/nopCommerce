namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public record CreateFundingPoolModel(string Name, string Message, CreateFundingPoolModelState State);

public enum CreateFundingPoolModelState
{
    Unsubmitted = 0,
    Successful = 1,
    Failure = 2,
}
