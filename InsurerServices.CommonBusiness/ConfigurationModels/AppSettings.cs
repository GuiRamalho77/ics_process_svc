using System.Collections.Generic;
using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.ConfigurationModels
{
	public  class AppSettingsConfiguration
	{
		public AppSettingsConfiguration()
		{
			ActiveIssuanceInsurers = new List<EnumInsurer>();
			ActiveCancelationInsurers = new List<EnumInsurer>();
		}
		
		public List<EnumInsurer> ActiveIssuanceInsurers { get; set; }
		public List<EnumInsurer> ActiveCancelationInsurers { get; set; }
		public DatabaseConfiguration DatabaseConfiguration { get; set; }
		public IcsModuleConfigurations IcsModuleConfigurations { get; set; }
	}
}
