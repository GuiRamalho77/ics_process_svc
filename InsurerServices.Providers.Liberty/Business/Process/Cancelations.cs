using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Contracts;

namespace InsurerServices.Providers.Liberty.Business.Process
{
	internal class Cancelations:BaseProcess
	{
		public Cancelations(AppSettingsConfiguration appSettings, ContextManagement contextManagement, Logs logs) : base(appSettings, contextManagement, logs)
		{
		}
		public void Process()
		{

		}
	}
}
