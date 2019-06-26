using System.Collections.Generic;

namespace InsurerServices.Providers.Hdi.ConfigurationModels
{
	public class BusinessSettings
	{
		public BusinessSettings()
		{
			DefaultValues= new Dictionary<string, string>();
		}
		public string HeaderIdentification { get; set; }
		public string BodyIdentification { get; set; }
		public string FooterIdentification { get; set; }
		public string BodyVehicleInformationIdentification { get; set; }
		public Dictionary<string,string> DefaultValues { get; set; }

	}
}
