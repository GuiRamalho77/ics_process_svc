using System;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Liberty.Business.Process;
using InsurerServices.Providers.Liberty.ConfigurationModels;
using InsurerServices.Providers.Liberty.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurerServices.Providers.Liberty.Services
{
	public class PolicyService:BaseServices
	{
		private LibertyProcessConfiguration _libertyProcessConfiguration;
		public PolicyService(EnumProcessType processType) : base(processType)
		{
		}

		protected override void RegisterServices(IServiceCollection services, IConfigurationRoot configuration)
		{
			_libertyProcessConfiguration = configuration.GetSection("LibertyProcessConfigurations").Get<LibertyProcessConfiguration>();
			_libertyProcessConfiguration.AppSettingsConfiguration = AppSettingsConfiguration;
			RegisterLogs(_libertyProcessConfiguration.LogsConfiguration);
			services.AddTransient<LibertyProcessConfiguration>(provider => _libertyProcessConfiguration);
			services.AddTransient<IcsIsurerConfigManagement>();
			services.AddTransient<Issuances>();
			services.AddTransient<Cancelations>();



		}

		protected override IConfigurationRoot ReadConfigurationFiles(IConfigurationBuilder builder)
		{
			builder.AddJsonFile($"ConfigurationFiles\\LIBERTY\\configuration.json", optional: false);
			return builder.Build();
		}


		public void Process()
		{
			switch (ProcessType)
			{
				case EnumProcessType.Issuance:
					if (_libertyProcessConfiguration.AppSettingsConfiguration.ActiveIssuanceInsurers.Contains(EnumInsurer.Liberty))
						GetService<Issuances>().Process();
					break;
				case EnumProcessType.Cancelation:
					if (_libertyProcessConfiguration.AppSettingsConfiguration.ActiveCancelationInsurers.Contains(EnumInsurer.Liberty))
						GetService<Cancelations>().Process();
					break;
				default:
					Logs.AddException(new Exception("processo_nao_desenvolvido"),
						_libertyProcessConfiguration.LogsConfiguration);
					break;
			}
		}

	}
}
