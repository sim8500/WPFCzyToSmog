using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CzyToSmog.net.Model
{
    [DataContract]
    public class StationOverviewModel
    {
        [IgnoreDataMember]
        public String StationName { get; set; }

        [DataMember(Name = "id")]
        public int StationId { get; set; }

        [DataMember(Name = "pm10IndexLevel")]
        public QualityIndexEntry PM10Index { get; set; }

        [DataMember(Name = "pm25IndexLevel")]
        public QualityIndexEntry PM25Index { get; set; }
    }

    [DataContract]
    public class QualityIndexEntry
    {
        [DataMember(Name = "indexLevelName")]
        public String IndexName { get; set; }

        [DataMember(Name = "id")]
        public int IdValue { get; set; }

    }

    [DataContract]
    public class StationInfoModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "stationName")]
        public String StationName { get; set; }

        [DataMember(Name = "city")]
        public CityInfoModel City { get; set; }

        [DataMember(Name = "addressStreet")]
        public String Address { get; set; }
    }

    [DataContract]
    public class CityInfoModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "commune")]
        public CommuneInfoModel Commune { get; set; }
    }

    [DataContract]
    public class CommuneInfoModel
    {
        [DataMember(Name = "communeName")]
        public String CommuneName { get; set; }
        [DataMember(Name = "provinceName")]
        public String ProvinceName { get; set; }
        [DataMember(Name = "districtName")]
        public String DistrictName { get; set; }

    }

    [DataContract]
    public class SensorInfoModel
    {
        [DataMember(Name = "id")]
        public String Id { get; set; }

        [DataMember(Name = "param")]
        public SensorParamEntry ParamInfo { get; set; }
    }

    [DataContract]
    public class SensorParamEntry
    {
        [DataMember(Name = "paramName")]
        public String Name { get; set; }

        [DataMember(Name = "paramFormula")]
        public String Formula { get; set; }

        [DataMember(Name = "paramCode")]
        public String Code { get; set; }

        [DataMember(Name = "idParam")]
        public int Id { get; set; }
    }

    [DataContract]
    public class SensorDataInfo
    {
        [DataMember(Name = "key")]
        public String Name { get; set; }

        [DataMember(Name = "values")]
        public List<SensorDataEntry> Entries { get; set; }

    }

    [DataContract]
    public class SensorDataEntry
    {
        [DataMember(Name = "date")]
        public String Date { get; set; }

        [DataMember(Name = "value")]
        public String Value { get; set; }

        [IgnoreDataMember]
        public String EntryString { get { return $"{this.Value} @ {this.Date}"; } }
    }
}
