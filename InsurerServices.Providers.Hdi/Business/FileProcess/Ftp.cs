using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Entities;
using Renci.SshNet;

namespace InsurerServices.Providers.Hdi.Business
{
	internal class Ftp
	{
		private readonly FtpConfigurationModel _ftpConfigurationModel;

		public Ftp(FtpConfigurationModel ftpConfigurationModel)
		{
			_ftpConfigurationModel = ftpConfigurationModel;
		}

		public void UploadFile(FileInfo fileInfo)
		{
			if (!_ftpConfigurationModel.IsSftp)
			{
				var request = ConfigureFtp(WebRequestMethods.Ftp.UploadFile, fileInfo.Name);

				var fileContent = System.IO.File.ReadAllBytes(fileInfo.FullName);
				request.ContentLength = fileContent.Length;

				using (Stream requestStream = request.GetRequestStream())
					requestStream.Write(fileContent, 0, fileContent.Length);

				using (var response = (FtpWebResponse) request.GetResponse())
					Console.WriteLine($"Upload File : {fileInfo.FullName} STATUS:[{response.StatusDescription}]");
			}
			else
			{
				using (var sftpClient = ConfigureSftp())
				{
					sftpClient.Connect();
					var uploadPath =$"{_ftpConfigurationModel.UploadFileFolder}{fileInfo.Name}";
					var file = System.IO.File.OpenRead(fileInfo.FullName);
					sftpClient.UploadFile(file,uploadPath);
					sftpClient.Disconnect();
					file.Close();
					Console.WriteLine($"Uploaded File : {fileInfo.FullName}");
				}
			}
		}
		public void DownloadFile(string fileName)
		{
			var destinationPath = $"{_ftpConfigurationModel.DownloadFilesFolder}{fileName}";
			if (!_ftpConfigurationModel.IsSftp)
			{
				Files.CreateDirectory(_ftpConfigurationModel.DownloadFilesFolder);
				var request = ConfigureFtp(WebRequestMethods.Ftp.DownloadFile, fileName);

				using (var response = (FtpWebResponse) request.GetResponse())
				using (var responseStream = response.GetResponseStream())
				using (var writer = new FileStream(destinationPath, FileMode.Create))
				{
					var length = response.ContentLength;
					const int bufferSize = 2048;
					var buffer = new byte[2048];
					if (responseStream != null)
					{
						var readCount = responseStream.Read(buffer, 0, bufferSize);
						while (readCount > 0)
						{
							writer.Write(buffer, 0, readCount);
							readCount = responseStream.Read(buffer, 0, bufferSize);
						}
					}
				}


				Console.WriteLine(
					$"Download File {fileName} Complete to Path:[{destinationPath.Replace(".zip", ".txt")}]");
			}
			else
			{
				
				using (var sftpClient = ConfigureSftp())
				{
					sftpClient.Connect();
					Files.CreateDirectory($"{_ftpConfigurationModel.DownloadFilesFolder}");
					var fileStream = new FileStream(destinationPath,FileMode.Create);
					
					sftpClient.DownloadFile($"{_ftpConfigurationModel.RootFolder}/{fileName}", fileStream);
					sftpClient.Disconnect();
					fileStream.Close();
					Console.WriteLine(
						$"Download File {fileName} Complete to Path:[{destinationPath.Replace(".zip", ".txt")}]");
				}
			}

		}


		

		public List<string> ListFilesNameFromServer()
		{
			var filesName = new List<string>();
			if (!_ftpConfigurationModel.IsSftp)
			{
				try
				{
					FtpWebRequest request = ConfigureFtp(WebRequestMethods.Ftp.ListDirectoryDetails);

					using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
					{
						using (StreamReader reader =
							new StreamReader(response.GetResponseStream() ?? throw new Exception($"ftp_response_nulo")))
						{
							while (!reader.EndOfStream)
							{
								var fileInfo = reader.ReadLine();
								if (fileInfo != null && (!fileInfo.isNullOrEmpty() && fileInfo.ToUpperInvariant()
															 .Contains(_ftpConfigurationModel.LookUpFileName
																 .ToUpperInvariant())))
									if (FileDateIntervalIsValid(fileInfo))
										filesName.Add(fileInfo.Substring(fileInfo.ToUpperInvariant()
											.IndexOf(_ftpConfigurationModel.LookUpFileName.ToUpperInvariant(),
												StringComparison.Ordinal)).Trim());

							}
						}
					}
				}
				catch (Exception ex)
				{
					return filesName;
				}
			}
			else
			{
				using (var sftpClient = ConfigureSftp())
				{
					try
					{
						sftpClient.Connect();
						var sftpFiles = sftpClient.ListDirectory($"{_ftpConfigurationModel.RootFolder}");
						foreach (var sftpFile in sftpFiles)
							if (sftpFile != null && (!sftpFile.Name.isNullOrEmpty() && sftpFile.Name.ToUpperInvariant()
								                         .Contains(_ftpConfigurationModel.LookUpFileName
									                         .ToUpperInvariant())))
								if (FileDateIntervalIsValid(sftpFile.LastWriteTime))
									filesName.Add(sftpFile.Name);
						sftpClient.Disconnect();
					}
					catch (Exception ex)
					{

						return filesName;
					}
				}
			}
			return filesName;
		}

