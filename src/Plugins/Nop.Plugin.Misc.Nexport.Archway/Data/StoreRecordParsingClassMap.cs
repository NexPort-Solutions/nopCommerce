using CsvHelper.Configuration;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data;

public class StoreRecordParsingClassMap : ClassMap<StoreRecordParsingInfo>
{
    public StoreRecordParsingClassMap()
    {
        Map(storeRecordParsingInfo => storeRecordParsingInfo.StoreNumber).Name("NATL_STR_NU");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.OperatorId).Name("OPER_ID_NU");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.RegionCode).Name("REG_CD");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.Address).Name("SITE_L2_AD");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.City).Name("SITE_CITY_AD");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.State).Name("SITE_ABBR_ST_AD");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.PostalCode).Name("SITE_PSTL_CD");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.AdvertisingCoop).Name("ADVT_COOP_NA");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.StoreType).Name("StoreType");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.OperatorFirstName).Name("OPER_FST_NA");
        Map(storeRecordParsingInfo => storeRecordParsingInfo.OperatorLastName).Name("OPER_LAST_NA");
    }
}
