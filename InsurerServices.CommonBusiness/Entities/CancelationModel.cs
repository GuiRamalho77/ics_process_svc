using System;
using InsurerServices.CommonBusiness.Enums;

namespace InsurerServices.CommonBusiness.Entities
{
	public class CancelationModel
	{

		public int PolicyIdentifier { get; set; }
		public string PolicyProposalIdentifier { get; set; }
		public string IdentificationCode { get; set; }

		public string PolicyInsurerIdentifier { get; set; }

		public int CarIdentifier { get; set; }
		public string CarChassis { get; set; }
		public EnumCustomerDocument? DocumentType { get; set; }
		public string CustomerDocument { get; set; }


		public DateTime CancelationDate { get; set; }
	}
}
