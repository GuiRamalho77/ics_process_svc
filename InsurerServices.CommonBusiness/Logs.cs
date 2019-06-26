using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InsurerServices.CommonBusiness.ConfigurationModels;
using InsurerServices.CommonBusiness.Entities;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers;
using InsurerServices.CommonBusiness.Helpers.Extensions;

namespace InsurerServices.CommonBusiness
{
	public class Logs
	{
		private List<LogModel> Log { get; set; }
		private readonly LogsConfiguration _configuration;
		internal readonly EnumProcessType ProcessType;
		public Logs(LogsConfiguration configuration, EnumProcessType processType)
		{
			Log = new List<LogModel>();
			_configuration = configuration;
			ProcessType = processType;
		}
		public string ReturnLogName() => $"{DateTime.Now.ToString("yyyyMM_ddHHmmss", CultureInfo.InvariantCulture)}.log";
		public List<EnumProcessType> ReturnLogFileTypes() => Log.DistinctBy(p => p.FileType).Select(p => p.FileType).ToList();
		public void Clear()
		{
			Log = new List<LogModel>();
		}

		public static void AddException(Exception exception, LogsConfiguration configuration, string aditionalInformation = "")
		{
			var type = exception.GetType().Name;

			Files.CreateDirectory(configuration.ExceptionsLogsFolder);
			var fileContent = new List<string>();
			if (!aditionalInformation.isNullOrEmpty())
				fileContent.Add($"Mensagem: {exception.Message}");
			fileContent.Add($"ExceptionMessage: {exception.Message}");
			fileContent.Add($"InnerException: {exception.InnerException}");
			fileContent.Add($"StackTrace: {exception.StackTrace}");

			File.WriteAllLines($"{configuration.ExceptionsLogsFolder}/{DateTime.Now.ToString("yyyyMM_ddHHmmss", CultureInfo.InvariantCulture)}{type}.log", fileContent);
		}
		public void Add(string message, EnumLog type = EnumLog.Information)
		{
			if (!message.isNullOrEmpty())
				Log.Add(new LogModel { Message = message, Step = $"PASSO {Log.Count.ToString()}", LogType = type, FileType = ProcessType });

		}

		public void Save(EnumInsurer insurer)
		{
			if(Log.Any())
				if ((Log.FirstOrDefault(p => p.LogType == EnumLog.Error) != null && !_configuration.DisableErrorLog) ||_configuration.EnableInformationalLog)
				{

					var fileContent = new List<string>();
					foreach (var fileType in ReturnLogFileTypes())
					{
						foreach (var log in Log)
							fileContent.Add($"[{DateTime.Now}] [{log.LogType.ToString()}] - {log.Step}: {log.Message}");

						string fileName = ReturnLogName();
						string logFolder = ReturnLogFolder(fileType);

						Files.CreateDirectory(logFolder);
						File.WriteAllLines($"{logFolder}/{fileName}", fileContent);
						Log = new List<LogModel>();
					}
				}
		}




		public string ReturnLogFolder(EnumProcessType fileType)
		{
			switch (fileType)
			{
				case EnumProcessType.Issuance:
					return $"{_configuration.IsuanceFileLogsFolder}";
				case EnumProcessType.Cancelation:
					return $"{_configuration.CancelationFileLogsFolder}";
				default:
					throw new Exception("Erro ao retornar pasta do log, tipo arquivo inexistente.");
			}
		}



	}
}
