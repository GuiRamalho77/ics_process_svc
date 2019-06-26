using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.Entities
{
	public class LogModel
    {
        public string Message { get; set; }
        public string Step { get; set; }
        public EnumLog LogType { get; set; }
		public  EnumProcessType FileType { get; set; }
    }
}
