using InsurerServices.CommonBusiness;
using InsurerServices.Providers.Hdi.ConfigurationModels;

namespace InsurerServices.Providers.Hdi.Contracts
{
	internal abstract class BaseBusiness
	{
		internal readonly HdiProcessConfiguration HdiProcessConfigurations;
		internal readonly Logs Logs;

		protected BaseBusiness(HdiProcessConfiguration hdiSettings, Logs logs)
		{
			HdiProcessConfigurations = hdiSettings;
			Logs = logs;
		}
	}
}
