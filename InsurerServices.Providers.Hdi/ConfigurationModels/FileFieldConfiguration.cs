using InsurerServices.Providers.Hdi.Enums;

namespace InsurerServices.Providers.Hdi.ConfigurationModels
{
	public class FileFieldConfiguration
	{
		public int? Sequence { get; set; }
		public int? InitialPosition { get; set; }
		public int? Length { get; set; }
		public bool? Required { get; set; }
		public char? ComplementaryChar { get; set; }
		public EnumAlignment? Alignment { get; set; } = EnumAlignment.Left;
		public string DefaultValue { get; set; }

		public bool IsValid()
		{ 
			if (this.Sequence == null)
				return false;
			if (this.InitialPosition == null)
				return false;
			if (this.Length == null)
				return false;
			if (this.Required == null)
				return false;
			if (this.ComplementaryChar == null)
				return false;

			return true;
		}
	}
}
