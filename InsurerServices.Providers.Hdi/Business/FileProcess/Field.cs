using System;
using InsurerServices.CommonBusiness.Enums;
using InsurerServices.CommonBusiness.Helpers.Extensions;
using InsurerServices.Providers.Hdi.ConfigurationModels;
using InsurerServices.Providers.Hdi.Enums;

namespace InsurerServices.Providers.Hdi.Business.FileProcess
{
	internal class Field
	{
		public int? Sequence { get; private set; }
		public int? InitialPosition { get; private set; }
		public int? Length { get; private set; }
		public char? ComplementaryChar { get; private set; }
		public string DefaultValue { get; private set; }
		public EnumAlignment? Alignment { get; private set; }
		public EnumProcessType? FileType { get; private set; }
		private string _value;

		public Field(EnumProcessType fileType, FileFieldConfiguration config)
		{
			Sequence = config.Sequence;
			InitialPosition = config.InitialPosition;
			Length = config.Length;
			FileType = fileType;
			ComplementaryChar = config.ComplementaryChar;
			DefaultValue = config.DefaultValue;
			Alignment = config.Alignment ?? EnumAlignment.Left;
			_value = config.DefaultValue.isNullOrEmpty() ? string.Empty : config.DefaultValue;
		}


		public string Value
		{
			get => GetValue();
			set => SetValue(value);
		}

		private void SetValue(string value)
		{
			_value = value ?? string.Empty;
		}

		private string GetValue()
		{
			if (Length == null)
				throw new Exception("O tamanho não pode ser nulo");

			if (_value.Length == Length)
				return _value.Trim();

			if (_value.Length > Length)
			{
				_value = _value.Get((int)Length);
				return Value;
			}

			if (_value.Length < Length)
				return CheckAlignment();

			return string.Empty;
		}


		private string CheckAlignment()
		{
			if (ComplementaryChar == null)
				ComplementaryChar = char.Parse("\0");

			if (Alignment == EnumAlignment.Right)
				return _value.PadRight((int)Length, (char)ComplementaryChar);

			if (Alignment == EnumAlignment.Left)
				return _value.PadLeft((int)Length, (char)ComplementaryChar);

			return string.Empty;
		}
	}
}
