using Common.Protocol;
using ReliableUdp.Utility;
using StackExchange.Redis;
using System;
using System.Linq;
using CommonServer.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CommonServer.Redis
{
    public static class RedisList
    {
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisConfiguration.ConnectionString);

        public static void Add(string key, string val)
        {
            redis.GetDatabase().ListRightPush(new RedisKey(key), new RedisValue(val));
        }

        public static void AddUnique(string key, string val)
        {
            redis.GetDatabase().SetAdd(new RedisKey(key), new RedisValue(val));
        }

        public static List<string> Get(string key)
		{
            return redis.GetDatabase().ListRange(new RedisKey(key)).ToList().ConvertAll((x) => x.ToString()).ToList();
        }

        public static void Remove(string key, string val)
        {
            redis.GetDatabase().ListRemove(new RedisKey(key), new RedisValue(val));
        }
    }
}
