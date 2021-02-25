using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class EnabledAdditionalScoringGroup
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("Group", 2)]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		public string ScoringGroupId { get; set; }
	}
}