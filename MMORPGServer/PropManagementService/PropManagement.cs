using Common;
using Common.GameDesign;
using Common.Protocol.Map;
using CommonServer.Configuration;
using CommonServer.Redis;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PropManagementService
{
	public class PropManagement
	{
		private readonly PropSpawnConfig config;
		private readonly List<PropStateMessage> props = new List<PropStateMessage>();
		private readonly Dictionary<string, int> respawnTimer = new Dictionary<string, int>();
		private readonly Random random = new Random();

		public PropManagement(PropSpawnConfig config)
		{
			this.config = config;
		}

		private Vector2 GetRandomPosition()
		{
			return config.SpawnStart + ((config.SpawnEnd - config.SpawnStart) * (float)random.NextDouble());
		}

		public void Initialize()
		{
			for (int i = 0; i < config.SpawnCount; i++)
			{
				var prop = new PropStateMessage()
				{
					Name = config.PropPrefix + i,
					Animation = 0,
					MaxHealth = config.MaxHealth,
					IsLookingRight = false,
					Type = config.Type
				};

				props.Add(prop);

				SpawnProp(prop);
			}
		}

		private void SpawnProp(PropStateMessage prop)
		{
			prop.ServerTime = DateTime.UtcNow.Ticks;
			prop.Position = GetRandomPosition();
			prop.Health = config.MaxHealth;

			respawnTimer[prop.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs)
		{
			foreach (var prop in props)
			{
				if (prop.Health > 0)
					continue;

				respawnTimer[prop.Name] = respawnTimer[prop.Name] - timeInMs;

				if (respawnTimer[prop.Name] <= 0)
				{
					SpawnProp(prop);
				}
			}
		}

		public void Update(int timeInMs)
		{
			HandleRespawn(timeInMs);

			foreach(var prop in props)
			{
				RedisPubSub.Publish<PropStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, prop);
			}
		}
	}
}
