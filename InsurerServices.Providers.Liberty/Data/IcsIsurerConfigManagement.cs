using System.Linq;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Data;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Liberty.Enums;
using Ituran.Framework.Data.Entities;
using Newtonsoft.Json;

namespace InsurerServices.Providers.Liberty.Data
{
	internal class IcsIsurerConfigManagement:BaseBusinessContext
	{
		public IcsIsurerConfigManagement(Context context, Logs logs) : base(context, logs)
		{
		}


		public SEGURADORAICSCONFIG GetConfiguration(EnumIntegrationConfigLiberty integration)
		{
			Logs.Add("0.1");
			return Context.IcsIsurerConfig.FirstOrDefault(x => x.CD_PESSOA_SEGURADORA == (int)EnumInsurer.Liberty && x.NR_INTEGRACAO == (int)integration);
		}
		public void UpdateIcsIsurerConfig(SEGURADORAICSCONFIG seguradoraIcs, ServiceConfigurationIcs serviceConfigurationIcs)
		{
			seguradoraIcs.JS_REGRAS = JsonConvert.SerializeObject(new
			{
			    serviceConfigurationIcs.CD_PESSOA_SEGURADORA,
			    serviceConfigurationIcs.NR_INTEGRACAO,
			    serviceConfigurationIcs.VALOR_MAXIMO,
			    serviceConfigurationIcs.PAGINA,
			    serviceConfigurationIcs.DT_ULTIMA_EXEC_SUCESSO
            });

			seguradoraIcs.JS_CONFIG = JsonConvert.SerializeObject(new
			{
				Config = new
				{
				    serviceConfigurationIcs.Config.NR_INTERVALO,
					serviceConfigurationIcs.Config.QTD_DADOS,
					serviceConfigurationIcs.Config.NR_THREADS,
					serviceConfigurationIcs.Config.STATUS,
					serviceConfigurationIcs.Config.EXECUTA_PERIODO,
					serviceConfigurationIcs.Config.DT_PROXIMA_RODADA
                }
			});

			Context.IcsIsurerConfig.Update(seguradoraIcs);
			Context.SaveChanges();
		}
	}
}
