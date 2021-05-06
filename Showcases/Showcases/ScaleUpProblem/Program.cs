using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScaleUpProblem
{
	class Program
	{
		static void Main(string[] args)
		{
			int c = 100000;
			int coreCount = Environment.ProcessorCount;

			long avgCount = 0;
			long oneThread = 0;
			long distributeThread = 0;
			long distributeCore = 0;

			for (; ; )
			{
				oneThread += OneThread(c);
				distributeThread += DistributeThread(c);
				distributeCore += DistributeOnCores(c, coreCount);
				avgCount++;

				Console.WriteLine($"One: {oneThread / avgCount} Distribute: {distributeThread / avgCount} DistributeByCore ({coreCount}): {distributeCore / avgCount}");
			}
		}

		private static long DistributeThread(int c)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			var tasks = new Task[c];
			for (int i = 0; i < c; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					WasteCPU(i);
				});
			}

			Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();

			sw.Stop();

			return sw.ElapsedMilliseconds;
		}

		private static long DistributeOnCores(int c, int cores)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			int numbersForEachCore = c / cores;
			int overhang = c % cores;

			var tasks = new Task[cores];
			var coreUpperBound = new int[cores];
			var coreLowerBound = new int[cores];
			int startForNextCore = 0;

			for (int i = 0; i < cores; i++)
			{
				coreUpperBound[i] = numbersForEachCore + startForNextCore;
				if (overhang > 0)
				{
					overhang--;
					coreUpperBound[i]++;
				}

				coreLowerBound[i] = startForNextCore;
				startForNextCore += coreUpperBound[i];

				// Copy i to closure scope
				int currentCore = i;
				tasks[i] = Task.Run(() =>
				{
					for (int i = coreLowerBound[currentCore]; i < coreUpperBound[currentCore]; i++)
					{
						WasteCPU(i);
					}
				});
			}

			Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();

			sw.Stop();

			return sw.ElapsedMilliseconds;
		}

		private static long OneThread(int c)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < c; i++)
			{
				WasteCPU(i);
			}
			sw.Stop();

			return sw.ElapsedMilliseconds;
		}

		private static float WasteCPU(float f, int c = 10000)
		{
			for (int i = 0; i < c; i++)
			{
				f = MathF.Sqrt(f);
			}

			return f;
		}
	}
}
