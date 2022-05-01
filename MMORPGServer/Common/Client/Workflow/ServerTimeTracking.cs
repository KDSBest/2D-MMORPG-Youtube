using System;

namespace Common.Client.Workflow
{
	public static class ServerTimeTracking
	{
		public static long UtcDiff = 0;
		public static long GetServerTime()
		{
			return DateTime.UtcNow.Ticks + UtcDiff;
		}
	}
}
