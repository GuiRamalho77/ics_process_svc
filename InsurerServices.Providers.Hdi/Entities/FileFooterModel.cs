
namespace InsurerServices.Providers.Hdi.Entities
{
	internal class FileFooterModel
	{
		public FileFooterModel()
		{
			Content = new LineFieldsModel();
		}
		public LineFieldsModel Content { get; set; }
	}
}
