using Nop.Data;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IFundingPoolService
{
    Task<FundingPool?> GetById(int id);
    Task<IList<FundingPool>> GetAll();
    Task InsertOrUpdate(FundingPool fundingPool);
    Task Delete(int id);
    Task Delete(FundingPool fundingPool);
}

public class FundingPoolService : IFundingPoolService
{
    private readonly IRepository<FundingPool> _fundingPools;

    public FundingPoolService(IRepository<FundingPool> fundingPools)
    {
        _fundingPools = fundingPools;
    }

    public Task<IList<FundingPool>> GetAll()
        => _fundingPools.GetAllAsync(query => from fundingPool in query orderby fundingPool.Name select fundingPool);

    public async Task InsertOrUpdate(FundingPool fundingPool)
    {
        if (await _fundingPools.GetByIdAsync(fundingPool.Id) is { })
        {
            await _fundingPools.UpdateAsync(fundingPool);
        }
        else
        {
            await _fundingPools.InsertAsync(fundingPool);
        }
    }

    public Task<FundingPool?> GetById(int id) => _fundingPools.GetByIdAsync(id)!;

    public async Task Delete(int id)
    {
        var fundingPool = await _fundingPools.GetByIdAsync(id);
        await _fundingPools.DeleteAsync(fundingPool);
    }

    public Task Delete(FundingPool fundingPool) => _fundingPools.DeleteAsync(fundingPool);
}
