using System;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Hdi.Business;
using InsurerServices.Providers.Hdi.Business.FileProcess;
using InsurerServices.Providers.Hdi.Business.Process;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurerServices.Providers.Hdi.Services
{
	public class PolicyFilesService : BaseServices
	{
		private HdiProcessConfiguration _hdiProcessConfiguration;
		public PolicyFilesService(EnumProcessType processType) : base(processType)
		{
		}

		 protected override void RegisterServices(IServiceCollection services, IConfigurationRoot configuration)
		{
			_hdiProcessConfiguration = configuration.GetSection("HdiProcessConfigurations").Get<HdiProcessConfiguration>();
			_hdiProcessConfiguration.AppSettingsConfiguration = AppSettingsConfiguration;
			var issuanceFileConfiguration = configuration.GetSection("IssuanceFileConfiguration").Get<IssuanceFileConfiguration>();
			var cancelationFileConfiguration = configuration.GetSection("CancelationFileConfiguration").Get<CancelationFileConfiguration>();
			
			
			services.AddTransient<FileManager>();
			services.AddTransient<ValidationRules>();
			services.AddTransient<ContextManagement>();
			services.AddTransient<IcsModuleIntegration>();
			RegisterLogs(_hdiProcessConfiguration.LogsConfiguration);
			services.AddTransient<HdiProcessConfiguration>(provider => _hdiProcessConfiguration);
			switch (ProcessType)
			{
				case EnumProcessType.Issuance:
					services.AddTransient<IssuanceFileConfiguration>(provider => issuanceFileConfiguration);
					services.AddTransient<Issuances>();
					break;
				case EnumProcessType.Cancelation:
					services.AddTransient<CancelationFileConfiguration>(provider =>cancelationFileConfiguration);
					services.AddTransient<Cancelations>();
					break;
				default:
					Logs.AddException(new Exception("processo_nao_desenvolvido"),
						_hdiProcessConfiguration.LogsConfiguration);
					break;
			}

			
		}

		protected override IConfigurationRoot ReadConfigurationFiles(IConfigurationBuilder builder)
		{
			builder.AddJsonFile($"ConfigurationFiles\\HDI\\configuration.json", optional: false);
			builder.AddJsonFile($"ConfigurationFiles\\HDI\\cancelationFile.json", optional: false);
			builder.AddJsonFile($"ConfigurationFiles\\HDI\\issuanceFile.json", optional: false);
			return builder.Build();
		}
		public void ProcessFiles()
		{
			switch (ProcessType)
			{
				case EnumProcessType.Issuance:
					if (_hdiProcessConfiguration.AppSettingsConfiguration.ActiveIssuanceInsurers.Contains(EnumInsurer.Hdi))
						GetService<Issuances>().ProcessFiles();
					break;
				case EnumProcessType.Cancelation:
					if (_hdiProcessConfiguration.AppSettingsConfiguration.ActiveCancelationInsurers.Contains(EnumInsurer.Hdi))
						GetService<Cancelations>().ProcessFiles();
					break;
				default:
					Logs.AddException(new Exception("processo_nao_desenvolvido"),
						_hdiProcessConfiguration.LogsConfiguration);
					break;
			}
		}
	}
}
