using Common.Protocol;
using ReliableUdp.Utility;
using StackExchange.Redis;
using System;
using CommonServer.Configuration;
using Newtonsoft.Json;

namespace CommonServer.Redis
{
    public static class RedisQueue
    {
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisConfiguration.ConnectionString);

        public static void Enqueue<T>(string queueName, T item) where T : class
        {
            redis.GetDatabase().ListLeftPush(new RedisKey(queueName), new RedisValue(JsonConvert.SerializeObject(item)));
		}

        public static T Dequeue<T>(string queueName) where T : class
		{
            var val = redis.GetDatabase().ListRightPop(new RedisKey(queueName));
            if (val.IsNullOrEmpty)
                return null;

            return JsonConvert.DeserializeObject<T>(val.ToString());
		}
    }
}