		private bool FileDateIntervalIsValid(string fileInfo)
		{
			var calendar = new GregorianCalendar();
			var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			calendar.TwoDigitYearMax = DateTime.Now.Year;
			if (currentCulture.Clone() is CultureInfo cultureToApply)
			{
				cultureToApply.DateTimeFormat.Calendar = calendar;
				var fileCreationDate =
					DateTime.ParseExact(fileInfo.Substring(0, 10).Trim(), "MM-dd-yy", cultureToApply);
				var minimunValidDate =
					DateTime.Now.AddDays(Math.Abs(_ftpConfigurationModel.LookUpCreationFileDaysInterval) * (-1));
				if (fileCreationDate >= minimunValidDate)
					return true;
			}

			return false;
		}
		private bool FileDateIntervalIsValid(DateTime fileInfo)
		{
				var minimunValidDate =
					DateTime.Now.AddDays(Math.Abs(_ftpConfigurationModel.LookUpCreationFileDaysInterval) * (-1));
				if (fileInfo >= minimunValidDate)
					return true;

			return false;
		}
		private FtpWebRequest ConfigureFtp(string method, string fileName = "")
		{
			FtpWebRequest request = null;
			switch (method)
			{
				case WebRequestMethods.Ftp.DownloadFile:
					if (fileName.isNullOrEmpty())
						throw new Exception("file_name_nulo: Para realizar o download é necessário passar um fileName.");

					request = (FtpWebRequest)WebRequest.Create($"{_ftpConfigurationModel.Host}{_ftpConfigurationModel.RootFolder}{fileName}");
					break;

				case WebRequestMethods.Ftp.ListDirectoryDetails:
					request = (FtpWebRequest)WebRequest.Create($"{_ftpConfigurationModel.Host}{_ftpConfigurationModel.RootFolder}");
					break;

				case WebRequestMethods.Ftp.UploadFile:
					request = (FtpWebRequest)WebRequest.Create($"{_ftpConfigurationModel.Host}{_ftpConfigurationModel.UploadFileFolder}{fileName}");
					break;
				default:
					throw new Exception("method_nao_configurado");
			}

			request.Method = method;
			request.Credentials =
				new NetworkCredential(_ftpConfigurationModel.Username, _ftpConfigurationModel.Password);

			request.UsePassive = true;
			request.UseBinary = true;
			request.KeepAlive = false;
			if (_ftpConfigurationModel.Timeout != null)
				request.Timeout = (int) _ftpConfigurationModel.Timeout;
			return request;
		}

		private SftpClient ConfigureSftp()
		{
			if(!_ftpConfigurationModel.Host.ToUpperInvariant().Contains("SFTP"))
				throw new Exception($"Para utilizar o sftp é necessário definir HOST válido no appsettings");

			if (_ftpConfigurationModel.Port==null)
				throw new Exception($"Para utilizar o sftp é necessário definir uma Porta válida");


			if (_ftpConfigurationModel.RsaFileName.isNullOrEmpty())
				throw  new Exception($"Para utilizar o sftp é necessário definir um valor para a configuração RsaFileName, no appsettings");

			var privateKeyFileConfiguration = new PrivateKeyFile($"ConfigurationFiles\\HDI\\{_ftpConfigurationModel.RsaFileName}");
			var connectionInfo = new PrivateKeyConnectionInfo(_ftpConfigurationModel.Host,
															  _ftpConfigurationModel.Port.Value,
															  _ftpConfigurationModel.Username,
															 //  ProxyTypes.Http,
															 // "px1.ituran.sp",
															 // 8080,
															  privateKeyFileConfiguration);
			if (_ftpConfigurationModel.Timeout != null)
				connectionInfo.Timeout = new TimeSpan(0,0,0,0,_ftpConfigurationModel.Timeout.Value);





			return  new SftpClient(connectionInfo);
		}


		internal static FtpConfigurationModel BuildHdiFtpConfigurationModel(EnumProcessType type, HdiProcessConfiguration hdiProcessConfiguration)
		{
			FtpConfiguration configuration;
			switch (type)
			{
				case EnumProcessType.Issuance:
					configuration = hdiProcessConfiguration.IssuanceFileFtp;
					break;
				case EnumProcessType.Cancelation:
					configuration = hdiProcessConfiguration.CancelationFileFtp;
					break;
				default:
					return null;
			}
					return new FtpConfigurationModel(type)
					{
						IsSftp = configuration.IsSftp,
						RsaFileName = configuration.RsaFileName,
						Host = configuration.Host,
						LookUpFileName = configuration.LookUpFileName,
						Port = configuration.Port,
						RootFolder = configuration.RootFolder,
						Username = configuration.Username,
						Password = configuration.Password,
						DownloadFilesFolder = $"{FileManager.GetDownloadFolder(type, hdiProcessConfiguration.DownloadFilesFolder)}",
						LookUpCreationFileDaysInterval = configuration.LookUpCreationFileDaysInterval,
						UploadFileFolder = configuration.UploadFileFolder,
						Timeout = configuration.Timeout
					};
		}

	}
}
