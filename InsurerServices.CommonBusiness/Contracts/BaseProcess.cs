using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.Contracts
{
	public abstract class BaseProcess
	{
		public readonly AppSettingsConfiguration AppSettings;
		public readonly ContextManagement ContextManagement;
		public readonly EnumProcessType ProcessType;
		public readonly Logs Logs;

		protected BaseProcess(AppSettingsConfiguration appSettings, ContextManagement contextManagement, Logs logs)
		{
			AppSettings = appSettings;
			ContextManagement = contextManagement;
			ProcessType = logs.ProcessType;
			Logs = logs;
		}

	}
}
