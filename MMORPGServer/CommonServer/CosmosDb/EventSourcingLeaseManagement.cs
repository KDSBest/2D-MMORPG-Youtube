using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;
using CommonServer.CosmosDb.Model;
using System.Net;

namespace CommonServer.CosmosDb
{
	public class EventSourcingLeaseManagement
	{
		private readonly Container container;

		public EventSourcingLeaseManagement(Container container)
		{
			this.container = container;
		}

		public async Task<bool> TryFreeAsync(EventSourcingLeaseResult leaseResult)
		{
			if (leaseResult.Acquired == false)
				return false;

			try
			{
				var partitionKey = new PartitionKey(leaseResult.Id);
				var etagCheck = new ItemRequestOptions()
				{
					IfMatchEtag = leaseResult.AcquiredEtag
				};

				var updatedLease = new EventSourcingLease()
				{
					Id = leaseResult.Id,
					LeasedUntil = DateTime.MinValue,
					LastCreatedEventTimestamp = leaseResult.LatestEvent
				};

				var updateLeaseResponse = await this.container.ReplaceItemAsync<EventSourcingLease>(updatedLease, leaseResult.Id, partitionKey, etagCheck);

				if (updateLeaseResponse.StatusCode == HttpStatusCode.OK)
				{
					return true;
				}
			}
			catch (CosmosException)
			{
				return false;
			}

			return false;
		}

		public async Task<EventSourcingLeaseResult> TryAcquireAsync(string id, TimeSpan throttleTime, DateTime eventCreationTimestamp)
		{
			var partitionKey = new PartitionKey(id);

			ItemResponse<EventSourcingLease> readResponse;
			try
			{
				readResponse = await this.container.ReadItemAsync<EventSourcingLease>(id, partitionKey);
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
				return await TryCreateLeaseAsync(id, throttleTime, eventCreationTimestamp);
			}

			var existingLease = readResponse.Resource;
			if (existingLease.LeasedUntil >= DateTime.UtcNow)
			{
				return new EventSourcingLeaseResult()
				{
					Id = id,
					Acquired = false
				};
			}

			var updatedLease = new EventSourcingLease()
			{
				Id = id,
				LeasedUntil = DateTime.UtcNow.Add(throttleTime),
				LastCreatedEventTimestamp = eventCreationTimestamp
			};

			try
			{
				var etagCheck = new ItemRequestOptions()
				{
					IfMatchEtag = readResponse.ETag
				};
				var updateLeaseResponse = await this.container.ReplaceItemAsync<EventSourcingLease>(updatedLease, id, partitionKey, etagCheck);

				return new EventSourcingLeaseResult()
				{
					Id = id,
					Acquired = updateLeaseResponse.StatusCode == HttpStatusCode.OK,
					LatestEvent = existingLease.LastCreatedEventTimestamp,
					AcquiredEtag = updateLeaseResponse.ETag
				};
			}
			catch (CosmosException)
			{
				return new EventSourcingLeaseResult()
				{
					Id = id,
					Acquired = false
				};
			}
		}

		private async Task<EventSourcingLeaseResult> TryCreateLeaseAsync(string id, TimeSpan throttleTime, DateTime eventCreationTimestamp)
		{
			var newLease = new EventSourcingLease()
			{
				Id = id,
				LeasedUntil = DateTime.UtcNow.Add(throttleTime),
				LastCreatedEventTimestamp = eventCreationTimestamp
			};

			try
			{
				var createResponse = await this.container.CreateItemAsync<EventSourcingLease>(newLease, new PartitionKey(id));

				return new EventSourcingLeaseResult()
				{
					Id = id,
					AcquiredEtag = createResponse.ETag,
					Acquired = createResponse.StatusCode == HttpStatusCode.Created
				};
			}
			catch (CosmosException)
			{
			}

			return new EventSourcingLeaseResult()
			{
				Acquired = false
			};
		}
	}
}
