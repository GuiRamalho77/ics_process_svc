using System.ComponentModel.DataAnnotations;

namespace InsurerServices.CommonBusiness.Enums
{
	public  enum EnumIntegration
	{
		[Display(Name="API")]
		Api = 0,
		[Display(Name = "TXT")]
		TxtFile = 1
	}
}
