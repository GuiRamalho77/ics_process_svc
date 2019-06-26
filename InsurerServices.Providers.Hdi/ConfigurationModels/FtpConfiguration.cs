namespace InsurerServices.Providers.Hdi.ConfigurationModels
{
	public class FtpConfiguration
	{
		
		public string Host { get; set; }
		public bool IsSftp { get; set; }
		public string RsaFileName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string RootFolder { get; set; }
		public string LookUpFileName { get; set; }
		
		public string UploadFileFolder { get; set; }
		public int LookUpCreationFileDaysInterval { get; set; }
		public int? Port { get; set; }
		public int? Timeout { get; set; } = 6000;
	}
}
