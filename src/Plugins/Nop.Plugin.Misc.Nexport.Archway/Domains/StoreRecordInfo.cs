using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains;

public interface IStoreRecord
{
    int StoreNumber { get; set; }
    string OperatorId { get; set; }
    int RegionCode { get; set; }
    string Address { get; set; }
    string City { get; set; }
    string State { get; set; }
    string PostalCode { get; set; }
    string AdvertisingCoop { get; set; }
    string StoreType { get; set; }
    string OperatorFirstName { get; set; }
    string OperatorLastName { get; set; }
}

public class StoreRecordInfo : BaseEntity, IStoreRecord
{
    public required int StoreNumber { get; set; }
    public required string OperatorId { get; set; }
    public required int RegionCode { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string AdvertisingCoop { get; set; }
    public required string StoreType { get; set; }
    public required string OperatorFirstName { get; set; }
    public required string OperatorLastName { get; set; }
}
