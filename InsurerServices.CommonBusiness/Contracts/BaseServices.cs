using System.IO;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Data;
using InsurerServices.CommonBusiness.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurerServices.CommonBusiness.Contracts
{
	public abstract class BaseServices
	{
		protected AppSettingsConfiguration AppSettingsConfiguration;
		protected readonly EnumProcessType ProcessType;
		private readonly IServiceCollection Services;

		protected BaseServices(EnumProcessType processType)
		{
			Services = new ServiceCollection();
			ProcessType = processType;
			ConfigureServices();
		}

		private void ConfigureServices()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory());
			builder.AddJsonFile($"ConfigurationFiles\\appsettings.json", optional: false);

			var configuration = builder.Build();
			AppSettingsConfiguration = configuration.GetSection("AppSettings").Get<AppSettingsConfiguration>();
			Services.AddEntityFrameworkSqlServer().AddDbContext<Context>(options =>
				options.UseSqlServer(AppSettingsConfiguration.DatabaseConfiguration.ConnectionString,
					dbBuilder => dbBuilder.CommandTimeout(AppSettingsConfiguration.DatabaseConfiguration.CommandTimeout ?? 50000)));

			
			Services.AddTransient<AppSettingsConfiguration>(provider => AppSettingsConfiguration);
			Services.AddTransient<ContextManagement>();
			Services.AddTransient<IcsModuleConfigurations>();
			RegisterServices(Services, ReadConfigurationFiles(builder));
		}

		protected abstract void RegisterServices(IServiceCollection services, IConfigurationRoot configuration);
		protected abstract IConfigurationRoot ReadConfigurationFiles(IConfigurationBuilder builder);
		protected void RegisterLogs(LogsConfiguration configuration) => Services.AddSingleton<Logs>(provider => new Logs(configuration,ProcessType));

		protected TService GetService<TService>()=> Services.BuildServiceProvider().GetService<TService>();
	}
}

