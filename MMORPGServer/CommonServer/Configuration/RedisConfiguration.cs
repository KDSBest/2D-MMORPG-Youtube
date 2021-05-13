namespace CommonServer.Configuration
{
    public static class RedisConfiguration
    {
#if DEBUG
        public static readonly string ConnectionString = "host.docker.internal:32767";
#else
        public static readonly string ConnectionString = "redis-svc";
#endif

        public static readonly string WorldChatChannelPrefix = "WorldChat";
        public static readonly string MapChannelPrefix = "Map-";
    }
}
