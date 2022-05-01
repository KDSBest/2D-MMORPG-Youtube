using System;
using System.Numerics;
using System.Text;

namespace Common.GameDesign
{
	public class EnemySpawnConfig
	{
		public string Prefix;

		public EntityStats Stats = new EntityStats()
		{
			Attack = 1,
			Defense = 1,
			MAttack = 1,
			MDefense = 1,
			Level = 1,
			MaxHP = 25
		};

		public Vector2 SpawnStart;
		public Vector2 SpawnEnd;

		public int SpawnCount;
		public int Exp = 5;

		public EnemyType Type;
		public EnemyAIConfig AIConfig;
		public int RespawnTimeInMs;
	}
}
