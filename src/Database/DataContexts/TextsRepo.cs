using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database.Models;

namespace Database.DataContexts
{
	public class TextsRepo
	{
		private readonly ULearnDb db;
		public const int MaxTextSize = 50000;

		public TextsRepo()
			: this(new ULearnDb())
		{
		}

		public TextsRepo(ULearnDb db)
		{
			this.db = db;
		}

		public async Task<TextBlob> AddText(string text)
		{
			if (text == null)
				return new TextBlob
				{
					Hash = null,
					Text = null
				};

			if (text.Length > MaxTextSize)
				text = text.Substring(0, MaxTextSize);

			var hash = GetHash(text);
			var blob = db.Texts.Find(hash);
			if (blob != null)
				return blob;

			blob = new TextBlob
			{
				Hash = hash,
				Text = text
			};
			db.Texts.AddOrUpdate(blob);

			try
			{
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbUpdateException)
			{
				// It's ok, just tried to insert text with hash which already exists, try to find it
				if (!db.Texts.AsNoTracking().Any(t => t.Hash == hash))
					throw;
				db.Entry(blob).State = EntityState.Unchanged;
			}
			return blob;
		}

		private static string GetHash(string text)
		{
			var byteArray = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
			return BitConverter.ToString(byteArray).Replace("-", "");
		}

		public TextBlob GetText(string hash)
		{
			if (hash == null)
				return new TextBlob
				{
					Hash = null,
					Text = null
				};
			return db.Texts.Find(hash);
		}

		public Dictionary<string, string> GetTextsByHashes(IEnumerable<string> hashes)
		{
			return db.Texts.Where(t => hashes.Contains(t.Hash)).ToDictionary(t => t.Hash, t => t.Text);
		}
	}
}