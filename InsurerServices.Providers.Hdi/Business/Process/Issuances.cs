using System;
using System.Linq;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.Providers.Hdi.Business.FileProcess;
using InsurerServices.Providers.Hdi.ConfigurationModels;

namespace InsurerServices.Providers.Hdi.Business.Process
{
	internal class Issuances:BaseProcess
	{
		private readonly IcsModuleIntegration _icsModuleIntegration;
		private readonly ValidationRules _validationRules;
		private readonly IssuanceFile _issuanceFile;
		private readonly FileManager _fileManager;
		public Issuances(
			IssuanceFileConfiguration issuanceFileConfiguration,
			AppSettingsConfiguration appSettingsConfiguration,
			IcsModuleIntegration icsModuleIntegration,
			ContextManagement contextManagement,
			ValidationRules validationRules,
			FileManager fileManager,
			Logs logs)
			: base(appSettingsConfiguration, contextManagement, logs)
		{
			_issuanceFile = new IssuanceFile(issuanceFileConfiguration);
			_icsModuleIntegration = icsModuleIntegration;
			_validationRules = validationRules;
			_fileManager = fileManager;
		}




		public void ProcessFiles()
		{
			if(!AppSettings.ActiveIssuanceInsurers.Contains(EnumInsurer.Hdi))
				return;

			_fileManager.DownloadFiles(ProcessType);
			var files = _fileManager.ListDownloadedFiles(ProcessType);
			if (files.Any())
			{
				foreach (var file in files)
				{

					Logs.Add($"Inicio do processamento do arquivo: [{file.FullName}]");
					try
					{
						var issuanceFileContent = _issuanceFile.ReadFile(file);

						if (!issuanceFileContent.Body.Content.Any())
							continue;
						else
							Logs.Add(
								$"Foi encontrado {issuanceFileContent.Body.Content.Count} registro(s) do tipo:[{_issuanceFile.Configuration.BusinessSettings.BodyIdentification}]");


						if (!issuanceFileContent.VehiclesInformation.Any())
							continue;
						else
							Logs.Add(
								$"Foi encontrado {issuanceFileContent.VehiclesInformation.Count} registro(s) do tipo:[{_issuanceFile.Configuration.BusinessSettings.BodyVehicleInformationIdentification}]");


						var policyProcess =
							ContextManagement.CreatePolicyProcess(EnumInsurer.Hdi, EnumIntegration.TxtFile);

						foreach (var issuance in issuanceFileContent.Body.Content)
						{
							try
							{
								if (_validationRules.ValidateIssuanceRules(issuance))
								{
									var dsProposal = issuance.Fields["DS_PROPOSTA"].Value;
									if (!issuanceFileContent.VehiclesInformation.ContainsKey(dsProposal))
										throw new Exception($"Não existe uma informação para o veículo com a proposta: [ {dsProposal}]");

									var vehicleInformation = issuanceFileContent.VehiclesInformation[dsProposal];
									ContextManagement.CreatePolicyProcessData(
										policyProcess,
										ProcessType,
										dsProposal,
										issuance.Fields["DS_APOLICE"].Value,
										vehicleInformation.Fields["DS_CI"].Value,
										customerDocument: issuance.Fields["NR_CNPJ_CPF"].Value,
										vehicleChassi: vehicleInformation.Fields["DS_CHASSI"].Value
										);
								}

							}
							catch (Exception ex)
							{
								Logs.Add($"Ocorreu um erro ao processar a linha: {issuance.LineNumber} " +
										 $"DADOS:[DS_PROPOSTA = {issuance.Fields["DS_PROPOSTA"].Value}]" +
										 $"[DS_APOLICE = {issuance.Fields["DS_APOLICE"].Value}]" +
										 $"[DS_CI = {issuance.Fields["DS_CI"].Value}]");
								Logs.Add($"- [ExceptionMessage] - {ex.Message}");
								if (ex.InnerException != null)
									Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
								Logs.Add($"- [StackTrace] - {ex.StackTrace}");
							}
						}

						var processedFileInfo = _fileManager.MoveFile(file, ProcessType);
						ContextManagement.UpdatePolicyProcess(policyProcess,
							sourceData: processedFileInfo.FullName);

					}
					catch (Exception ex)
					{
						_fileManager.MoveFile(file, ProcessType, true);
						Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada.", EnumLog.Error);
						Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
						if (ex.InnerException != null)
							Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
						Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
					}
					finally
					{
						Logs.Add($"Termino do processamento.");
						Logs.Save(EnumInsurer.Hdi);
					}
				}
			}
			else
			{
				Logs.Add($"Não existe nenhum arquivo baixado para ser processado.");
				Logs.Save(EnumInsurer.Hdi);
			}
		}


	}
}
