using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsDbLibrary.Scrapers
{
	public class ScraperCredential
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Source { get; set; } // "LinkedIn", "TokyoDev"

		[Required]
		public string Username { get; set; }

		[Required]
		public string EncryptedPassword { get; set; }

		public string CookieData { get; set; }

		public DateTime? LastUsed { get; set; }

		public bool IsActive { get; set; }
	}
}
