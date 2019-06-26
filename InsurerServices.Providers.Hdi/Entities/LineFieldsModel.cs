using System.Collections.Generic;
using InsurerServices.Providers.Hdi.Business.FileProcess;

namespace InsurerServices.Providers.Hdi.Entities
{
	internal class LineFieldsModel
	{
		public LineFieldsModel()
		{
			Fields=new Dictionary<string, Field>();
		}
		public  Dictionary<string, Field> Fields { get; set; }
		public int LineNumber { get; set; }
	}
}
