using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using CommonServer.CosmosDb.ReadModelHandler;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
	[TestClass]
	public class EventSourcingLeaseTest
	{
		[TestInitialize]
		public async Task TestSetup()
		{
			CosmosClientSinglton.RemoveSingleton();

			CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
			{
				HttpClientFactory = () =>
				{
					HttpMessageHandler httpMessageHandler = new HttpClientHandler()
					{
						ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
					};
					return new HttpClient(httpMessageHandler);
				},
				ConnectionMode = ConnectionMode.Gateway
			};

			var cosmosDb = new CosmosClient(CosmosDbConfiguration.CosmosDbEndpointUrl, CosmosDbConfiguration.CosmosDbKey, cosmosClientOptions);
			try
			{
				await cosmosDb.GetDatabase(CosmosDbConfiguration.CosmosDb).DeleteAsync();
			}
			catch (Exception)
			{
			}
		}

		[TestMethod]
		public async Task LeasingFullIntegrationTest()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var repo = new InventoryRepository();
			var readModelHandler = new InventoryReadModelHandler();

			string testPlayer = "TestPlayer";
			string testItem = "Gold";
			int testAmount = 100;

			var newEvents = new ConcurrentQueue<InventoryEvent>();
			newEvents.Enqueue(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = testPlayer,
				Add = new Dictionary<string, int>
							{
								{ testItem, testAmount * 2 }
							}
			});

			await UpdateReadModelAsync(cts, readModelHandler, newEvents);

			bool runReadModel = true;
			var readModelTask = Task.Run(async () =>
			{
				while (runReadModel)
				{
					await UpdateReadModelAsync(cts, readModelHandler, newEvents);
					await Task.Delay(10);
				}
			});

			newEvents = new ConcurrentQueue<InventoryEvent>();
			List<Task<EventReasons>> tasks = new List<Task<EventReasons>>();
			for (int i = 0; i < 10; i++)
			{
				tasks.Add(RemoveIfInventoryHasEnoughWithLeaseConsistentRead(testPlayer, testItem, newEvents, testAmount, InventoryEventType.Quest));
			}

			await Task.WhenAll(tasks);

			await Task.Delay(50);
			runReadModel = false;

			var inv = await repo.GetAsync(testPlayer);
			Assert.AreEqual(0, inv.Items[testItem]);

			var result = tasks.Select(x => x.Result);
			Assert.AreEqual(2, result.Count(x => x == EventReasons.EventAdded));
			Assert.AreEqual(8, result.Count(x => x == EventReasons.EventNotAddedDueToBusinessLogic));
		}

		[TestMethod]
		public async Task IssueEventualConsistencyFixTest()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var repo = new InventoryRepository();
			var readModelHandler = new InventoryReadModelHandler();

			string testPlayer = "TestPlayer";
			string testItem = "Gold";
			int testAmount = 100;

			var newEvents = new ConcurrentQueue<InventoryEvent>();
			newEvents.Enqueue(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = testPlayer,
				Add = new Dictionary<string, int>
				{
					{ testItem, testAmount * 2 }
				}
			});

			await UpdateReadModelAsync(cts, readModelHandler, newEvents);

			bool runReadModel = true;
			var readModelTask = Task.Run(async () =>
			{
				while (runReadModel)
				{
					await UpdateReadModelAsync(cts, readModelHandler, newEvents);
					await Task.Delay(10);
				}
			});

			newEvents = new ConcurrentQueue<InventoryEvent>();
			var t1 = RemoveIfInventoryHasEnoughWithLeaseConsistentRead(testPlayer, testItem, newEvents, testAmount, InventoryEventType.Quest);
			var t2 = RemoveIfInventoryHasEnoughWithLeaseConsistentRead(testPlayer, testItem, newEvents, testAmount, InventoryEventType.TradeItem);

			await Task.WhenAll(t1, t2);

			await Task.Delay(50);
			runReadModel = false;

			var result = new List<EventReasons>();
			result.Add(t1.Result);
			result.Add(t2.Result);
			Assert.AreEqual(2, result.Count(x => x == EventReasons.EventAdded));

			var inv = await repo.GetAsync(testPlayer);
			Assert.AreEqual(0, inv.Items[testItem]);
		}

		[TestMethod]
		public async Task IssueEventualConsistencyLeaseFixIssueTest()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var repo = new InventoryRepository();
			var readModelHandler = new InventoryReadModelHandler();

			string testPlayer = "TestPlayer";
			string testItem = "Gold";
			int testAmount = 100;

			var newEvents = new ConcurrentQueue<InventoryEvent>();
			newEvents.Enqueue(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = testPlayer,
				Add = new Dictionary<string, int>
							{
								{ testItem, testAmount * 2 }
							}
			});

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			newEvents = new ConcurrentQueue<InventoryEvent>();
			var t1 = RemoveIfInventoryHasEnoughWithLease(testPlayer, testItem, newEvents, testAmount, InventoryEventType.Quest);
			var t2 = RemoveIfInventoryHasEnoughWithLease(testPlayer, testItem, newEvents, testAmount, InventoryEventType.TradeItem);

			await Task.WhenAll(t1, t2);
			Assert.AreEqual(1, newEvents.Count);

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			var inv = await repo.GetAsync(testPlayer);
			Assert.AreEqual(100, inv.Items[testItem]);

			var result = new List<EventReasons>();
			result.Add(t1.Result);
			result.Add(t2.Result);
			Assert.AreEqual(1, result.Count(x => x == EventReasons.EventAdded));
			Assert.AreEqual(1, result.Count(x => x == EventReasons.LeaseBlocked));
		}

		[TestMethod]
		public async Task IssueEventualConsistencyLeaseFixTest()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var repo = new InventoryRepository();
			var readModelHandler = new InventoryReadModelHandler();

			string testPlayer = "TestPlayer";
			string testItem = "Gold";
			int testAmount = 100;

			var newEvents = new ConcurrentQueue<InventoryEvent>();
			newEvents.Enqueue(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = testPlayer,
				Add = new Dictionary<string, int>
							{
								{ testItem, testAmount }
							}
			});

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			newEvents = new ConcurrentQueue<InventoryEvent>();
			var t1 = RemoveIfInventoryHasEnoughWithLease(testPlayer, testItem, newEvents, testAmount, InventoryEventType.Quest);
			var t2 = RemoveIfInventoryHasEnoughWithLease(testPlayer, testItem, newEvents, testAmount, InventoryEventType.TradeItem);

			await Task.WhenAll(t1, t2);
			Assert.AreEqual(1, newEvents.Count);

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			var inv = await repo.GetAsync(testPlayer);
			Assert.AreEqual(0, inv.Items[testItem]);
		}

		[TestMethod]
		public async Task IssueEventualConsistencyTest()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var repo = new InventoryRepository();
			var readModelHandler = new InventoryReadModelHandler();
			string testPlayer = "TestPlayer";
			string testItem = "Gold";
			int testAmount = 100;

			var newEvents = new ConcurrentQueue<InventoryEvent>();
			newEvents.Enqueue(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = testPlayer,
				Add = new Dictionary<string, int>
							{
								{ testItem, testAmount }
							}
			});

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			newEvents = new ConcurrentQueue<InventoryEvent>();
			var t1 = RemoveIfInventoryHasEnough(testPlayer, testItem, newEvents, testAmount, InventoryEventType.Quest, DateTime.UtcNow);
			var t2 = RemoveIfInventoryHasEnough(testPlayer, testItem, newEvents, testAmount, InventoryEventType.TradeItem, DateTime.UtcNow);

			await Task.WhenAll(t1, t2);
			Assert.AreEqual(2, newEvents.Count);

			await readModelHandler.ChangeHandler(newEvents, cts.Token);

			var inv = await repo.GetAsync(testPlayer);
			Assert.AreEqual(-100, inv.Items[testItem]);
		}


		private static async Task UpdateReadModelAsync(CancellationTokenSource cts, InventoryReadModelHandler readModelHandler, ConcurrentQueue<InventoryEvent> newEvents)
		{
			List<InventoryEvent> cloneEvents = new List<InventoryEvent>();
			while (newEvents.Count > 0)
			{
				InventoryEvent ev;
				if (newEvents.TryDequeue(out ev))
					cloneEvents.Add(ev);
			}

			if(cloneEvents.Count > 0)
				await readModelHandler.ChangeHandler(cloneEvents, cts.Token);
		}


		public enum EventReasons
		{
			EventAdded,
			EventNotAddedDueToBusinessLogic,
			LeaseBlocked,
			MaxRetriesReached
		}

		private static async Task<EventReasons> RemoveIfInventoryHasEnoughWithLeaseConsistentRead(string testPlayer, string testItem, ConcurrentQueue<InventoryEvent> newEvents, int testAmount, InventoryEventType type, int retry = 0)
		{
			if (retry > 100)
				return EventReasons.MaxRetriesReached;

			DateTime eventTimestamp = DateTime.UtcNow;
			InventoryEventRepository repo = new InventoryEventRepository();

			var lease = await repo.LeaseManagement.TryAcquireAsync(testPlayer, TimeSpan.FromSeconds(10), eventTimestamp);

			if (lease.Acquired)
			{
				InventoryRepository invRepo = new InventoryRepository();
				var inv = await invRepo.GetAsync(testPlayer);
				int maxInvGetRetries = 100;

				while (inv == null && inv.LatestEventTimestamp < lease.LatestEvent && maxInvGetRetries > 0)
				{
					await Task.Delay(10);
					maxInvGetRetries--;
					inv = await invRepo.GetAsync(testPlayer);
				}

				var result = await RemoveIfInventoryHasEnough(testPlayer, testItem, newEvents, testAmount, type, eventTimestamp);
				await repo.LeaseManagement.TryFreeAsync(lease);
				return result;
			}
			else
			{
				await Task.Delay(10);
				return await RemoveIfInventoryHasEnoughWithLeaseConsistentRead(testPlayer, testItem, newEvents, testAmount, type, retry + 1);
			}
		}

		private static async Task<EventReasons> RemoveIfInventoryHasEnoughWithLease(string testPlayer, string testItem, ConcurrentQueue<InventoryEvent> newEvents, int testAmount, InventoryEventType type)
		{
			DateTime eventTimestamp = DateTime.UtcNow;

			InventoryEventRepository repo = new InventoryEventRepository();
			var lease = await repo.LeaseManagement.TryAcquireAsync(testPlayer, TimeSpan.FromSeconds(10), eventTimestamp);

			if (lease.Acquired)
			{
				return await RemoveIfInventoryHasEnough(testPlayer, testItem, newEvents, testAmount, type, eventTimestamp);
			}

			return EventReasons.LeaseBlocked;
		}

		private static async Task<EventReasons> RemoveIfInventoryHasEnough(string testPlayer, string testItem, ConcurrentQueue<InventoryEvent> newEvents, int testAmount, InventoryEventType type, DateTime eventTimestamp)
		{
			InventoryRepository repo = new InventoryRepository();
			var inv = await repo.GetAsync(testPlayer);
			if (inv.Items[testItem] >= testAmount)
			{
				newEvents.Enqueue(new InventoryEvent()
				{
					Id = Guid.NewGuid(),
					PlayerId = testPlayer,
					Type = type,
					CreationDate = eventTimestamp,
					Remove = new Dictionary<string, int>
					{
						{ testItem, testAmount }
					}
				});

				return EventReasons.EventAdded;
			}
			else
			{
				// Empty Event so that lease -> readmodel consistency is given
				newEvents.Enqueue(new InventoryEvent()
				{
					Id = Guid.NewGuid(),
					PlayerId = testPlayer,
					Type = type,
					CreationDate = eventTimestamp
				});
				return EventReasons.EventNotAddedDueToBusinessLogic;
			}
		}
	}
}
