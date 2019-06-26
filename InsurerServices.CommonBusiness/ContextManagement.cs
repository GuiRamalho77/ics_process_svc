using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Data;
using InsurerServices.CommonBusiness.Data.Models;
using InsurerServices.CommonBusiness.Entities;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using Ituran.Framework.Data.Entities;

namespace InsurerServices.CommonBusiness
{
	public class ContextManagement : BaseBusinessContext
	{

		public ContextManagement(Context context, Logs logs) : base(context, logs)
		{
		}

		public APOLICE_PROCESSAMENTO CreatePolicyProcess(EnumInsurer insurer, EnumIntegration integration)
		{

			try
			{
				var policyProcess = new APOLICE_PROCESSAMENTO();
				policyProcess.CD_PESSOA_SEGURADORA = insurer.ToInt();
				policyProcess.DS_INTEGRACAO = integration.GetDisplayName();
				policyProcess.NR_REGISTROS = 0;
				policyProcess.DT_CRIACAO = DateTime.Now;
				Context.PolicyProcess.Add(policyProcess);
				Context.SaveChanges();
				Logs.Add($"Registro na tabela APOLICE_PROCESSAMENTO criado. ID:{policyProcess.CD_APOLICE_PROCESSAMENTO}");
				return policyProcess;
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A aplicação gerou uma exceção não tratada ao tentar criar um registro na tabela APOLICE_PROCESSAMENTO.", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}
		}


		public APOLICE_PROCESSAMENTO_DADOS UpdatePolicyProcessData(string policyProposalIdentifier, int policyProcessIdentifier, EnumPolicyProcessDataStatus status, string errorCode = "", string errorDescription = "", decimal? endorsementValue = null)
		{
			var policyProcessData = Context.PolicyProcessData.FirstOrDefault(p =>
				(p.DS_PROPOSTA == policyProposalIdentifier || p.DS_APOLICE == policyProposalIdentifier) && p.CD_APOLICE_PROCESSAMENTO == policyProcessIdentifier &&
				p.NR_STATUS == EnumPolicyProcessDataStatus.PendingCancelationConfirmation.ToInt());

			if (policyProcessData == null)
				throw new Exception($"Não foi possível encontrar um registro na tabela APOLICE_PROCESSAMENTO_DADOS  correspondente a DS_PROPOSTA/DS_APOLICE: {policyProposalIdentifier}");

			policyProcessData.NR_STATUS = status.ToInt();
			if (!errorCode.isNullOrEmpty())
				policyProcessData.CD_ERRO = errorCode;
			if (!errorDescription.isNullOrEmpty())
				policyProcessData.DS_ERRO = errorDescription;
			if (endorsementValue != null)
				policyProcessData.VL_PREMIO = endorsementValue.Value;

			Context.PolicyProcessData.Update(policyProcessData);
			Context.SaveChanges();

			return policyProcessData;
		}

		public APOLICE_PROCESSAMENTO UpdatePolicyProcess(APOLICE_PROCESSAMENTO policyProcess, int? totalRegisters = null, string sourceData = "", string returnData = "")
		{

			try
			{
				var hasChange = false;
				if (totalRegisters != null)
				{
					policyProcess.NR_REGISTROS = totalRegisters.Value;
					hasChange = true;
				}

				if (!sourceData.isNullOrEmpty())
				{
					policyProcess.DS_FONTE_DADOS = sourceData;
					hasChange = true;
				}

				if (!returnData.isNullOrEmpty())
				{
					policyProcess.DS_RETORNO = returnData;
					hasChange = true;
				}


				if (hasChange)
				{
					Context.PolicyProcess.Update(policyProcess);
					Context.SaveChanges();
				}


				Logs.Add($"O registro na tabela APOLICE_PROCESSAMENTO foi atualizado. ID:{policyProcess.CD_APOLICE_PROCESSAMENTO}");
				return policyProcess;
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A aplicação gerou uma exceção não tratada ao realizar uma alteração em um registro na tabela APOLICE_PROCESSAMENTO.", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}


		}

		public APOLICE_PROCESSAMENTO_DADOS CreatePolicyProcessData(APOLICE_PROCESSAMENTO policyProcess, EnumProcessType processType, string dsProposal,
			string dsPolicy, string dsIdentifier, EnumPolicyProcessDataStatus policyProcessDataStatus = EnumPolicyProcessDataStatus.PendingProcess, string customerDocument = "", string vehicleChassi = "")
		{

			try
			{
				policyProcess.NR_REGISTROS++;
				var policyProcessData = new APOLICE_PROCESSAMENTO_DADOS
				{
					APOLICE_PROCESSAMENTO = policyProcess,
					DS_PROPOSTA = dsProposal,
					DS_APOLICE = dsPolicy,
					DS_CI = dsIdentifier,
					NR_TIPO_REGISTRO = processType.ToInt(),
					NR_STATUS = policyProcessDataStatus.ToInt(),
					NR_CNPJ_CPF = customerDocument,
					DS_CHASSI = vehicleChassi,
					DT_CRIACAO = DateTime.Now
				};
				Context.PolicyProcessData.Add(policyProcessData);
				Context.SaveChanges();

				Logs.Add($"Registro na tabela APOLICE_PROCESSAMENTO_DADOS criado. ID:{policyProcess.CD_APOLICE_PROCESSAMENTO}");
				Logs.Add($"Campo NR_REGISTROS incrementado na tabela APOLICE_PROCESSAMENTO. ID:{policyProcess.CD_APOLICE_PROCESSAMENTO}   NR_REGISTROS_ATUALIZADO:{policyProcess.NR_REGISTROS}");

				return policyProcessData;
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A aplicação gerou uma exceção não tratada ao tentar criar um registro na tabela APOLICE_PROCESSAMENTO_DADOS.", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}
		}

		public int? ReturnPolicyProcessAmount(int idPolicyProcess)
		{
			try
			{
				var policyProcessAmount = Context.PolicyProcessData.Count(p => p.CD_APOLICE_PROCESSAMENTO == idPolicyProcess);
				Logs.Add($"Foi retornado {policyProcessAmount} registros na tabela APOLICE_PROCESSAMENTO_DADOS relacionado ao ID:{idPolicyProcess} da tabela APOLICE_PROCESSAMENTO");
				return policyProcessAmount;
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A aplicação gerou uma exceção não tratada ao tentar retornar a quantidade de registros relacionado ao ID:{idPolicyProcess} da tabela APOLICE_PROCESSAMENTO", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}


		}
		public List<CancelationModel> GetCancelationPopulationData(EnumInsurer insurer)
		{
			try
			{
				var pApoliceListarDadosParaCancelamentos = Context.Procedure<PApoliceListarDadosParaCancelamento>(
					"P_Apolice_ListarDadosParaCancelamento", new { CD_PESSOA_SEGURADORA = insurer.ToInt() }).ToList();

				var population = new List<CancelationModel>();
				foreach (var cancelationData in pApoliceListarDadosParaCancelamentos)
					if (!cancelationData.DS_APOLICE.isNullOrEmpty())
						population.Add(cancelationData.ToCancelationModel());
					else
						Logs.Add($"A apólice {cancelationData.CD_APOLICE} foi listada pra cancelamento, porém não pode ser cancelada, pois não possui um número de apólice");

				return population;
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada ao tentar retornar a população elegivél para cancelamento.", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}

		}

		public List<CancelationModel> GetCancelationPopulationData(List<int> policyIdentifiers)
		{

			try
			{
				var policyIdentifiersCommaSeparated = string.Join(",", policyIdentifiers);
				var pApoliceListarDadosParaCancelamentos = Context.Query<PApoliceListarDadosParaCancelamento>($@"select    
																		distinct(ap.CD_APOLICE),
																		ap.DS_APOLICE,
																		ap.DS_PROPOSTA,
																		ap.CD_IDENTIFICACAO,
																		pl.DS_CHASSI,
																		pl.NR_PLATAFORMA,
																		ben.CD_TIPO_PESSOA,
																		ben.NR_CNPJ_CPF
																		from APOLICE ap
																		join pedido_item_apolice pia  on pia.cd_apolice = ap.cd_apolice
																		join pedido_item pi  on pi.cd_pedido_item = pia.cd_pedido_item
																		join pedido pd  on pd.cd_pedido = pi.cd_pedido
																		join PESSOA ben  on pd.CD_PESSOA_BENEFICIARIO = ben.CD_PESSOA
																		join plataforma pl  on pd.nr_plataforma = pl.nr_plataforma
																		where ap.CD_APOLICE in ( {policyIdentifiersCommaSeparated})").ToList();
				return ConvertToListOfCancelationModels(pApoliceListarDadosParaCancelamentos);
			}
			catch (Exception ex)
			{
				Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada ao tentar retornar a população elegivél para cancelamento.", EnumLog.Error);
				Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
				if (ex.InnerException != null)
					Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
				Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
				return null;
			}

		}

		private List<CancelationModel> ConvertToListOfCancelationModels(List<PApoliceListarDadosParaCancelamento> pApoliceListarDadosParaCancelamentos)
		{
			var population = new List<CancelationModel>();

			foreach (var cancelationData in pApoliceListarDadosParaCancelamentos)
				if (!cancelationData.DS_APOLICE.isNullOrEmpty())
					population.Add(cancelationData.ToCancelationModel());
				else
					Logs.Add($"A apólice {cancelationData.CD_APOLICE} foi listada pra cancelamento, porém não pode ser cancelada, pois não possui um número de apólice");

			return population;
		}

		public APOLICE_PROCESSAMENTO FileName(string fileName)
		{
			return Context.PolicyProcess.FirstOrDefault(p => p.DS_FONTE_DADOS.Contains(fileName));
		}



	}
}
