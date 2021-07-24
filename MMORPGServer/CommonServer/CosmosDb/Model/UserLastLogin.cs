using Newtonsoft.Json;
using System;

namespace CommonServer.CosmosDb.Model
{
    public class UserLastLogin
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public DateTime LastLogin { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
