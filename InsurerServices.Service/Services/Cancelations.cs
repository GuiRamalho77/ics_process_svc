using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Hdi.Services;

namespace InsurerServices.Service.Services
{
	internal class Cancelations
	{

		public static void Process()
		{
			//HDI
			new PolicyFilesService(EnumProcessType.Cancelation).ProcessFiles();
		}

	}
}
