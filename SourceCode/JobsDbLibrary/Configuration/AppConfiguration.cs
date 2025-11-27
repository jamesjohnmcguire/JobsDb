namespace JobsDb.Core.Configuration
{
	public class AppConfiguration
	{
		public string MasterPassword { get; set; }
		public List<CredentialConfig> Credentials { get; set; }
		public SearchDefaultsConfig SearchDefaults { get; set; }
		public ScraperSettingsConfig ScraperSettings { get; set; }
	}
}
