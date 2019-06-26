using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InsurerServices.CommonBusiness;
using InsurerServices.CommonBusiness.Entities;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Contracts;
using InsurerServices.Providers.Hdi.Entities;
using InsurerServices.Providers.Hdi.Enums;
using InsurerServices.Providers.Hdi.Helpers;

namespace InsurerServices.Providers.Hdi.Business.FileProcess
{
	internal class CancelationFile : HdiFile<CancelationFileConfiguration>
	{
		private readonly Logs _logs;
		public CancelationFile(CancelationFileConfiguration configuration, Logs logs) : base(configuration, EnumProcessType.Cancelation)
		{
			_logs = logs;
		}


		#region WriteFile
		public CancelationFileModel CreateContent(List<CancelationModel> population)
		{
			var fileContent = new List<string>();
			var fileSequenceNumber = Configuration.BusinessSettings.DefaultValues["NUMERO_SEQUENCIAL_ARQUIVO"];
			var fileName = $"{DateTime.Now:yyyyMMdd}_{fileSequenceNumber}.txt";

			var headerLine = CreateHeaderLine(fileName);
			if (headerLine.isNullOrEmpty())
				throw new Exception($"Header Inválido, linha em branco.");

			var bodyLines = CreateBodyLines(population);
			if (!bodyLines.Any())
				throw new Exception($"Body Inválido, não foi gerado nenhum conteúdo.");

			var footerLine = CreateFooterLine(bodyLines.Count());
			if (footerLine.isNullOrEmpty())
				throw new Exception($"Header Inválido, linha em branco.");
			
			fileContent.Add(headerLine);
			fileContent.AddRange(bodyLines);
			fileContent.Add(footerLine);

			return new CancelationFileModel()
			{
				Content = ConvertToBytes(fileContent),
				FileName = fileName
			};
		}


		private string CreateHeaderLine(string fileName)
		{
			var headerModel = new FileHeaderModel { Content = { LineNumber = 1 } };
			foreach (var key in Configuration.BusinessSettings.DefaultValues.Keys)
				if (Configuration.HeaderFields.ContainsKey(key))
				{
					var fieldConfiguration = Configuration.HeaderFields[key];
					var fileField = new Field(ProcessType, fieldConfiguration)
					{
						Value = Configuration.BusinessSettings.DefaultValues[key]
					};
					headerModel.Content.Fields.Add(key, fileField);
				}


			var creationDate = DateTime.Now;
			CreateField(headerModel.Content, EnumFileFieldsType.Header, "TIPO_REGISTRO", Configuration.BusinessSettings.HeaderIdentification);
			CreateField(headerModel.Content, EnumFileFieldsType.Header, "DATA_GERACAO", $"{creationDate:ddMMyyyy}");
			CreateField(headerModel.Content, EnumFileFieldsType.Header, "HORA_GERACAO", $"{creationDate:HHmmss}");
			CreateField(headerModel.Content, EnumFileFieldsType.Header, "NOME_ARQUIVO_REFERENCIA_ITURAN", $"{fileName}");

			var header = new StringBuilder();
			foreach (var headerField in headerModel.Content.PrepareSequenceFileFields())
				header.Append(headerField.Value);

			return header.Append(Environment.NewLine).ToString();

		}

