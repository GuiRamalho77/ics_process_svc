
namespace InsurerServices.Providers.Hdi.Entities
{
	internal class FileHeaderModel
	{
		public FileHeaderModel()
		{
			Content = new LineFieldsModel();
		}
		public LineFieldsModel Content { get; set; }
	}
}
