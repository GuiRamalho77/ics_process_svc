using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Hdi.Services;
using InsurerServices.Providers.Liberty.Services;

namespace InsurerServices.Service.Services
{
	public class Issuances
	{
		public static void Process()
		{
			//HDI
			new PolicyFilesService(EnumProcessType.Issuance).ProcessFiles();

			//LIBERTY 
			 new PolicyService(EnumProcessType.Issuance).Process();
		}
	}
}
