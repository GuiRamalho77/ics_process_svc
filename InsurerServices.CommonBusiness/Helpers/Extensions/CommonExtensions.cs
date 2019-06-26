using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using InsurerServices.CommonBusiness.Data.Models;
using InsurerServices.CommonBusiness.Entities;
using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.Helpers.Extensions
{
	public static class CommonExtensions
	{
		public static int ToInt(this string text)
		{
			return int.Parse(text);
		}
		public static decimal ToDecimal(this string text)
		{
			var textWithoutSign = text.Contains("-")?text.Replace("-",""):text;
				
			var textAsDecimal = $"{textWithoutSign.Substrings(1, textWithoutSign.Length - 2)}.{textWithoutSign.Substrings(textWithoutSign.Length - 1, 2)}";

			return decimal.Parse(textAsDecimal.Trim());
		}

		public static bool isNullOrEmpty(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return true;


			return string.IsNullOrEmpty(text.Trim());
		}

		public static string Get(this string value, int? length)
		{
			if (length != null) return value.PadRight((int)length, ' ').Substring(0, (int)length).Trim();

			return string.Empty;
		}

		public static string Substrings(this string line, int? initialPosition, int? length)
		{
			var @return = string.Empty;
			if (initialPosition != null && length != null)
				if (line.Length>length.Value)
					@return = line.Substring((int)initialPosition - 1, (int)length);
				return @return;

		}

	    public static DateTime GetNextDateFromInterval(this DateTime initialDate, long intervalTicks)
	    {
	        var date = initialDate;
	        while (DateTime.Now > date)
	            date = date.AddTicks(intervalTicks);
	        return date;
	    }

	    public static string GetDescriptionEnum(this System.Enum value)
	    {
	        DescriptionAttribute[] da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
	        return da.Length > 0 ? da[0].Description : value.ToString();
	    }

        public static DateTime ToDateTime(this string date, string actualFormat)
		{
			return DateTime.ParseExact(date, actualFormat, CultureInfo.InvariantCulture);
		}
		public static string ToDateTimeString(this DateTime date, string actualFormat)
		{
			return date.ToString(actualFormat, CultureInfo.InvariantCulture);
		}
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (knownKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
		public static int ToInt(this EnumCustomerDocument document) => (int)document;
		public static int ToInt(this EnumInsurer insurer) => (int) insurer;
		public static int ToInt(this EnumProcessType processType) => (int)processType;
		internal static int ToInt(this EnumPolicyProcessDataStatus policyProcessDataStatus) => (int)policyProcessDataStatus;

		internal static string GetDisplayName(this EnumIntegration integration)
		{
			return integration.GetType().GetMember(integration.ToString()).First().GetCustomAttribute<DisplayAttribute>()
				.GetName();
		}
		internal static CancelationModel ToCancelationModel(this PApoliceListarDadosParaCancelamento cancelationData)
		{
			return new CancelationModel
			{
				PolicyIdentifier = cancelationData.CD_APOLICE,
				PolicyProposalIdentifier = cancelationData.DS_PROPOSTA,
				PolicyInsurerIdentifier = cancelationData.DS_APOLICE,
				CarIdentifier = cancelationData.NR_PLATAFORMA,
				CarChassis = cancelationData.DS_CHASSI,
				CustomerDocument = cancelationData.NR_CNPJ_CPF,
				DocumentType = cancelationData.CD_TIPO_PESSOA,
				CancelationDate = cancelationData.DATA_CANCELAMENTO,
				IdentificationCode = cancelationData.CD_IDENTIFICACAO
			};
		}

	}
}
