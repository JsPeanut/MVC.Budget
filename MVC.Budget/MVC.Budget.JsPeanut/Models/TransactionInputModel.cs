using System.ComponentModel.DataAnnotations;

namespace MVC.Budget.JsPeanut.Models
{
	public class TransactionInputModel
	{
		public int Id { get; set; }
		[Required]
		public DateTime Date { get; set; }
		[StringLength(60, MinimumLength = 3)]
		[Required]
		public string Name { get; set; }
		[Range(0, int.MaxValue, ErrorMessage = "The value must be positive.")]
		[DataType(DataType.Currency)]
		[Required]
		public decimal Value { get; set; }
		[StringLength(120)]
		[Required]
		public string Description { get; set; }
		public int CategoryId { get; set; }
		public string UserId { get; set; }
	}
}
