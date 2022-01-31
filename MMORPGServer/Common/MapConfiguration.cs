using System;
using System.Numerics;

namespace Common
{
	public static class MapConfiguration
	{
		public static string MapName
		{
			get
			{
				return Environment.GetEnvironmentVariable("MAPNAME") ?? "Town";

			}
		}

		public static int MapAreaSize = 100;
		public static int RegistrationBorder = 1;
		public static int UnregistrationBorder = 2;
		public static int PlayerPriority = 100;
		public static float SmallDistance = 0.0001f;
		public static long MaxPlayerStateTime = TimeSpan.TicksPerSecond * 2;
		public static long ServerSaveTime = TimeSpan.TicksPerSecond * 10;

		public static float MaxPlayerSpeedSquared = (new Vector2(20, 35)).LengthSquared();
		public static float MaxPlayerSpeedEpsilon = 0.01f;
	}
}
