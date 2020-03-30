using Newtonsoft.Json;
using System.Collections.Generic;

namespace TelegramWebHook.Models
{
    public class JsonReportModel
    {
        [JsonProperty("kerala")]
        public Dictionary<string, SummaryModel> Summary { get; set; }
    }

    public partial class SummaryModel
    {
        [JsonProperty("under_observation")]
        public long? UnderObservation { get; set; }

        [JsonProperty("under_home_isolation")]
        public long? UnderHomeIsolation { get; set; }

        [JsonProperty("total_hospitalised")]
        public long? TotalHospitalised { get; set; }

        [JsonProperty("hospitalised_today")]
        public long? HospitalisedToday { get; set; }

        [JsonProperty("corona_positive")]
        public long? CoronaPositive { get; set; }

        [JsonProperty("cured_discharged")]
        public long? CuredDischarged { get; set; }

        [JsonProperty("deaths")]
        public long? Deaths { get; set; }
    }
}
