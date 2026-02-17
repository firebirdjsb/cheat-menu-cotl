using System;

namespace CheatMenu
{
	public class CheatCategory : Attribute
	{
		public CheatCategory(CheatCategoryEnum enumValue)
		{
			this._category = enumValue;
		}

		public virtual CheatCategoryEnum Category
		{
			get
			{
				return this._category;
			}
		}

		private readonly CheatCategoryEnum _category;
	}
}
