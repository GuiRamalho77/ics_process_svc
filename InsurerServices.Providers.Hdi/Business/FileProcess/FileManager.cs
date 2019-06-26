using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Contracts;
using InsurerServices.Providers.Hdi.Entities;

namespace InsurerServices.Providers.Hdi.Business
{
    internal class FileManager : BaseBusiness
    {

        public FileManager(HdiProcessConfiguration hdiSettings, Logs logs) : base(hdiSettings, logs)
        {
            CreateDefaultDirectories();
        }

        internal FileInfo CreateCancelationFile(CancelationFileModel fileModel)
        {
            string destinationFolder = HdiProcessConfigurations.SendCancelationFilesFolder;
            //var destinationFolder = HdiProcessConfigurations.NewCancelationFilesFolder;
            string filePath = $"{destinationFolder}\\{fileModel.FileName}";
            Files.CreateDirectory(destinationFolder);
            File.WriteAllBytes(filePath, fileModel.Content);

            return RemoveNullCharactersOnFile(filePath, fileModel);
        }

        internal List<FileInfo> GetApprovedCancelationFiles()
        {
            var filesName = Directory.GetFiles(HdiProcessConfigurations.SendCancelationFilesFolder);
            return filesName.Select(fileName => new FileInfo(fileName)).ToList();
        }

        internal void UploadFile(FileInfo fileInfo, EnumProcessType processType)
        {

            Logs.Add($"Inicio da transferencia do arquivo.");
            var ftp = new Ftp(Ftp.BuildHdiFtpConfigurationModel(processType, HdiProcessConfigurations));
            ftp.UploadFile(fileInfo);
            Logs.Add($"O Arquivo foi transferido para o servidor com sucesso.");
        }
        internal void DownloadFiles(EnumProcessType processType)
        {
            var ftp = new Ftp(Ftp.BuildHdiFtpConfigurationModel(processType, HdiProcessConfigurations));
            var filesName = ftp.ListFilesNameFromServer();
            if (filesName.Any())
            {
                foreach (var fileName in filesName)
                {
                    if (!FileExists(fileName))
                    {
                        try
                        {
                            ftp.DownloadFile(fileName);
                            var filePath = $"{GetDownloadFolder(processType, HdiProcessConfigurations.DownloadFilesFolder)}\\{ fileName}";

                            Logs.Add($"O Download do arquivo {fileName} foi concluido.");
                            Files.UnzipDeleteZipFile($"{filePath}");
                            if (fileName.Contains(".zip"))
                                Logs.Add($"O arquivo foi extraido para { filePath.Replace(".zip", ".txt")}");
                        }
                        catch (Exception ex)
                        {
                            Logs.Add($"Não foi possível realizar o download do arquivo:[{fileName}]", EnumLog.Error);
                            Logs.Add($"[Exception Message]:{ex.Message}", EnumLog.Error);
                            Logs.Add($"[Exception StackTrace]:{ex.StackTrace}", EnumLog.Error);
                        }
                    }
                    else
                    {
                        Logs.Add($"O download do arquivo:[{fileName.Replace(".zip", ".txt")}]  não foi realizado, pois o mesmo já existe");
                    }
                }
            }
            else
            {
                Logs.Add($"Não existe nenhum arquivo para ser baixado no servidor.", EnumLog.Information);
            }
        }
        internal FileInfo MoveFile(FileInfo file, EnumProcessType processType, bool error = false)
        {
            string fileFolder;
            switch (processType)
            {
                case EnumProcessType.Issuance:
                    fileFolder = ReturnIssuanceFolder(error);
                    break;
                case EnumProcessType.Cancelation:
                    fileFolder = ReturnCancelationFolder(file.Name, error);
                    break;
                default:
                    Logs.Add($"Não foi encontrado nenhuma configuração para o arquivo do tipo {processType.ToString()}", EnumLog.Error);
                    return null;
            }

            if (fileFolder.isNullOrEmpty())
                return null;

            Files.CreateDirectory(fileFolder);
            var destinationPath = $"{fileFolder}\\{file.Name}";

            System.IO.File.Move(file.FullName, destinationPath);
            Logs.Add($"Arquivo movido com sucesso. Caminho Antigo: [{file.FullName}]  - Caminho Destino:[{destinationPath}]", EnumLog.Error);
            return new FileInfo(destinationPath);
        }

