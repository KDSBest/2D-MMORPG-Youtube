using System;

namespace CommonServer.Configuration
{
    public static class RedisConfiguration
    {
        public static readonly string ConnectionString = Environment.GetEnvironmentVariable("REDIS") ?? "localhost:6379";

        public static readonly string WorldChatChannelPrefix = "WorldChat";
        public static readonly string MapChannelNewPlayerStatePrefix = "PlMapState-";
        public static readonly string MapChannelNewPropStatePrefix = "PrMapState-";
        public static readonly string MapChannelSkillCastPrefix = "SkillCast-";
        public static readonly string MapChannelRemoveStatePrefix = "MapRemove-";

        public static readonly string CharUpdatePrefix = "CharUpdate-";

        public static readonly string PlayerDamagePrefix = "Damage-";
        public static readonly string PlayerExpPrefix = "Exp-";

        public static readonly string LoginQueue = "Queue:Login";
    }
}
