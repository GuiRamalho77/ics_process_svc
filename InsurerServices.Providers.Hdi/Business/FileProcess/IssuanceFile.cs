using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Contracts;
using InsurerServices.Providers.Hdi.Entities;

namespace InsurerServices.Providers.Hdi.Business.FileProcess
{
	internal class IssuanceFile : HdiFile<IssuanceFileConfiguration>
	{
		public IssuanceFile(IssuanceFileConfiguration configuration) : base(configuration, EnumProcessType.Issuance)
		{
		}


		public override FileModel ReadFile(FileInfo fileInfo)
		{
			var file = base.ReadFile(fileInfo.FullName);
			var fileModel = new FileModel();
			fileModel.Header = ReadHeader(file);
			fileModel.Body = ReadBody(file);
			fileModel.Footer = ReadFooterModel(file);
			fileModel.VehiclesInformation = ReadVehiclesInformation(file);



			return fileModel;
		}

		private Dictionary<string, LineFieldsModel> ReadVehiclesInformation(string[] file)
		{
			if (Configuration.BodyVehicleInformation == null || !Configuration.BodyVehicleInformation.Any())
				return null;

			var bodyVehiclesInformationLines = file.Select((text, index) => new { LineText = text, LineNumber = index + 1 })
				.Where(p =>
				{
					var initialPosition = Configuration.BodyVehicleInformation["TIPO_REGISTRO"].InitialPosition;
					var length = Configuration.BodyVehicleInformation["TIPO_REGISTRO"].Length;
					return length != null && (initialPosition != null &&
					                          p.LineText.Substrings(initialPosition.Value, length.Value) == Configuration.BusinessSettings.BodyVehicleInformationIdentification);
				}).ToList();

			if (!bodyVehiclesInformationLines.Any())
				throw new Exception($"Não foi encontrado nenhum registro(s) do tipo:[{Configuration.BusinessSettings.BodyVehicleInformationIdentification}]");

			var vehiclesInformation = new Dictionary<string, LineFieldsModel>();
			foreach (var vehiclesInformationLine in bodyVehiclesInformationLines)
			{
				var line = new LineFieldsModel() { LineNumber = vehiclesInformationLine.LineNumber };
				foreach (var key in Configuration.BodyVehicleInformation.Keys)
				{
					var fieldConfiguration = Configuration.BodyVehicleInformation[key];

					var fileField = new Field(ProcessType, fieldConfiguration)
					{
						Value = vehiclesInformationLine.LineText.Substrings(fieldConfiguration.InitialPosition.Value, fieldConfiguration.Length.Value)
					};
					line.Fields.Add(key, fileField);
				}
				if(!vehiclesInformation.ContainsKey(line.Fields["DS_PROPOSTA"].Value))
					vehiclesInformation.Add(line.Fields["DS_PROPOSTA"].Value,line);
			}

			if (!vehiclesInformation.Any())
				throw new Exception($"Não foi possível converter nenhuma linha de informações de veiculo para o modelo.");

			return vehiclesInformation;
		}
		protected override FileHeaderModel ReadHeader(string[] file)
		{
			if (Configuration.HeaderFields == null || !Configuration.HeaderFields.Any())
				return null;

			var headerLine = file.Select((text, index) => new { LineText = text, LineNumber = index + 1 })
				.Where(p =>
				{
					var initialPosition = Configuration.HeaderFields["TIPO_REGISTRO"].InitialPosition;
					var length = Configuration.HeaderFields["TIPO_REGISTRO"].Length;
					return length != null && (initialPosition != null &&
											  p.LineText.Substrings(initialPosition.Value, length.Value) == Configuration.BusinessSettings.HeaderIdentification);
				}).FirstOrDefault();

			if (headerLine == null || headerLine.LineText.isNullOrEmpty())
				throw new Exception($"header_invalido");

			var headerModel = new FileHeaderModel();
			headerModel.Content.LineNumber = 1;
			foreach (var key in Configuration.HeaderFields.Keys)
			{
				var fieldConfiguration = Configuration.HeaderFields[key];

				var fileField = new Field(ProcessType, fieldConfiguration)
				{
					Value = headerLine.LineText.Substrings(fieldConfiguration.InitialPosition, fieldConfiguration.Length),
				};

				headerModel.Content.Fields.Add(key, fileField);
			}

			if (!headerModel.Content.Fields.Any())
				throw new Exception($"header_invalido");

			return headerModel;

		}

		protected override FileBodyModel ReadBody(string[] file)
		{
			if (Configuration.BodyFields == null || !Configuration.BodyFields.Any())
				return null;

			var bodyLines = file.Select((text, index) => new { LineText = text, LineNumber = index + 1 })
				.Where(p =>
				{
					var initialPosition = Configuration.BodyFields["TIPO_REGISTRO"].InitialPosition;
					var length = Configuration.BodyFields["TIPO_REGISTRO"].Length;
					return length != null && (initialPosition != null && p.LineText.Substrings(initialPosition.Value, length.Value) == Configuration.BusinessSettings.BodyIdentification);
				})
				.ToList();

			if (!bodyLines.Any())
				throw new Exception($"Não foi encontrado nenhum registro(s) do tipo:[{Configuration.BusinessSettings.BodyIdentification}]");



			var bodyModel = new FileBodyModel();
			foreach (var bodyLine in bodyLines)
			{
				var line = new LineFieldsModel() { LineNumber = bodyLine.LineNumber };
				foreach (var key in Configuration.BodyFields.Keys)
				{
					var fieldConfiguration = Configuration.BodyFields[key];

					var fileField = new Field(ProcessType, fieldConfiguration)
					{
						Value = bodyLine.LineText.Substrings(fieldConfiguration.InitialPosition.Value, fieldConfiguration.Length.Value)

					};
					line.Fields.Add(key, fileField);
				}
				bodyModel.Content.Add(line);
			}

			if (!bodyModel.Content.Any() && bodyLines.Any())
				throw new Exception($"Não foi possível converter nenhuma linha do body para o modelo.");

			return bodyModel;
		}

		protected override FileFooterModel ReadFooterModel(string[] file)
		{
			if (Configuration.FooterFields == null || !Configuration.FooterFields.Any())
				return null;


			var footerLine = file.Select((text, index) => new { LineText = text, LineNumber = index + 1 })
				.Where(p =>
				{
					var initialPosition = Configuration.FooterFields["TIPO_REGISTRO"].InitialPosition;
					var length = Configuration.FooterFields["TIPO_REGISTRO"].Length;
					return length != null && (initialPosition != null &&
											  p.LineText.Substrings(initialPosition.Value, length.Value) == Configuration.BusinessSettings.FooterIdentification);
				}).FirstOrDefault();


			if (footerLine == null || footerLine.LineText.isNullOrEmpty())
				throw new Exception($"footer_invalido");

			return null;
		}


	}
}
