using System.IO;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurerServices.CommonBusiness.Contracts
{
	public  abstract class BaseStartup
	{
		internal readonly AppSettingsConfiguration AppSettings;
		internal readonly EnumProcessType ProcessType;
		private readonly IServiceCollection Services;

		protected BaseStartup(EnumProcessType processType)
		{
			Services= new ServiceCollection();
			ConfigureServices();
		}

		private void ConfigureServices()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory());

			RegisterServices(Services, ReadConfigurationFiles(builder));

		}

		public abstract void RegisterServices(IServiceCollection services, IConfigurationRoot configuration);
		protected abstract IConfigurationRoot ReadConfigurationFiles(IConfigurationBuilder builder);


	}
}
