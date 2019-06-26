using System.Collections.Generic;

namespace InsurerServices.Providers.Hdi.Entities
{
	internal class FileBodyModel
	{
		public FileBodyModel()
		{
			Content = new List<LineFieldsModel>();
		}
		public List<LineFieldsModel> Content { get; set; }
	}
}
