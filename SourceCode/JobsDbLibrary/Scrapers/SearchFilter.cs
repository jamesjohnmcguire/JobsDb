namespace JobsDb.Core.Scrapers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SearchFilter
{
	[Key]
	public int Id { get; set; }

	[Required]
	public string Name { get; set; }

	public string Keywords { get; set; }

	public string Location { get; set; }

	public string Source { get; set; }

	public bool IsActive { get; set; }

	public DateTime CreatedDate { get; set; }

	public DateTime? LastRunDate { get; set; }
}

