namespace InsurerServices.CommonBusiness.ConfigurationModels
{
	public class DatabaseConfiguration
	{
		public string ConnectionString { get; set; }
		public int? CommandTimeout { get; set; }
	}
}
