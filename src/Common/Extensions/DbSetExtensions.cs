using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ulearn.Common.Extensions
{
	public static class DbSetExtensions
	{
		public static EntityEntry<TEntity> AddOrUpdate<TEntity>(this DbContext dbContext, TEntity entity, Expression<Func<TEntity, bool>> findFunction) where TEntity : class
		{
			var dbSet = dbContext.Set<TEntity>();
			var exists = dbSet.Any(findFunction);

			EntityEntry<TEntity> entry;
			if (!exists)
			{
				entry = dbContext.Add(entity);
				entry.State = EntityState.Added;
				return entry;
			}

			var dbValue = dbSet.FirstOrDefault(findFunction);
			entry = dbContext.Entry(dbValue);
			entry.CurrentValues.SetValues(entity);
			entry.State = EntityState.Modified;
			return entry;
		}
	}
}