using InsurerServices.CommonBusiness.Data;

namespace InsurerServices.CommonBusiness.Contracts
{
	public abstract class BaseBusinessContext
	{
		protected readonly Logs Logs;
		protected readonly Context Context;

		protected BaseBusinessContext(Context context, Logs logs)
		{
			Logs = logs;
			Context = context;
		}
	}
}
