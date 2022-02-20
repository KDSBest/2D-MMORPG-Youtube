using Common.GameDesign;
using Newtonsoft.Json;

namespace Common.Protocol.Character
{
    public class CharacterInformation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public byte Color { get; set; }

        public byte Eyes { get; set; }

        public EntityStats Stats { get; set; } = new EntityStats();

        public int Experience { get; set; } = 0;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
