using System.Collections.Generic;

namespace InsurerServices.Providers.Hdi.Entities
{
	internal class FileModel{

		public FileModel()
		{
			VehiclesInformation = new Dictionary<string, LineFieldsModel>();
		}
		public FileHeaderModel Header { get; set; }
		public FileBodyModel Body { get; set; }
		public FileFooterModel Footer { get; set; }
		public Dictionary<string, LineFieldsModel> VehiclesInformation { get; set; }
	}
}
