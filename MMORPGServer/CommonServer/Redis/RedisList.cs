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

        public static void AddUnique(string key, string val)
        {
            redis.GetDatabase().SortedSetAdd(new RedisKey(key), new RedisValue(val), 0);
        }

        public static List<string> Get(string key)
		{
            return redis.GetDatabase().SortedSetRangeByScore(new RedisKey(key)).ToList().ConvertAll((x) => x.ToString()).ToList();
        }

        public static void Remove(string key, string val)
        {
            redis.GetDatabase().SortedSetRemove(new RedisKey(key), new RedisValue(val));
        }

		internal static void Clear(string key)
		{
            redis.GetDatabase().KeyDelete(key);
		}
	}
}
