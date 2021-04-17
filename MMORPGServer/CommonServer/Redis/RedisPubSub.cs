using Common.Protocol;
using ReliableUdp.Utility;
using StackExchange.Redis;
using System;

namespace CommonServer.Redis
{
    public static class RedisPubSub
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisConfiguration.ConnectionString);

        private static void Subscribe<T>(string nameOrPattern, RedisChannel.PatternMode mode, Action<RedisChannel, T> action) where T : IUdpPackage, new()
        {
            redis.GetSubscriber().SubscribeAsync(new RedisChannel(nameOrPattern, mode), (channel, value) =>
            {
                T val = new T();
                val.Read(new UdpDataReader((byte[])value));
                action(channel, val);
            }).FireAndForget();
        }

        public static void Subscribe<T>(string name, Action<RedisChannel, T> action) where T : IUdpPackage, new()
        {
            Subscribe<T>(name, RedisChannel.PatternMode.Literal, action);
        }

        public static void SubscribePattern<T>(string pattern, Action<RedisChannel, T> action) where T : IUdpPackage, new()
        {
            Subscribe<T>(pattern, RedisChannel.PatternMode.Pattern, action);
        }

        public static void PublishPattern<T>(string pattern, T val) where T : IUdpPackage
        {
            Publish<T>(pattern, RedisChannel.PatternMode.Pattern, val);
        }

        public static void Publish<T>(string name, T val) where T : IUdpPackage
        {
            Publish<T>(name, RedisChannel.PatternMode.Literal, val);
        }

        private static void Publish<T>(string nameOrPattern, RedisChannel.PatternMode mode, T val) where T : IUdpPackage
        {
            UdpDataWriter writer = new UdpDataWriter();
            val.Write(writer);
            redis.GetSubscriber().PublishAsync(new RedisChannel(nameOrPattern, mode), writer.CopyData()).FireAndForget();
        }

        public static void UnSubscribe(string name)
        {
            redis.GetSubscriber().UnsubscribeAsync(new RedisChannel(name, RedisChannel.PatternMode.Literal)).FireAndForget();
        }

        public static void UnSubscribePattern(string pattern)
        {
            redis.GetSubscriber().UnsubscribeAsync(new RedisChannel(pattern, RedisChannel.PatternMode.Pattern)).FireAndForget();
        }
    }
}
