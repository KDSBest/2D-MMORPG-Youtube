using Common.Protocol;
using ReliableUdp.Utility;
using StackExchange.Redis;
using System;
using CommonServer.Configuration;
using Newtonsoft.Json;

namespace CommonServer.Redis
{
    public static class RedisKV
    {
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisConfiguration.ConnectionString);

        public static void SetIfNotExists(string key, string val)
        {
            redis.GetDatabase().StringSet(new RedisKey(key), new RedisValue(val), null, When.NotExists);
		}

        public static void Set(string key, string val)
        {
            redis.GetDatabase().StringSet(new RedisKey(key), new RedisValue(val));
        }

        public static string Get(string key)
		{
            return redis.GetDatabase().StringGet(new RedisKey(key)).ToString();
        }

        public static void Remove(string key)
        {
            redis.GetDatabase().KeyDelete(new RedisKey(key));
        }

		public static void Set<T>(string key, T data)
		{
            Set(key, JsonConvert.SerializeObject(data));
		}

		public static T Get<T>(string key) where T : class
		{
            string redisObj = Get(key);
            if (string.IsNullOrEmpty(redisObj))
                return null;

            return JsonConvert.DeserializeObject<T>(redisObj); 
        }
    }
}
