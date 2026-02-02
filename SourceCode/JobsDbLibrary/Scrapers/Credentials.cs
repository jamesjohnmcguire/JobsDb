namespace JobsDb.Core.Scrapers;

public class Credentials
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string CookieData { get; set; }

	public bool IsActive { get; set; }
}
