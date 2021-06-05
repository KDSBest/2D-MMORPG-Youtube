namespace CommonServer.Configuration
{
    public static class RedisConfiguration
    {
#if DEBUG
        public static readonly string ConnectionString = "localhost:64886";
#else
        public static readonly string ConnectionString = "redis-svc";
#endif

        public static readonly string WorldChatChannelPrefix = "WorldChat";
        public static readonly string MapChannelPrefix = "Map-";
    }
}
