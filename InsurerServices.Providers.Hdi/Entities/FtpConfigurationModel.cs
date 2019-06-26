using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.Providers.Hdi.Entities
{
	internal class FtpConfigurationModel
	{
		internal readonly EnumProcessType FileType;
		public FtpConfigurationModel(EnumProcessType fileType)
		{
			FileType = fileType;
		}

		public bool IsSftp { get; set; }
		public string RsaFileName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public string LookUpFileName { get; set; }
		public string UploadFileFolder { get; set; }
		public int LookUpCreationFileDaysInterval { get; set; }
		public string RootFolder { get; set; }
		public string DownloadFilesFolder { get; set; }
		public int? Port { get; set; }
		public int? Timeout { get; set; }


	}
}
