using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Contracts;
using InsurerServices.Providers.Hdi.Entities;

namespace InsurerServices.Providers.Hdi.Business.FileProcess
{
	internal class ValidationRules:BaseBusiness
	{
		public ValidationRules(Logs logs, HdiProcessConfiguration hdiConfiguration) : base(hdiConfiguration, logs)
		{
		}
		internal bool ValidateIssuanceRules(LineFieldsModel issuance)
		{
			var dsProposal = issuance.Fields["DS_PROPOSTA"];
			if (dsProposal.Value.isNullOrEmpty())
			{
				Logs.Add($"A Linha:{issuance.LineNumber} não atende as regras para processamento como Emissão." +
				         $"O campo da posição inicial:{dsProposal.InitialPosition} até a Posição: {dsProposal.InitialPosition + dsProposal.Length} não pode estar vazio!");
				return false;
			}


			if (!ValidateProposalLength(dsProposal.Value))
			{
				Logs.Add($"A Linha:{issuance.LineNumber} não atende as regras para processamento como Emissão." +
				         $"O campo da posição inicial:{dsProposal.InitialPosition} " +
				         $"até a Posição: {dsProposal.InitialPosition + dsProposal.Length} tem que conter no mínimo {HdiProcessConfigurations.ProposalMinimunValidLength} caracteres!");
				return false;
			}

			return true;
		}
		private bool ValidateProposalLength(string proposal) => proposal.Trim().Length > HdiProcessConfigurations.ProposalMinimunValidLength;
	}
}
