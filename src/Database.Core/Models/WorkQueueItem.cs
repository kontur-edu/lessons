using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class WorkQueueItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public const string IdColumnName = "Id";

		[Required]
		public int QueueId { get; set; } // В одной таблице может лежать несколько очередей
		public const string QueueIdColumnName = "QueueId";

		[Required]
		public string ItemId { get; set; } // Id элемента другой таблицы, которому соответствует запись в очереди
		public const string ItemIdColumnName = "ItemId";

		public int Priority { get; set; } // Приоритет, чем больше, тем приоритетнее
		public const string PriorityColumnName = "Priority";

		public string Type { get; set; } // Некоторая информация, по которой можно фильтровать задачи
		public const string TypeColumnName = "Type";

		public DateTime? TakeAfterTime { get; set; } // Устанавливается при взятии элемента из очереди. После этого времени разрешено повторно взять элемент из очереди. Если null, то можно брать в любое время.
		public const string TakeAfterTimeColumnName = "TakeAfterTime";
	}
}