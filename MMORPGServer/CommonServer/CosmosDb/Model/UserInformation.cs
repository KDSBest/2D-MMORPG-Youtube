using Newtonsoft.Json;

namespace CommonServer.CosmosDb.Model
{
    public class UserInformation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string PasswordHash { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
