﻿namespace CommonServer.Configuration
{
    public static class RedisConfiguration
    {
#if DEBUG
        public static readonly string ConnectionString = "localhost:6379";
#else
        public static readonly string ConnectionString = "redis-svc";
#endif

        public static readonly string WorldChatChannelPrefix = "WorldChat";
        public static readonly string MapChannelNewPlayerStatePrefix = "PlMapState-";
        public static readonly string MapChannelNewPropStatePrefix = "PrMapState-";
        public static readonly string MapChannelSkillCastPrefix = "SkillCast-";
        public static readonly string MapChannelRemoveStatePrefix = "MapRemove-";

        public static readonly string PlayerDamagePrefix = "Damage-";

        public static readonly string LoginQueue = "Queue:Login";
    }
}
