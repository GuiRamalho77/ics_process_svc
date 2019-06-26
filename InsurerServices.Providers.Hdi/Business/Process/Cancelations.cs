using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Contracts;
using InsurerServices.CommonBusiness.Entities;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.Business.FileProcess;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Helpers;

namespace InsurerServices.Providers.Hdi.Business.Process
{
    internal class Cancelations : BaseProcess
    {
        private readonly HdiProcessConfiguration _hdiProcessConfiguration;
        private readonly IcsModuleIntegration _icsModuleIntegration;
        private readonly ValidationRules _validationRules;
        private readonly CancelationFile _cancelationFile;
        private readonly FileManager _fileManager;
        public Cancelations(
            CancelationFileConfiguration cancelationFileConfiguration,
            HdiProcessConfiguration hdiConfiguration,
            IcsModuleIntegration icsModuleIntegration,
            ContextManagement contextManagement,
            ValidationRules validationRules,
            FileManager fileManager,
            Logs logs)
            : base(hdiConfiguration.AppSettingsConfiguration, contextManagement, logs)
        {
            _hdiProcessConfiguration = hdiConfiguration;
            _cancelationFile = new CancelationFile(cancelationFileConfiguration, logs);
            _icsModuleIntegration = icsModuleIntegration;
            _validationRules = validationRules;
            _fileManager = fileManager;
        }

        public void ProcessFiles()
        {
            try
            {       
				if(_hdiProcessConfiguration.CreateCancelationFiles)
					CreateCancelationFiles();

                if (_hdiProcessConfiguration.SendCancelationsFiles)
                    SendCancelationFiles();

                if (_hdiProcessConfiguration.ProcessCancelationFilesReturn)
                    ProcessCancelationsReturn();
            }
            catch (Exception ex)
            {
                Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada.", EnumLog.Error);
                Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
                if (ex.InnerException != null)
                    Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
                Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);

            }
            finally
            {
                Logs.Save(EnumInsurer.Hdi);
            }
        }

