using CsvHelper.Configuration;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data
{
    public class ArchwayStoreRecordParsingClassMap : ClassMap<ArchwayStoreRecordParsingInfo>
    {
        public ArchwayStoreRecordParsingClassMap()
        {
            Map(x => x.StoreNumber).Name("NATL_STR_NU");
            Map(x => x.OperatorId).Name("OPER_ID_NU");
            Map(x => x.RegionCode).Name("REG_CD");
            Map(x => x.Address).Name("SITE_L2_AD");
            Map(x => x.City).Name("SITE_CITY_AD");
            Map(x => x.State).Name("SITE_ABBR_ST_AD");
            Map(x => x.PostalCode).Name("SITE_PSTL_CD");
            Map(x => x.AdvertisingCoop).Name("ADVT_COOP_NA");
            Map(x => x.StoreType).Name("StoreType");
            Map(x => x.OperatorFirstName).Name("OPER_FST_NA");
            Map(x => x.OperatorLastName).Name("OPER_LAST_NA");
        }
    }
}
