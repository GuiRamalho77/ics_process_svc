using System.Collections.Generic;

namespace InsurerServices.Providers.Hdi.ConfigurationModels
{
	public class IssuanceFileConfiguration
	{
		public BusinessSettings BusinessSettings { get; set; }
		public Dictionary<string, FileFieldConfiguration> HeaderFields { get; set; }
		public Dictionary<string, FileFieldConfiguration> BodyFields { get; set; }
		public Dictionary<string, FileFieldConfiguration> BodyVehicleInformation { get; set; }
		public Dictionary<string, FileFieldConfiguration> FooterFields { get; set; }
	}
}
