using InsurerServices.CommonBusiness.ConfigurationModels;

namespace InsurerServices.Providers.Hdi.ConfigurationModels
{
	public class HdiProcessConfiguration
	{

		public AppSettingsConfiguration AppSettingsConfiguration { get; set; }
		public HdiProcessConfiguration()
		{}
		public bool ProcessCancelationFilesReturn { get; set; }
		public bool CreateCancelationFiles { get; set; }
		public bool SendCancelationsFiles { get; set; }
		
		public int ProposalMinimunValidLength { get; set; }
		public string ProcessedFilesFolder { get; set; }
		public string DownloadFilesFolder { get; set; }
		public string NewCancelationFilesFolder { get; set; }
		public string SendCancelationFilesFolder { get; set; }
		public LogsConfiguration LogsConfiguration { get; set; }
		public FtpConfiguration CancelationFileFtp { get; set; }
		public FtpConfiguration IssuanceFileFtp { get; set; }
		
	}
}
