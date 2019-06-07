using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ulearn.Common
{
	public class LruCache<TKey, TValue>
	{
		private readonly int capacity;
		private readonly TimeSpan maxLifeTime;
		
		private readonly Dictionary<TKey, LinkedListNode<LruCacheItem<TKey, TValue>>> cache = new Dictionary<TKey, LinkedListNode<LruCacheItem<TKey, TValue>>>();
		private readonly LinkedList<LruCacheItem<TKey, TValue>> lastUsedItems = new LinkedList<LruCacheItem<TKey, TValue>>();

		public LruCache(int capacity, TimeSpan maxLifeTime)
		{
			this.capacity = capacity;
			this.maxLifeTime = maxLifeTime;
		}

		public LruCache(int capacity)
			:this(capacity, TimeSpan.MaxValue)
		{
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool TryGet(TKey key, out TValue value)
		{
			value = default(TValue);

			if (!cache.TryGetValue(key, out var node))
				return false;

			if (IsItemTooOld(node.Value))
			{
				lastUsedItems.Remove(node);
				cache.Remove(key);
				return false;
			}
			
			value = node.Value.Value;
			lastUsedItems.Remove(node);
			lastUsedItems.AddLast(node);
			return true;
		}

		private bool IsItemTooOld(LruCacheItem<TKey, TValue> cacheItem)
		{
			var now = DateTime.Now;
			return now - cacheItem.AddingTime > maxLifeTime;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Add(TKey key, TValue val)
		{
			if (cache.Count >= capacity)
				RemoveFirst();

			var cacheItem = new LruCacheItem<TKey, TValue>(key, val);
			var node = new LinkedListNode<LruCacheItem<TKey, TValue>>(cacheItem);
			lastUsedItems.AddLast(node);
			cache.Add(key, node);
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Clear()
		{
			cache.Clear();
			lastUsedItems.Clear();
		}

		private void RemoveFirst()
		{
			var node = lastUsedItems.First;
			lastUsedItems.RemoveFirst();

			/* Remove from cache */
			cache.Remove(node.Value.Key);
		}
	}

	internal class LruCacheItem<TKey, TValue>
	{
		public readonly TKey Key;
		public readonly TValue Value;
		public readonly DateTime AddingTime;

		public LruCacheItem(TKey k, TValue v)
		{
			Key = k;
			Value = v;
			AddingTime = DateTime.Now;
		}
	}
}