using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Entities;

namespace InsurerServices.Providers.Hdi.Helpers
{
	internal static class BusinessExtensions
	{
		public static bool IsValid(this Dictionary<string, FileFieldConfiguration> fields)
		{
			foreach (var key in fields.Keys)
				if (!fields[key].IsValid())
					return false;

			return true;
		}
		public static Dictionary<int?, string> PrepareSequenceFileFields(this LineFieldsModel model)
		{
			Dictionary<int?, string> dic = new Dictionary<int?, string>();
			foreach (var field in model.Fields.Values)
				dic.Add(field.InitialPosition,field.Value);	
			
			return dic.OrderBy(key => key.Key).ToDictionary(keyItem => keyItem.Key, valueItem => valueItem.Value);
		}

		public static string ToString(this FileHeaderModel model)
		{
			var header = new StringBuilder();
			foreach (var headerField in model.Content.PrepareSequenceFileFields())
				header.Append(headerField.Value);

			return header.Append(Environment.NewLine).ToString();
		}
		public static EnumCustomerDocument? GetDocumentType(string document)
		{
			if (document.RemoveDocumentSpecialCharacters().Length == 11)
				return EnumCustomerDocument.Cpf;
			if (document.RemoveDocumentSpecialCharacters().Length == 14)
				return EnumCustomerDocument.Cnpj;

			return null;
		}


		public static string RemoveHdiDocumentFormat(this string document) => document.Replace("01.045.432.A.", "").Replace(".000000", "");

		public static string RemoveDocumentSpecialCharacters(this string document)=>document.isNullOrEmpty() 
			? string.Empty 
			: document.Replace("-", "").Replace(".", "").Replace("/", "");
	}
}
