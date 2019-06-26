using System;
using System.IO;
using System.Linq;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.Entities;

namespace InsurerServices.Providers.Hdi.Contracts
{
	internal abstract class HdiFile<TConfigurationFile>
		where TConfigurationFile : class
	{
		internal readonly TConfigurationFile Configuration;
		internal readonly EnumProcessType ProcessType;
		protected HdiFile(TConfigurationFile configuration, EnumProcessType processType)
		{
			Configuration = configuration;
			ProcessType = processType;
		}

		public abstract FileModel ReadFile(FileInfo fileInfo);

		protected abstract FileHeaderModel ReadHeader(string[] file);
		protected abstract FileBodyModel ReadBody(string[] file);
		protected abstract FileFooterModel ReadFooterModel(string[] file);

		protected virtual string[] ReadFile(string filePath)
		{
			var bytes = System.IO.File.ReadAllBytes(filePath);
			if (bytes == null)
				throw new Exception($"Não foi possível ler os bytes do arquivo [ {filePath} ]!");

			var content = Files.ConvertFromByteToStringArray(bytes);
			if (content == null || !content.ToList().Any())
				throw new Exception($"Não foi possível converter os bytes do arquivo para string [ {filePath} ]!");

			if(content[content.Length-1].isNullOrEmpty())
				return content.Take(content.Length-1).ToArray();
			return content;

		}


	}

}