        private void CreateCancelationFiles()
	    {
            FileInfo fileInfo = null;
            try
            {
                Logs.Add($"Inicio do processo de geração de arquivo para cancelamento de apólices");
                //var population = ContextManagement.GetCancelationPopulationData(EnumInsurer.Hdi).OrderByDescending(x=>x.PolicyIdentifier).Take(800).ToList();
                var population = ContextManagement.GetCancelationPopulationData(EnumInsurer.Hdi);

                if (population == null || !population.Any())
                {
                    Logs.Add($"Não foi retornado nenhum dado para cancelamento.", EnumLog.Error);
                    return;
                }
                else
                    Logs.Add($"Foram encontrados {population.Count} registros de cancelamento.");

                //ChangeCustomerDocumentsForTest(population);

                var cancelationFileModel = _cancelationFile.CreateContent(population);
                Logs.Add($"Conteudo do arquivo criado.");

                fileInfo = _fileManager.CreateCancelationFile(cancelationFileModel);
                Logs.Add($"O conteúdo do arquivo foi salvo no arquivo: [{fileInfo.FullName}]");
            }
            catch (Exception ex)
            {
                Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada.", EnumLog.Error);
                Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
                if (ex.InnerException != null)
                    Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
                Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);

                if (fileInfo != null)
                    _fileManager.MoveFile(fileInfo, ProcessType, true);
            }
            finally
            {
                Logs.Add($"Termino do processamento.");
                Logs.Save(EnumInsurer.Hdi);
            }
        }

        private void SendCancelationFiles()
        {
            Logs.Add($"Inicio do processo de envio do arquivo para cancelamento de apólices");
            var cancelationFiles = _fileManager.GetApprovedCancelationFiles();

            if (cancelationFiles.Any())
	        {
		        foreach (var cancelationFile in cancelationFiles)
		        {
			        var fileInfo = cancelationFile;
					try
					{
						Logs.Add($"Inicio do processo de envio do arquivo [{cancelationFile.FullName}] para cancelamento de apólices");
						Logs.Add($"Retornar dados do arquivo escrito para inserção das informações no banco de dados.");
						var cancelationFileContent = _cancelationFile.ReadFile(cancelationFile);
						var policyIdentifiers = cancelationFileContent.Body.Content.Select(p => p.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt()).ToList();
						var population = ContextManagement.GetCancelationPopulationData(policyIdentifiers);
						var policyProcess = ContextManagement.CreatePolicyProcess(EnumInsurer.Hdi, EnumIntegration.TxtFile);

						foreach (var cancelation in cancelationFileContent.Body.Content)
						{
							try
							{
								var policyIdentifier = cancelation.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt();
								var cancelationModel = population.FirstOrDefault(p =>
									p.PolicyIdentifier == policyIdentifier);

								if (cancelationModel != null)
								{
									var policeProcessData = ContextManagement.CreatePolicyProcessData(
									        policyProcess,
									        ProcessType,
											cancelationModel.PolicyProposalIdentifier,
									        cancelationModel.PolicyInsurerIdentifier,
                                            //cancelation.Fields["DOCUMENTO"].Value,
                                            cancelationModel.IdentificationCode,
											EnumPolicyProcessDataStatus.PendingCancelationConfirmation,
									        cancelation.Fields["NR_CNPJ_CPF"].Value.RemoveDocumentSpecialCharacters(), 
									        cancelationModel.CarChassis);
									if (policeProcessData != null)
										_icsModuleIntegration.SetPolicyPendingCancelation(policyIdentifier);
								}
								else
									Logs.Add($"Não foi encontrado uma apólice na população, retornada do banco de dados, referente ao CD_APOLICE:{policyIdentifier} escrito no arquivo");
							}
							catch (Exception ex)
							{
								Logs.Add($"Ocorreu um erro ao processar a linha: {cancelation.LineNumber} " +
										 $"DADOS:[DS_PROPOSTA = {cancelation.Fields["DS_PROPOSTA"].Value}]" +
										 $"[DS_APOLICE = {cancelation.Fields["DS_APOLICE"].Value}]" +
										 $"[DS_CI = {cancelation.Fields["DS_CI"].Value}]");
								Logs.Add($"- [ExceptionMessage] - {ex.Message}");
								if (ex.InnerException != null)
									Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
								Logs.Add($"- [StackTrace] - {ex.StackTrace}");
							}
						}
						_fileManager.UploadFile(cancelationFile, ProcessType);
						fileInfo = _fileManager.MoveFile(cancelationFile, ProcessType);
						ContextManagement.UpdatePolicyProcess(policyProcess, sourceData: fileInfo.FullName);
					}
					catch (Exception ex)
					{
						Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada.", EnumLog.Error);
						Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
						if (ex.InnerException != null)
							Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
						Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);

						if (fileInfo != null)
							_fileManager.MoveFile(fileInfo, ProcessType, true);
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
		        Logs.Add($"Não existe nenhum arquivo na pasta [{_hdiProcessConfiguration.SendCancelationFilesFolder}]  para ser enviado.");
				Logs.Add($"Termino do processamento.");
		        Logs.Save(EnumInsurer.Hdi);
			}
           
        }

        private void ProcessCancelationsReturn()
        {

            Logs.Add($"Inicio do processamento de arquivos de retorno de cancelamento de apólices");
            _fileManager.DownloadFiles(ProcessType);
            var files = _fileManager.ListDownloadedFiles(ProcessType);

            if (files.Any())
            {
                foreach (var file in files)
                {
                    var policyProcess = ContextManagement.FileName(file.Name.Replace("RET_",string.Empty));
#if DEBUG
					//Usado para criar dados no modo debug, caso não exista esses registros no banco.
					//if (policyProcess == null)
					//{
					//	CreateTestCancelationProcessData(file);
					//	policyProcess = ContextManagement.FileName(file.FullName);
					//}
#endif


                    Logs.Add($"Inicio do processamento do arquivo: [{file.FullName}]");

                    try
                    {
                        var cancelationFileContent = _cancelationFile.ReadFile(file);
                        var policyIdentifiers = cancelationFileContent.Body.Content.Select(p => p.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt()).ToList();
                        var population = ContextManagement.GetCancelationPopulationData(policyIdentifiers);
                        if (population == null || !population.Any())
                        {
                            Logs.Add($"Não foi retornado nenhum dado para cancelamento.", EnumLog.Error);
                            return;
                        }
                        else
                        {
                            Logs.Add($"Foram encontrados {population.Count} registros de cancelamento.");
                        }

                        foreach (var cancelation in cancelationFileContent.Body.Content)
                        {
                            try
                            {
                                var policyIdentifier = cancelation.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt();
                                var cancelationModel = population.FirstOrDefault(p =>
                                    p.PolicyIdentifier == policyIdentifier);

                                if (cancelationModel != null)
                                {
                                    if (cancelation.Fields["STATUS_CANCELAMENTO"].Value == "1")
                                        ContextManagement.UpdatePolicyProcessData(
                                            cancelationModel.PolicyProposalIdentifier,
                                            policyProcess.CD_APOLICE_PROCESSAMENTO,
                                            EnumPolicyProcessDataStatus.PendingProcess);

                                    if (cancelation.Fields["STATUS_CANCELAMENTO"].Value == "2")
                                        ContextManagement.UpdatePolicyProcessData(
                                                cancelationModel.PolicyProposalIdentifier,
                                                policyProcess.CD_APOLICE_PROCESSAMENTO,
                                                EnumPolicyProcessDataStatus.ErrorCancelationResponse,
                                                errorCode: cancelation.Fields["CODIGO_OBSERVACAO"].Value,
                                                errorDescription: cancelation.Fields["OBSERVACAO"].Value,
                                                endorsementValue: cancelation.Fields["VALOR_PREMIO"].Value.ToDecimal()
                                            );

                                }
                                else
                                    Logs.Add(
                                        $"Não foi encontrado uma apólice no banco de dados referente ao CD_APOLICE:{policyIdentifier} escrito no arquivo");

                            }
                            catch (Exception ex)
                            {
                                Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada ao tentar processar a linha {cancelation.LineNumber}", EnumLog.Error);
                                Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
                                if (ex.InnerException != null)
                                    Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
                                Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
                            }
                        }
                        var fileInfo = _fileManager.MoveFile(file, ProcessType);
                        ContextManagement.UpdatePolicyProcess(policyProcess, sourceData: fileInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        var fileInfo = _fileManager.MoveFile(file, ProcessType, true);
                        ContextManagement.UpdatePolicyProcess(policyProcess, sourceData: fileInfo.FullName);
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


        private void CreateTestCancelationProcessData(FileInfo fileInfo)
        {
            try
            {
                var cancelationFileContent = _cancelationFile.ReadFile(fileInfo);
                var policyIdentifiers = cancelationFileContent.Body.Content.Select(p => p.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt()).ToList();
                var population = ContextManagement.GetCancelationPopulationData(policyIdentifiers);

                if (population == null || !population.Any())
                    throw new Exception($"Não é possível gerar uma população de testes, pois não foi encontrado nenhum registro no banco de dados correspondente aos CD_APOLICES no arquivo");
                var policyProcess =
                    ContextManagement.CreatePolicyProcess(EnumInsurer.Hdi, EnumIntegration.TxtFile);
                foreach (var cancelation in cancelationFileContent.Body.Content)
                {
                    try
                    {
                        var policyIdentifier = cancelation.Fields["NUMERO_CONTROLE_ITURAN"].Value.ToInt();
                        var cancelationModel = population.FirstOrDefault(p => p.PolicyIdentifier == policyIdentifier);
                        if (cancelationModel != null)
                        {
                            ContextManagement
                                .CreatePolicyProcessData(policyProcess, ProcessType,
                                    cancelationModel.PolicyProposalIdentifier,
                                    cancelation.Fields["DOCUMENTO"].Value,
                                    cancelationModel.IdentificationCode,
                                    EnumPolicyProcessDataStatus.PendingCancelationConfirmation);
                            _icsModuleIntegration.SetPolicyPendingCancelation(policyIdentifier);
                        }
                        else
                            Logs.Add(
                                $"Não foi encontrado uma apólice na população, retornada do banco de dados, referente ao CD_APOLICE:{policyIdentifier} escrito no arquivo");
                    }
                    catch (Exception ex)
                    {
                        Logs.Add($"Ocorreu um erro ao processar a linha: {cancelation.LineNumber} " +
                                 $"DADOS:[DS_PROPOSTA = {cancelation.Fields["DS_PROPOSTA"].Value}]" +
                                 $"[DS_APOLICE = {cancelation.Fields["DS_APOLICE"].Value}]" +
                                 $"[DS_CI = {cancelation.Fields["DS_CI"].Value}]");
                        Logs.Add($"- [ExceptionMessage] - {ex.Message}");
                        if (ex.InnerException != null)
                            Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
                        Logs.Add($"- [StackTrace] - {ex.StackTrace}");
                    }
                }
                ContextManagement.UpdatePolicyProcess(policyProcess, sourceData: fileInfo.FullName);
            }
            catch (Exception ex)
            {
                Logs.Add($"[Exception] A Aplicação gerou uma exceção não tratada ao tentar simular a criação de registros de dados de cancelamento.", EnumLog.Error);
                Logs.Add($"- [ExceptionMessage] - {ex.Message}", EnumLog.Error);
                if (ex.InnerException != null)
                    Logs.Add($"- [InnerException] - {ex.InnerException.Message}", EnumLog.Error);
                Logs.Add($"- [StackTrace] - {ex.StackTrace}", EnumLog.Error);
            }
        }

	    private void ChangeCustomerDocumentsForTest(List<CancelationModel> population)
	    {
		    foreach (var cancelationModel in population
			    .Where(p => p.CustomerDocument.RemoveDocumentSpecialCharacters().Length == 11)
			    .Take(population.Count / 10).ToList())
		    {
				var newRandomCpf= Helper.GenerateValidCpf();
				Logs.Add($"Alteração de cpf da apolice :{cancelationModel.PolicyIdentifier}. NR_CNPJ_CPF_ANTIGO:{cancelationModel.CustomerDocument}   - NR_CNPJ_CPF_NOVO:{newRandomCpf}");
			    cancelationModel.CustomerDocument = newRandomCpf;
		    }
		    
	    }
    }
}
