using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CommonServer.ServerModel
{
	public abstract class Server
	{
		public Guid Id { get; private set; }
		public int UpdateDelay { get; private set; }
		public bool IsRunning { get; private set; } = false;

		public long UpdateDuration { get; private set; } = 0;
		private Task updateTask;

		protected Server(Guid id, int updateDelay)
		{
			Id = id;
			UpdateDelay = updateDelay;
		}

		protected abstract Task Update();

		public void Start()
		{
			if (IsRunning)
				return;

			Console.WriteLine($"Start with id - {this.Id}");
			if (updateTask != null)
			{
				updateTask.Wait(this.UpdateDelay);
			}

			IsRunning = true;
			updateTask = Task.Run(UpdateLoop);
		}

		private async Task UpdateLoop()
		{
			Stopwatch sw = new Stopwatch();
			while (IsRunning)
			{
				sw.Start();
				
				await this.Update();
				
				sw.Stop();
				UpdateDuration = sw.ElapsedMilliseconds;

				if (sw.ElapsedMilliseconds < this.UpdateDelay)
				{
					await Task.Delay((int)(this.UpdateDelay - sw.ElapsedMilliseconds));
				}

				sw.Reset();
			}
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			Console.WriteLine($"Stop with id - {this.Id}");
			IsRunning = false;
		}
	}
}
