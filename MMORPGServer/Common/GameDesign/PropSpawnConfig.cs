using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Common.GameDesign
{
	public class PropSpawnConfig
	{
		public string PropPrefix;
		public int MaxHealth = 50;

		public Vector2 SpawnStart;
		public Vector2 SpawnEnd;

		public int SpawnCount;

		public PropType Type;
		public int RespawnTimeInMs;
	}
}
