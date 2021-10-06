using Newtonsoft.Json;

namespace Common.Protocol.Character
{
    public class CharacterInformation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public byte Color { get; set; }

        public byte Eyes { get; set; }

        public byte Level { get; set; } = 1;

        public int Experience { get; set; } = 0;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
