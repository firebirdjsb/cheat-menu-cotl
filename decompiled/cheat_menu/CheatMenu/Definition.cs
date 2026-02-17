using System;
using System.Reflection;

namespace CheatMenu
{
	public class Definition
	{
		public static bool IsCheatMethod(MethodInfo info)
		{
			return ReflectionHelper.HasAttribute<CheatDetails>(info) != null;
		}

		public static string GetCheatFlagID(Type declaringType, string methodName)
		{
			return declaringType.Name + "-" + methodName;
		}

		public static string GetCheatFlagID(MethodBase info)
		{
			return info.DeclaringType.Name + "-" + info.Name;
		}

		public static string GetCheatFlagID(MethodInfo info)
		{
			return info.DeclaringType.Name + "-" + info.Name;
		}

		public Definition(MethodInfo info, CheatCategoryEnum category)
		{
			this._info = info;
			this._category = category;
			this._categoryName = category.GetCategoryName();
			this._details = ReflectionHelper.HasAttribute<CheatDetails>(info);
			this._cheatWIP = ReflectionHelper.HasAttribute<CheatWIP>(info);
			this._flagName = Definition.GetCheatFlagID(info);
		}

		public virtual CheatCategoryEnum CategoryEnum
		{
			get
			{
				return this._category;
			}
		}

		public virtual string CategoryName
		{
			get
			{
				return this._categoryName;
			}
		}

		public virtual MethodInfo MethodInfo
		{
			get
			{
				return this._info;
			}
		}

		public virtual bool IsWIPCheat
		{
			get
			{
				return this._cheatWIP != null;
			}
		}

		public virtual CheatDetails Details
		{
			get
			{
				return this._details;
			}
		}

		public virtual bool IsModeCheat
		{
			get
			{
				return this._details.IsFlagCheat;
			}
		}

		public virtual string FlagName
		{
			get
			{
				return this._flagName;
			}
		}

		private readonly MethodInfo _info;

		private readonly CheatCategoryEnum _category;

		private readonly string _categoryName;

		private readonly CheatDetails _details;

		private readonly CheatWIP _cheatWIP;

		private readonly string _flagName;
	}
}
