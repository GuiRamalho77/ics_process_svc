using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Contracts;
using Ituran.Framework.Comum.HttpClient;
using Newtonsoft.Json;

namespace InsurerServices.Providers.Hdi.Business.FileProcess
{
	internal class IcsModuleIntegration:BaseBusiness
	{

		private readonly IcsModuleConfigurations _icsConfiguration;
		public IcsModuleIntegration(HdiProcessConfiguration hdiConfiguration, Logs logs) : base(hdiConfiguration, logs)
		{
			_icsConfiguration = hdiConfiguration.AppSettingsConfiguration.IcsModuleConfigurations;
		}




		public void SetPolicyPendingCancelation(int policyIdentifier)
		{
			var apiConfiguration = _icsConfiguration.Apis["UpdatePolicyPendingCancelation"];
			var restfulClient = new CommonBusiness.Integrations.RestfulClient<dynamic>(_icsConfiguration.BaseUrl, apiConfiguration.Suffix,timeOut: apiConfiguration.Timeout);
			var result = restfulClient.Save(new
			{
				CD_APOLICE = policyIdentifier,
				MOTIVO = $"Apolice enviada para cancelamento, aguardando retorno seguradora.",
				NM_SISTEMA_ALTERACAO = $"ICS_PROCESS_SVC", 
				NM_USUARIO_ALTERACAO=$"INTEGRACAO",
				validarRegras = false,
				NRSTATUS = 8
			});
			var retorno = JsonConvert.DeserializeObject<dynamic>(result.ToString());

			if(retorno.ReturnCode==0) 
				Logs.Add($"Ocorreu um erro ao alterar a apólice: {policyIdentifier}. O Modulo_ICS a mensagem : {retorno.Message} ");
		}
	}
}
