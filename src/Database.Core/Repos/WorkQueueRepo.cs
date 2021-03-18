using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;
using static Database.Models.WorkQueueItem;

namespace Database.Repos
{
	public class WorkQueueRepo : IWorkQueueRepo
	{
		private readonly UlearnDb db;

		public WorkQueueRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task Add(int queueId, string itemId, string type, int priority = 0)
		{
			db.WorkQueueItems.Add(new WorkQueueItem {QueueId = queueId, ItemId = itemId, Type = type, Priority = priority});
			await db.SaveChangesAsync();
		}

		public async Task Remove(int id)
		{
			await db.WorkQueueItems.Where(i => i.Id == id).DeleteAsync();
		}

		public async Task RemoveByItemId(int queueId, string itemId)
		{
			await db.WorkQueueItems.Where(i => i.QueueId == queueId && i.ItemId == itemId).DeleteAsync();
		}

		public async Task ReturnToQueue(int id)
		{
			await db.WorkQueueItems.Where(i => i.Id == id).UpdateAsync(c => new WorkQueueItem {TakeAfterTime = null});
		}

		// https://habr.com/ru/post/481556/
		[ItemCanBeNull]
		public async Task<WorkQueueItem> TakeNoTracking(int queueId, List<string> types, TimeSpan? timeLimit = null)
		{
			timeLimit = timeLimit ?? TimeSpan.FromMinutes(5);
			// skip locked пропускает заблокированные строки
			// for update подсказывает блокировать строки, а не страницы
			var typesCondition = types.Count == 0
				? ""
				: $"and \"{TypeColumnName}\" in ({string.Join(", ", types.Select(t => $"'{t}'"))})";
			var sql = 
$@"with next_task as (
	select ""{IdColumnName}"" from ""{nameof(db.WorkQueueItems)}""
	where ""{QueueIdColumnName}"" = @queueId
		and (""{TakeAfterTimeColumnName}"" is NULL or ""{TakeAfterTimeColumnName}"" < @now)
		{typesCondition}
	order by ""{PriorityColumnName}"" desc, ""{IdColumnName}""
	limit 1
	for update skip locked
)
update ""{nameof(db.WorkQueueItems)}""
set ""{TakeAfterTimeColumnName}"" = @timeLimit
from next_task
where ""{nameof(db.WorkQueueItems)}"".""{IdColumnName}"" = next_task.""{IdColumnName}""
returning next_task.""{IdColumnName}"", ""{QueueIdColumnName}"", ""{ItemIdColumnName}"", ""{PriorityColumnName}"", ""{TypeColumnName}"", ""{TakeAfterTimeColumnName}"";"; // Если написать *, Id возвращается дважды
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }, TransactionScopeAsyncFlowOption.Enabled))
			{
				var taken = (await db.WorkQueueItems.FromSqlRaw(
					sql,
					new NpgsqlParameter<int>("@queueId", queueId),
					new NpgsqlParameter<DateTime>("@now", DateTime.UtcNow),
					new NpgsqlParameter<DateTime>("@timeLimit", (DateTime.UtcNow + timeLimit).Value)
				).AsNoTracking().ToListAsync()).FirstOrDefault();
				scope.Complete();
				return taken;
			}
		}
	}
}