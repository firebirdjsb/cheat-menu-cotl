using System;

namespace CheatMenu
{
	[AttributeUsage(AttributeTargets.Method)]
	public class EnforceOrderFirst : Attribute
	{
		public int Order { get; }

		public EnforceOrderFirst(int order = 0)
		{
			this.Order = order;
		}
	}
}
