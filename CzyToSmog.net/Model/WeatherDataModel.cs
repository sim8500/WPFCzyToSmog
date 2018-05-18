using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CzyToSmog.net.Model
{
    [DataContract]
    public class WeatherDataModel
    {
        [DataMember(Name ="main")]
        public WeatherMainEntry Main { get; set; }

        [DataMember(Name ="weather")]
        public List<WeatherDescription> WeatherEntries { get; set; }
    }

    [DataContract]
    public class WeatherMainEntry
    {
        [DataMember(Name ="temp")]
        public float Temp { get; set; }

        [DataMember(Name ="pressure")]
        public float Pressure { get; set; }

        [DataMember(Name ="humidity")]
        public float Humid { get; set; }

        [DataMember(Name ="temp_min")]
        public float TempMin { get; set; }

        [DataMember(Name ="temp_max")]
        public float TempMax { get; set; }

        [IgnoreDataMember]
        public string TextSummary { get { return $"Temp: {Temp} (min: {TempMin}, max: {TempMax}), Pressure: {Pressure}, Humidity: {Humid}."; } }
    }

    [DataContract]
    public class WeatherDescription
    {
        [DataMember(Name ="description")]
        public string Text { get; set; }

        [DataMember(Name ="icon")]
        public string Icon { get; set; }
    }
}
