using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Database.Models
{
	public class Group
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		[StringLength(300)]
		public string Name { get; set; }

		[Required]
		[StringLength(64)]
		public string OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }
		
		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		/* Архивная группа не учитываются в фильтрах «Мои группы» и всегда показывается позже неархивных */
		public bool IsArchived { get; set; }
		
		[Required]
		public Guid InviteHash { get; set; }

		[Required]
		public bool IsInviteLinkEnabled { get; set; }

		[Required]
		/* Если в курсе выключена ручная проверка, то можно включить её для этой группы */
		public bool IsManualCheckingEnabled { get; set; }

		[Required]
		/* Если опция выключена, то старые решения не будут отправлены на код-ревью в момент вступления в группу */
		public bool IsManualCheckingEnabledForOldSolutions { get; set; }

		[Required]
		/* Могут ли студенты этой группы видеть сводную таблицу прогресса по курсу всех студентов группы */
		public bool CanUsersSeeGroupProgress { get; set; }
		
		[Required]
		/* Значение по умолчанию для галочки «Не принимать больше код-ревью у этого студента по этой задаче» */
		public bool DefaultProhibitFutherReview { get; set; }

		public DateTime? CreateTime { get; set; }

		public virtual ICollection<GroupMember> Members { get; set; }

		[NotMapped]
		/* TODO (andgein): Use ToListAsync()? */
		public List<GroupMember> NotDeletedMembers => Members?.Where(m => !m.User.IsDeleted).ToList() ?? new List<GroupMember>();
	}
}