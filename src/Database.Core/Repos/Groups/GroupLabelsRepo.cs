namespace Database.Repos.Groups
{
	/* Group labels are not used now. Maybe they will be used in future */
	public class GroupLabelsRepo
	{
		/*
		public Task<List<GroupLabel>> GetLabelsAsync(string ownerId)
		{
			return db.GroupLabels.Where(l => !l.IsDeleted && l.OwnerId == ownerId).ToListAsync();
		}

		public async Task<GroupLabel> CreateLabelAsync(string ownerId, string name, string colorHex)
		{
			var label = new GroupLabel
			{
				OwnerId = ownerId,
				Name = name,
				IsDeleted = false,
				ColorHex = colorHex
			};
			db.GroupLabels.Add(label);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return label;
		}

		public async Task AddLabelToGroupAsync(int groupId, int labelId)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				if (db.LabelsOnGroups.Any(g => g.LabelId == labelId && g.GroupId == groupId))
					return;

				var labelOnGroup = new LabelOnGroup
				{
					GroupId = groupId,
					LabelId = labelId,
				};
				db.LabelsOnGroups.Add(labelOnGroup);
				await db.SaveChangesAsync().ConfigureAwait(false);

				transaction.Commit();
			}
		}

		public async Task RemoveLabelFromGroupAsync(int groupId, int labelId)
		{
			var labels = db.LabelsOnGroups.Where(g => g.LabelId == labelId && g.GroupId == groupId);
			db.LabelsOnGroups.RemoveRange(labels);
			
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public Task<List<int>> GetGroupsWithSpecificLabelAsync(int labelId)
		{
			return db.LabelsOnGroups.Where(l => l.LabelId == labelId).Select(l => l.GroupId).ToListAsync();
		}

		public async Task<DefaultDictionary<int, List<int>>> GetGroupsLabelsAsync(IEnumerable<int> groupsIds)
		{
			var groupsIdsSet = new HashSet<int>(groupsIds);
			var groupLabels = await db.LabelsOnGroups
				.Where(l => groupsIdsSet.Contains(l.GroupId))
				.GroupBy(l => l.GroupId)
				.ToDictionaryAsync(g => g.Key, g => g.Select(l => l.LabelId).ToList())
				.ConfigureAwait(false);
			
			return groupLabels.ToDefaultDictionary();
		}

		[ItemCanBeNull]
		public Task<GroupLabel> FindLabelByIdAsync(int labelId)
		{
			return db.GroupLabels.FindAsync(labelId);
		}
		*/
	}
}