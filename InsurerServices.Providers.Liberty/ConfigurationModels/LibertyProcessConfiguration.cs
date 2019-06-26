using InsurerServices.CommonBusiness.ConfigurationModels;

namespace InsurerServices.Providers.Liberty.ConfigurationModels
{
	public class LibertyProcessConfiguration
	{
		public LogsConfiguration LogsConfiguration { get; set; }
		public AppSettingsConfiguration AppSettingsConfiguration { get; set; }
	}
}
