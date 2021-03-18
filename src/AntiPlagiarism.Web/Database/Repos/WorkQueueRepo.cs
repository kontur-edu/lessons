﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AntiPlagiarism.Web.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static AntiPlagiarism.Web.Database.Models.WorkQueueItem;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace AntiPlagiarism.Web.Database.Repos
{
	public interface IWorkQueueRepo
	{
		Task Add(QueueIds queueId, string itemId);
		Task<WorkQueueItem> TakeNoTracking(QueueIds queueId, TimeSpan? timeLimit = null);
		Task Remove(int id);
	}
	
	public class WorkQueueRepo : IWorkQueueRepo
	{
		private readonly AntiPlagiarismDb db;

		public WorkQueueRepo(AntiPlagiarismDb db)
		{
			this.db = db;
		}

		public async Task Add(QueueIds queueId, string itemId)
		{
			db.WorkQueueItems.Add(new WorkQueueItem {QueueId = queueId, ItemId = itemId});
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task Remove(int id)
		{
			var itemToRemove = db.WorkQueueItems.SingleOrDefault(x => x.Id == id);
			if (itemToRemove != null) {
				db.WorkQueueItems.Remove(itemToRemove);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		// https://habr.com/ru/post/481556/
		public async Task<WorkQueueItem> TakeNoTracking(QueueIds queueId, TimeSpan? timeLimit = null)
		{
			timeLimit = timeLimit ?? TimeSpan.FromMinutes(5);
			// skip locked пропускает заблокированные строки
			// for update подсказывает блокировать строки, а не страницы
			var sql = 
$@"with next_task as (
	select ""{IdColumnName}"" from {AntiPlagiarismDb.DefaultSchema}.""{nameof(db.WorkQueueItems)}""
	where ""{QueueIdColumnName}"" = @queueId
		and (""{TakeAfterTimeColumnName}"" is NULL or ""{TakeAfterTimeColumnName}"" < @now)
	order by ""{IdColumnName}""
	limit 1
	for update skip locked
)
update {AntiPlagiarismDb.DefaultSchema}.""{nameof(db.WorkQueueItems)}""
set ""{TakeAfterTimeColumnName}"" = @timeLimit
from next_task
where {AntiPlagiarismDb.DefaultSchema}.""{nameof(db.WorkQueueItems)}"".""{IdColumnName}"" = next_task.""{IdColumnName}""
returning next_task.""{IdColumnName}"", ""{QueueIdColumnName}"", ""{ItemIdColumnName}"", ""{TakeAfterTimeColumnName}"";"; // Если написать *, Id возвращается дважды
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }, TransactionScopeAsyncFlowOption.Enabled))
			{
				var taken = (await db.WorkQueueItems.FromSqlRaw(
					sql,
					new NpgsqlParameter<int>("@queueId", (int)queueId),
					new NpgsqlParameter<DateTime>("@now", DateTime.UtcNow),
					new NpgsqlParameter<DateTime>("@timeLimit", (DateTime.UtcNow + timeLimit).Value)
				).AsNoTracking().ToListAsync()).FirstOrDefault();
				scope.Complete();
				return taken;
			}
		}
	}
}