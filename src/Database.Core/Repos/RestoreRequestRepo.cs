using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class RestoreRequestRepo : IRestoreRequestRepo
	{
		private readonly UlearnDb db;

		public RestoreRequestRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<string> CreateRequest(string userId)
		{
			var previousRequests = db.RestoreRequests.Where(r => r.UserId == userId).ToList();
			if (previousRequests.Any())
			{
				var hasRecent = false;
				foreach (var previous in previousRequests)
				{
					if (DateTime.Now.Subtract(previous.Timestamp) < TimeSpan.FromMinutes(5))
						hasRecent = true;
					else
						db.RestoreRequests.Remove(previous);
				}
				if (hasRecent)
				{
					await db.SaveChangesAsync();
					return null;
				}
			}

			var request = new RestoreRequest
			{
				Id = Guid.NewGuid().ToString(),
				UserId = userId,
				Timestamp = DateTime.Now
			};
			db.RestoreRequests.Add(request);
			await db.SaveChangesAsync();

			return request.Id;
		}

		public string FindUserId(string requestId)
		{
			var request = db.RestoreRequests.Find(requestId);
			return request?.UserId;
		}

		public async Task DeleteRequest(string requestId)
		{
			var request = db.RestoreRequests.Find(requestId);
			if (request == null)
				return;
			db.RestoreRequests.Remove(request);
			await db.SaveChangesAsync();
		}

		public bool ContainsRequest(string requestId)
		{
			return db.RestoreRequests.Find(requestId) != null;
		}
	}
}