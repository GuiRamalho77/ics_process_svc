using System;
using System.Globalization;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Liberty.ConfigurationModels;
using InsurerServices.Providers.Liberty.Data;
using InsurerServices.Providers.Liberty.Entities;
using InsurerServices.Providers.Liberty.Enums;
using Ituran.Framework.Comum.HttpClient;
using Ituran.Framework.Comum.Response;
using Ituran.Framework.Data.Entities;
using Newtonsoft.Json;

namespace InsurerServices.Providers.Liberty.Business.Process
{
	internal class Issuances : BaseProcess
	{
		private readonly IcsIsurerConfigManagement _icsIsurerConfig;
	    private readonly IcsModuleConfigurations _icsConfiguration;

        public Issuances(AppSettingsConfiguration appSettings, ContextManagement contextManagement, Logs logs, IcsIsurerConfigManagement icsIsurerConfig, LibertyProcessConfiguration libertyConfiguration) : base(
			appSettings, contextManagement, logs)
		{
			_icsIsurerConfig = icsIsurerConfig;
		    _icsConfiguration = libertyConfiguration.AppSettingsConfiguration.IcsModuleConfigurations;
        }

		public void Process()
		{
			try
			{
				Logs.Add($"Inicio Teste - [Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}]");
				SEGURADORAICSCONFIG seguradoraIcs =_icsIsurerConfig.GetConfiguration(EnumIntegrationConfigLiberty.EmissaoApolice);
				if (seguradoraIcs == null)
				{
					Logs.Add($"Configuração nula",EnumLog.Error);
					return;
				}

				ServiceConfigurationIcs serviceConfigurationIcs = new ServiceConfigurationIcs();
				serviceConfigurationIcs = JsonConvert.DeserializeObject<ServiceConfigurationIcs>(seguradoraIcs.JS_REGRAS);
				serviceConfigurationIcs.Config = JsonConvert.DeserializeObject<ServiceConfigurationIcs>(seguradoraIcs.JS_CONFIG).Config;

				if (ExecutaServico(serviceConfigurationIcs))
					EmissaoApolice(seguradoraIcs, serviceConfigurationIcs);
			}
			catch (Exception ex)
			{
				Logs.Add($"Inicio - [Servico de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()} - Erro: { ex.InnerException.Message} - { ex.StackTrace}", EnumLog.Error);
			}
			finally
			{
				Logs.Add($"Fim - [Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}]");
				Logs.Save(EnumInsurer.Liberty);
			}

			
		}
		private void EmissaoApolice(SEGURADORAICSCONFIG seguradoraIcs, ServiceConfigurationIcs serviceConfigurationIcs)
		{
			try
			{
				DateTime currentDateToRun = Convert.ToDateTime(serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO).AddDays(1);

				if (currentDateToRun.Date == DateTime.Today)
				{
					Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. EmissaoApolice - Fim do fluxo de Consulta de Emissões.");
					return;
				}

       //         var apiConfigurationTeste = _icsConfiguration.Apis["UpdatePolicyPendingCancelation"];
       //         var restfulClientTeste = new RestfulClient<dynamic>(_icsConfiguration.BaseUrl, apiConfigurationTeste.Suffix, timeOut: apiConfigurationTeste.Timeout);
       //         var resultTeste = restfulClientTeste.Save(new
       //         {
       //             CD_APOLICE = 123456,
       //             MOTIVO = $"Apolice enviada para cancelamento, aguardando retorno seguradora.",
       //             NM_SISTEMA_ALTERACAO = $"ICS_PROCESS_SVC",
       //             NM_USUARIO_ALTERACAO = $"INTEGRACAO",
       //             validarRegras = false,
       //             NRSTATUS = 8
       //         });
       //         var retornoTeste = JsonConvert.DeserializeObject<dynamic>(resultTeste.ToString());


			    //var apiConfigurationTeste2 = _icsConfiguration.Apis["UpdatePolicyPendingCancelation"];
			    //var restfulClientTeste2 = new Dominio.Modulo.Insurance.Liberty.HttpClient.RestfulClient<dynamic>("http://api1.ituran.sp/insurance", $"Apolice/Renovacao/{123456}", timeOut: apiConfigurationTeste2.Timeout);
			    //var resultTeste2 = restfulClientTeste2.Get();
			    //var retornoTeste2 = JsonConvert.DeserializeObject<dynamic>(resultTeste.ToString());


                var apiConfiguration = _icsConfiguration.Apis["ConsultarEmissaoLiberty"];

			    Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Inicio da requisição para API: {_icsConfiguration.BaseUrl}/{apiConfiguration.Suffix}{(int)EnumInsurer.Liberty},{currentDateToRun:yyyy-MM-dd}/{serviceConfigurationIcs.PAGINA}");

			    var restfulClient = new CommonBusiness.Integrations.RestfulClient<JsonDataResponse>(_icsConfiguration.BaseUrl, apiConfiguration.Suffix, timeOut: apiConfiguration.Timeout);

                var result = restfulClient.Save(new
			    {
			        CD_PESSOA_SEGURADORA = (int)EnumInsurer.Liberty,
                    DT_FILTRO = currentDateToRun.ToDateTimeString("yyyy-MM-dd'T'HH:mm:ss"),
                    //DT_FILTRO = currentDateToRun,
                    serviceConfigurationIcs.PAGINA
                });

			    Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Retorno da Consulta de Emissões. ");

                if (result?.Data == null || string.IsNullOrWhiteSpace(result.Data.ToString()))
					Logs.Add($"[Serviço de Emissão { EnumInsurer.Liberty.GetDescriptionEnum() }. Data Filtro: {serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO} - Erro: Retorno Modulo ICS Inválido ");

				else
				{
					ResponseData retorno = JsonConvert.DeserializeObject<ResponseData>(result.Data.ToString());

					if (retorno.Success)
					{
						serviceConfigurationIcs.Config.DT_PROXIMA_RODADA = Convert.ToDateTime($"{DateTime.Now.AddDays(1):yyyy-MM-dd} 10:00").ToString();
						serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO = $"{currentDateToRun.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

						_icsIsurerConfig.UpdateIcsIsurerConfig(seguradoraIcs, serviceConfigurationIcs);

						Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Data Filtro: {serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO} - Data Execução: {DateTime.Now} - Finalizado com Sucesso.");
						
						if (currentDateToRun < DateTime.Now.Date && serviceConfigurationIcs.Config.EXECUTA_PERIODO)
						{
							Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Retorna ao inicio do fluxo, para consultar o próximo dia.");
							EmissaoApolice(seguradoraIcs, serviceConfigurationIcs);
							_icsIsurerConfig.UpdateIcsIsurerConfig(seguradoraIcs, serviceConfigurationIcs);
						}
					}

					else if (retorno.Success == false)
					{
						if (retorno.Page == 0)
						{
							serviceConfigurationIcs.Config.DT_PROXIMA_RODADA = Convert
								.ToDateTime(serviceConfigurationIcs.Config.DT_PROXIMA_RODADA)
								.GetNextDateFromInterval(serviceConfigurationIcs.Config.NR_INTERVALO).ToString("dd/MM/yyyy HH:mm");
							Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Data Filtro: {serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO} - Data Execução: {DateTime.Now} - Erro - {retorno.Error}");
						}
						else
						{
							serviceConfigurationIcs.Config.DT_PROXIMA_RODADA = Convert.ToDateTime(serviceConfigurationIcs.Config.DT_PROXIMA_RODADA).GetNextDateFromInterval(serviceConfigurationIcs.Config.NR_INTERVALO).ToString("dd/MM/yyyy HH:mm");
							serviceConfigurationIcs.PAGINA = retorno.Page;
							_icsIsurerConfig.UpdateIcsIsurerConfig(seguradoraIcs, serviceConfigurationIcs);

							Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Data Filtro: {serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO} - Data Execução: {DateTime.Now} - Erro - Falha ao tentar consultar a página {retorno.Page} da API. - {retorno.Error}");
						}

						if (currentDateToRun < DateTime.Now)
						{
							serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO =
								Convert.ToDateTime($"{currentDateToRun:yyyy-MM-dd} 10:00").ToString();
							_icsIsurerConfig.UpdateIcsIsurerConfig(seguradoraIcs, serviceConfigurationIcs);
							EmissaoApolice(seguradoraIcs, serviceConfigurationIcs);
						}
						_icsIsurerConfig.UpdateIcsIsurerConfig(seguradoraIcs, serviceConfigurationIcs);
					}
				}
			}
			catch (Exception ex)
			{
				Logs.Add($"[Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. Data Filtro: {serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO} - Data Execução: {DateTime.Now} - Erro: {ex.Message} / {ex.InnerException}");
			}
		}

		private bool ExecutaServico(ServiceConfigurationIcs serviceConfigurationIcs)
		{
			
			//if (!serviceConfigurationIcs.Config.STATUS)
			//{
			//	Logs.Add($"Fim - [Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. O Serviço de Emissão está desativado. Verifique a tabela de configurações.");
			//	return false;
			//}

			if (string.IsNullOrEmpty(serviceConfigurationIcs.Config.DT_PROXIMA_RODADA) ||
			    Convert.ToDateTime(serviceConfigurationIcs.Config.DT_PROXIMA_RODADA) > DateTime.Now)
			{
				Logs.Add($"Fim - [Serviço de Emissão {EnumInsurer.Liberty.GetDescriptionEnum()}. A próxima execução está programada para acontecer após {serviceConfigurationIcs.Config.DT_PROXIMA_RODADA}");
				return false;
			}

			return true;
		}


	}
}
