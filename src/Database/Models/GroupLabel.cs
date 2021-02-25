using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupLabel
	{
		private const string Owner_IsDeleted = "Owner_IsDeleted";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("Owner")]
		[Index(Owner_IsDeleted, 1)]
		public string OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }

		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(6)]
		public string ColorHex { get; set; }

		[Required]
		[Index(Owner_IsDeleted, 2)]
		public bool IsDeleted { get; set; }
	}

	public class LabelOnGroup
	{
		private const string Group_Label = "Group_Label";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("Group")]
		[Index(Group_Label, 1, IsUnique = true)]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		[Index("Label")]
		[Index(Group_Label, 2, IsUnique = true)]
		public int LabelId { get; set; }

		public virtual GroupLabel Label { get; set; }
	}
}