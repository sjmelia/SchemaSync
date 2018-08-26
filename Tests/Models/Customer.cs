using Postulate.Lite.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Tests.Models
{
	[Identity(nameof(Id))]
	public class Customer
	{
		public int Id { get; set; }

		[MaxLength(100)]
		[Required]
		public string Name { get; set; }

		[MaxLength(100)]
		public string Address { get; set; }

		[MaxLength(50)]
		public string City { get; set; }

		[MaxLength(2)]
		public string State { get; set; }

		[MaxLength(10)]
		public string ZipCode { get; set; }

		[DecimalPrecision(2, 3)]
		public decimal? Discount { get; set; }

		public DateTime StartDate { get; set; }

		public bool IsActive { get; set; } = true;
	}
}