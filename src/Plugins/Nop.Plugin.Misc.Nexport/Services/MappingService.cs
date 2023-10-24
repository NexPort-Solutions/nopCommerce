namespace Nop.Plugin.Misc.Nexport.Services;

//public interface IMappingService
//{
//    Task<bool> HasUnprocessedAnswer(int orderId);
//}

//public class MappingService : IMappingService
//{
//    private readonly IProductMappingService _productMapping;
//    private readonly IOrderService _order;
//    private readonly ISupplementalInfoService _supplementalInfo;

//    public MappingService(
//        IOrderService order,
//        IProductMappingService productMapping,
//        ISupplementalInfoService supplementalInfo)
//    {
//        _order = order;
//        _productMapping = productMapping;
//        _supplementalInfo = supplementalInfo;
//    }

//    public async Task<bool> HasUnprocessedAnswer(int orderId)
//    {
//        var order = await _order.GetOrderByIdAsync(orderId);
//        if (order?.Deleted is not false || order.OrderStatus != OrderStatus.Complete)
//        {
//            return false;
//        }
//        var orderItems = await _order.GetOrderItemsAsync(order.Id);
//        var productMappings = await orderItems
//            .SelectAwait(async item =>
//                await _productMapping.GetByNopProductId(item.ProductId, order.StoreId)
//                    ?? await _productMapping.GetByNopProductId(item.ProductId))
//            .WhereNotNull()
//            .ToListAsync();
//        return await productMappings.SelectAwait(async productMapping =>
//            await (await _supplementalInfo.GetSupplementalInfoQuestionMappings(productMapping.Id))
//                .Select(questionMapping => questionMapping.QuestionId)
//                .ToListAsync())
//            .SelectAwait(async questions =>
//                (await _supplementalInfo.GetSupplementalInfoAnswers(order.CustomerId, order.StoreId))
//                    .Any(answer => questions.Contains(answer.QuestionId) && answer.Status is AnswerStatus.NotProcessed))
//            .FirstOrDefaultAsync();
//    }
//}
