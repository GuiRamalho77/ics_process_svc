

namespace InsurerServices.CommonBusiness.ConfigurationModels
{
	public class LogsConfiguration
	{
		public bool DisableErrorLog { get; set; }
		public bool EnableInformationalLog { get; set; }
		public string CancelationFileLogsFolder{ get; set; }
		public string IsuanceFileLogsFolder { get; set; }
		public string ExceptionsLogsFolder { get; set; }

	}
}
