using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CourseFile
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Index("CourseVersion", 1)]
		public Guid CourseVersionId { get; set; }

		public virtual CourseVersion CourseVersion { get; set; }

		[Required]
		public byte[] File { get; set; }
	}
}