		private List<string> CreateBodyLines(List<CancelationModel> population)
		{
			var bodyModel = new FileBodyModel();
			var lineNumber = 1;

			foreach (var data in population)
			{
				if (data.PolicyInsurerIdentifier.isNullOrEmpty())
				{
					_logs.Add($"A apólice {data.PolicyIdentifier} foi listada pra cancelamento, porém não pode ser cancelada, pois não possui um número de apólice");
					continue;
				}

				var line = new LineFieldsModel() { LineNumber = lineNumber++ };
				foreach (var key in Configuration.BusinessSettings.DefaultValues.Keys)
					if (Configuration.BodyFields.ContainsKey(key))
					{
						var fieldConfiguration = Configuration.BodyFields[key];
						var fileField = new Field(ProcessType, fieldConfiguration)
						{
							Value = Configuration.BusinessSettings.DefaultValues[key]
						};

						line.Fields.Add(key, fileField);
					}


				CreateField(line, EnumFileFieldsType.Body, "TIPO_REGISTRO", Configuration.BusinessSettings.BodyIdentification);
				CreateField(line, EnumFileFieldsType.Body, "REGISTRO_SEQUENCIAL", line.LineNumber.ToString());
				CreateField(line, EnumFileFieldsType.Body, "DOCUMENTO", data.PolicyInsurerIdentifier.RemoveHdiDocumentFormat());
				CreateField(line, EnumFileFieldsType.Body, "ENDOSSO");
				CreateField(line, EnumFileFieldsType.Body, "NUMERO_CONTROLE_ITURAN", data.PolicyIdentifier.ToString());
				CreateField(line, EnumFileFieldsType.Body, "CODIGO_EQUIPAMENTO");
				CreateField(line, EnumFileFieldsType.Body, "DATA_CANCELAMENTO", $"{data.CancelationDate:yyyyMMdd}");
				CreateField(line, EnumFileFieldsType.Body, "DATA_PROCESSAMENTO");
				CreateField(line, EnumFileFieldsType.Body, "STATUS_CANCELAMENTO");
				CreateField(line, EnumFileFieldsType.Body, "CODIGO_OBSERVACAO");
				CreateField(line, EnumFileFieldsType.Body, "OBSERVACAO");
				CreateField(line, EnumFileFieldsType.Body, "VALOR_PREMIO");

				if(!ValidateCustomerDocument(data))
					continue;

				CreateField(line, EnumFileFieldsType.Body, "TIPO_DOCUMENTO",data.DocumentType == null
						? BusinessExtensions.GetDocumentType(data.CustomerDocument)?.ToInt().ToString()
						: data.DocumentType?.ToInt().ToString());

				CreateField(line, EnumFileFieldsType.Body, "NR_CNPJ_CPF",data.CustomerDocument.RemoveDocumentSpecialCharacters());

				bodyModel.Content.Add(line);
			}

			var bodyContent = new List<string>();
			foreach (var line in bodyModel.Content)
			{
				var body = new StringBuilder();
				foreach (var bodyField in line.PrepareSequenceFileFields())
					body.Append(bodyField.Value);

				bodyContent.Add(body.Append(Environment.NewLine).ToString());
			}

			return bodyContent;
		}


	
		private string CreateFooterLine(int totalRegisters)
		{
			var footerModel = new FileFooterModel { Content = { LineNumber = totalRegisters + 2 } };
			foreach (var key in Configuration.BusinessSettings.DefaultValues.Keys)
				if (Configuration.FooterFields.ContainsKey(key))
				{
					var fieldConfiguration = Configuration.FooterFields[key];
					var fileField = new Field(ProcessType, fieldConfiguration)
					{
						Value = Configuration.BusinessSettings.DefaultValues[key]
					};
					footerModel.Content.Fields.Add(key, fileField);
				}
			CreateField(footerModel.Content, EnumFileFieldsType.Footer, "TIPO_REGISTRO", Configuration.BusinessSettings.FooterIdentification);
			CreateField(footerModel.Content, EnumFileFieldsType.Footer, "TOTAL_REGISTROS", $"{totalRegisters}");

			var footer = new StringBuilder();
			foreach (var footerField in footerModel.Content.PrepareSequenceFileFields())
				footer.Append(footerField.Value);

			return footer.Append(Environment.NewLine).ToString();
		}



		private void CreateField(LineFieldsModel content, EnumFileFieldsType type, string name, string value="")
		{
			switch (type)
			{
				case EnumFileFieldsType.Header:
					content.Fields.Add(name, new Field(ProcessType, Configuration.HeaderFields[name]) { Value = value.isNullOrEmpty()?Configuration.HeaderFields[name].DefaultValue:value });
					return;
				case EnumFileFieldsType.Body:
					content.Fields.Add(name, new Field(ProcessType, Configuration.BodyFields[name]) { Value = value.isNullOrEmpty() ? Configuration.BodyFields[name].DefaultValue : value });
					return;
				case EnumFileFieldsType.Footer:
					content.Fields.Add(name, new Field(ProcessType, Configuration.FooterFields[name]) { Value = value.isNullOrEmpty() ? Configuration.FooterFields[name].DefaultValue : value });
					return;
				default:
					throw new Exception("Tipo do registro não contemplado.");
			}

		}
		private byte[] ConvertToBytes(List<string> content) =>
			content.SelectMany(s => Encoding.ASCII.GetBytes(s)).ToArray();
		#endregion



		#region ReadFile
		public override FileModel ReadFile(FileInfo fileInfo)
		{
			var file = base.ReadFile(fileInfo.FullName);
			var fileModel = new FileModel();
			fileModel.Header = ReadHeader(file);
			fileModel.Body = ReadBody(file);
			fileModel.Footer = ReadFooterModel(file);
			return fileModel;
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
				throw new Exception($"body_sem_conteudo");



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

			if (!bodyModel.Content.Any())
				throw new Exception($"body_invalido");

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


		#endregion




		private bool ValidateCustomerDocument(CancelationModel cancelationModel)
		{
			if (cancelationModel.DocumentType == null &&
			    BusinessExtensions.GetDocumentType(cancelationModel.CustomerDocument) == null)
				return false;


			if (cancelationModel.CustomerDocument.RemoveDocumentSpecialCharacters().Length == 11 ||
			    cancelationModel.CustomerDocument.RemoveDocumentSpecialCharacters().Length == 14)
				return true;

			_logs.Add($"O cliente vinculado à apólice {cancelationModel.PolicyIdentifier} não possui um documento válido e foi ignorado no arquivo." +
			          $" NR_CNPJ_CPF [{cancelationModel.CustomerDocument}] - CD_TIPO_PESSO [{cancelationModel.DocumentType}]");
			return false;
		}

	}
}
