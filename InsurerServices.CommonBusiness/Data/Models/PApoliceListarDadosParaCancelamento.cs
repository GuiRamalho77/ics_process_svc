using System;
using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.Data.Models
{
	internal class PApoliceListarDadosParaCancelamento
	{

		public int CD_APOLICE { get; set; }
		public string CD_IDENTIFICACAO { get; set; }
		public int NR_PLATAFORMA { get; set; }
		public string DS_APOLICE { get; set; }
		public string DS_PROPOSTA { get; set; }

		public string DS_CHASSI { get; set; }
		

		public string NR_CNPJ_CPF { get; set; }
		public EnumCustomerDocument? CD_TIPO_PESSOA { get; set; }


		public DateTime DATA_CANCELAMENTO { get; set; }
	    public bool FL_NUMERIC { get; set; }

	}
}
