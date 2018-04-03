using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Database.Models;
using uLearn.CSharp;

namespace Database.DataContexts
{
	public class StyleErrorsRepo
	{
		private readonly ULearnDb db;
		private Dictionary<StyleErrorType, bool> settingsCache;

		public StyleErrorsRepo(ULearnDb db)
		{
			this.db = db;
		}

		public async Task<bool> IsStyleErrorEnabledAsync(StyleErrorType errorType)
		{
			await CacheSettingsAsync();
			if (settingsCache.ContainsKey(errorType))
				return settingsCache[errorType];

			/* By default all style validation errors are enabled */ 
			return true;
		}

		public async Task<Dictionary<StyleErrorType, bool>> GetStyleErrorSettingsAsync()
		{
			await CacheSettingsAsync();
			var allErrorTypes = typeof(StyleErrorType).GetEnumValues().Cast<StyleErrorType>();
			
			var result = new Dictionary<StyleErrorType, bool>();
			foreach (var errorType in allErrorTypes)
				result[errorType] = await IsStyleErrorEnabledAsync(errorType);
			
			return result;
		}

		public async Task EnableStyleErrorAsync(StyleErrorType errorType, bool isEnabled)
		{
			var oldSettings = await db.StyleErrorSettings.FirstOrDefaultAsync(s => s.ErrorType == errorType);
			if (oldSettings == null)
				db.StyleErrorSettings.Add(new StyleErrorSettings
				{
					ErrorType = errorType,
					IsEnabled = isEnabled,
				});
			else
			{
				oldSettings.IsEnabled = isEnabled;
			}

			await db.SaveChangesAsync();
		}

		private async Task CacheSettingsAsync()
		{
			if (settingsCache == null)
				settingsCache = await db.StyleErrorSettings.ToDictionaryAsync(s => s.ErrorType, s => s.IsEnabled);
		}
	}
}