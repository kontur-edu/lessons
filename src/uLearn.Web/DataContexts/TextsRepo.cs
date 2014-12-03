using System;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
	public class TextsRepo
	{
		private readonly ULearnDb db;

		public TextsRepo() : this(new ULearnDb())
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

			var hash = getHash(text);
			var inBase = db.Texts.Find(hash);
			if (inBase != null)
				return inBase;
			var blob = db.Texts.Add(new TextBlob
			{
				Hash = hash,
				Text = text
			});

			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbEntityValidationException e)
			{
				Debug.Write(
					string.Join("\r\n",
					e.EntityValidationErrors.SelectMany(v => v.ValidationErrors).Select(err => err.PropertyName + " " + err.ErrorMessage)));
			}
			return blob;
		}

		private string getHash(string text)
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
	}
}