        private string ReturnCancelationFolder(string fileName, bool error = false)
        {

            if (error)
                return $"{HdiProcessConfigurations.ProcessedFilesFolder}\\Cancelamentos\\_ERROS";

            if (fileName.ToUpperInvariant().Contains(HdiProcessConfigurations.CancelationFileFtp.LookUpFileName.ToUpperInvariant()))
            {
                var fileDateTime = DateTime.ParseExact(fileName.ToUpperInvariant().Replace(HdiProcessConfigurations.CancelationFileFtp.LookUpFileName.ToUpperInvariant(), "").Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

                return $"{HdiProcessConfigurations.ProcessedFilesFolder}\\Cancelamentos\\{fileDateTime.Year}\\{fileDateTime:dd-MM-yyyy}\\Retorno";

            }

            if (!fileName.ToUpperInvariant()
                .Contains(HdiProcessConfigurations.CancelationFileFtp.LookUpFileName.ToUpperInvariant()))
            {

                var fileDateTime = DateTime.ParseExact(fileName.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                return $"{HdiProcessConfigurations.ProcessedFilesFolder}\\Cancelamentos\\{fileDateTime.Year}\\{fileDateTime:dd-MM-yyyy}";
            }

            throw new Exception($"Não foi definido nenhuma condição que seja possível retornar para qual pasta o arquivo de cancelamento deve ser movido.");
        }
        private string ReturnIssuanceFolder(bool error = false)
        {
            return error
                ? $"{HdiProcessConfigurations.ProcessedFilesFolder}\\Emissoes\\_ERROS"
                : $"{HdiProcessConfigurations.ProcessedFilesFolder}\\Emissoes\\{DateTime.Now.Year}\\{DateTime.Now:dd-MM-yyyy}";
        }
        internal List<FileInfo> ListDownloadedFiles(EnumProcessType fileType)
        {
            var downloadFolder = GetDownloadFolder(fileType, HdiProcessConfigurations.DownloadFilesFolder);
            var filesName = Directory.GetFiles(downloadFolder);


            return filesName.Select(fileName => new FileInfo(fileName)).ToList();
        }
        private bool FileExists(string fileName)
        {
            return (Directory.GetFiles(HdiProcessConfigurations.DownloadFilesFolder, fileName, SearchOption.AllDirectories).Any() ||
                    (Directory.GetFiles(HdiProcessConfigurations.DownloadFilesFolder, fileName.Replace(".zip", ".txt"), SearchOption.AllDirectories).Any() ||
                     (Directory.GetFiles(HdiProcessConfigurations.ProcessedFilesFolder, fileName, SearchOption.AllDirectories).Any() ||
                      Directory.GetFiles(HdiProcessConfigurations.ProcessedFilesFolder, fileName.Replace(".zip", ".txt"), SearchOption.AllDirectories).Any() ||
                        Directory.GetFiles(HdiProcessConfigurations.NewCancelationFilesFolder, fileName, SearchOption.AllDirectories).Any())));
        }


        private void CreateDefaultDirectories()
        {
            Files.CreateDirectory(HdiProcessConfigurations.DownloadFilesFolder);
            Files.CreateDirectory(HdiProcessConfigurations.ProcessedFilesFolder);
            Files.CreateDirectory(HdiProcessConfigurations.NewCancelationFilesFolder);
            Files.CreateDirectory(HdiProcessConfigurations.SendCancelationFilesFolder);
        }



        internal static string GetDownloadFolder(EnumProcessType fileType, string downloadFilesFolder)
        {
            switch (fileType)
            {
                case EnumProcessType.Issuance:
                    return $"{downloadFilesFolder}\\Emissoes\\";
                case EnumProcessType.Cancelation:
                    return $"{downloadFilesFolder}\\Cancelamentos\\";
                default:
                    throw new Exception("file_type_nao_configurado");
            }
        }

        public FileInfo RemoveNullCharactersOnFile(string filePath, CancelationFileModel fileModel)
        {
            List<string> file = File.ReadAllLines(filePath).ToList();
            int nullCharactersFound = 0;

            for (int i = 0; i < file.Count; i++)
            {
                nullCharactersFound += file[i].Split('\0').Length - 1;

                //if (i == file.Count - 1)
                //    file[i] = $"{file[i].Replace("\0", " ")}";
                //else
                file[i] = $"{file[i].Replace("\0", " ")}{Environment.NewLine}";
            }

            Byte[] info = file.SelectMany(s => Encoding.Default.GetBytes(s)).ToArray();
            fileModel.Content = info;

            File.WriteAllBytes(filePath, fileModel.Content);

            FileInfo fileInfo = new FileInfo(filePath);
            Logs.Add($"Foram removidos {nullCharactersFound} caracteres nulos do arquivo: [{fileInfo.FullName}]");

            return fileInfo;
        }
    }
}
