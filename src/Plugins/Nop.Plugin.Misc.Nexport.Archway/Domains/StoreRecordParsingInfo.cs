namespace Nop.Plugin.Misc.Nexport.Archway.Domains;

public class StoreRecordParsingInfo : IStoreRecord
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
