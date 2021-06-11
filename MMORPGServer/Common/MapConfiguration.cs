using System;

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
		public static long MaxPlayerStateTime = TimeSpan.TicksPerMillisecond * 2000;
	}
}
