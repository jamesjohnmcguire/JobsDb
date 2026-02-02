using System;

namespace JobsDb.Core.Models
{
	using System;

	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	public class Job
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		public string Company { get; set; }

		public string Location { get; set; }

		public string Description { get; set; }

		public string Requirements { get; set; }

		[Required]
		public string Source { get; set; } // "LinkedIn", "TokyoDev", etc.

		[Required]
		public string SourceUrl { get; set; }

		public string SourceJobId { get; set; }

		public decimal? SalaryMin { get; set; }

		public decimal? SalaryMax { get; set; }

		public string SalaryCurrency { get; set; }

		public string JobType { get; set; } // Full-time, Part-time, Contract, etc.

		public string RemoteType { get; set; } // On-site, Remote, Hybrid

		public DateTime DatePosted { get; set; }

		public DateTime DateScraped { get; set; }

		public DateTime? DateApplied { get; set; }

		public ApplicationStatus Status { get; set; }

		public int? Priority { get; set; } // 1-5 rating

		public string Notes { get; set; }

		public bool IsArchived { get; set; }
	}
}

