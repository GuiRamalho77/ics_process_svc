using System.Collections.Generic;

namespace InsurerServices.CommonBusiness.ConfigurationModels
{
	public class IcsModuleConfigurations
	{
		public IcsModuleConfigurations()
		{
			Apis = new Dictionary<string, ApiConfiguration>();
		}

		public string BaseUrl { get; set; }
		public Dictionary<string, ApiConfiguration> Apis { get; set; }
	}
}
