using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntiPlagiarism.Web.Database.Models
{
	public class WorkQueueItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public const string IdColumnName = "Id";

		[Required]
		public QueueIds QueueId { get; set; } // В одной таблице может лежать несколько очередей
		public const string QueueIdColumnName = "QueueId";

		[Required]
		public string ItemId { get; set; } // Id элемента другой таблицы, которому соответствует запись в очереди
		public const string ItemIdColumnName = "ItemId";

		public DateTime? TakeAfterTime { get; set; } // Устанавливается при взятии элемента из очереди. После этого времени разрешено повторно взять элемент из очереди
		public const string TakeAfterTimeColumnName = "TakeAfterTime";
	}

	public enum QueueIds
	{
		None,
		NewSubmissionsQueue
	}
}