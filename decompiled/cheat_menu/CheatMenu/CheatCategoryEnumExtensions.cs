using System;
using System.Collections.Generic;
using System.Reflection;

namespace CheatMenu
{
	public static class CheatCategoryEnumExtensions
	{
		[Init]
		public static void Init()
		{
			CheatCategoryEnumExtensions.s_forwardsCache = new Dictionary<CheatCategoryEnum, string>();
			CheatCategoryEnumExtensions.s_backwardsCache = new Dictionary<string, CheatCategoryEnum>();
		}

		public static string GetCategoryName(this CheatCategoryEnum enumValue)
		{
			string text;
			if (CheatCategoryEnumExtensions.s_forwardsCache.TryGetValue(enumValue, out text))
			{
				return text;
			}
			StringEnum attributeOfTypeEnum = ReflectionHelper.GetAttributeOfTypeEnum<StringEnum>(enumValue);
			if (attributeOfTypeEnum == null)
			{
				throw new Exception("Expected StringEnum on CheatCategory enum but not found!");
			}
			CheatCategoryEnumExtensions.s_forwardsCache[enumValue] = attributeOfTypeEnum.Value;
			return attributeOfTypeEnum.Value;
		}

		public static CheatCategoryEnum GetEnumFromName(string name)
		{
			CheatCategoryEnum cheatCategoryEnum;
			if (CheatCategoryEnumExtensions.s_backwardsCache.TryGetValue(name, out cheatCategoryEnum))
			{
				return cheatCategoryEnum;
			}
			foreach (FieldInfo fieldInfo in typeof(CheatCategoryEnum).GetFields())
			{
				StringEnum stringEnum = (StringEnum)fieldInfo.GetCustomAttribute(typeof(StringEnum));
				if (stringEnum != null && stringEnum.Value == name)
				{
					CheatCategoryEnum cheatCategoryEnum2 = (CheatCategoryEnum)fieldInfo.GetValue(null);
					CheatCategoryEnumExtensions.s_backwardsCache[name] = cheatCategoryEnum2;
					return cheatCategoryEnum2;
				}
			}
			CheatCategoryEnumExtensions.s_backwardsCache[name] = CheatCategoryEnum.NONE;
			return CheatCategoryEnum.NONE;
		}

		private static Dictionary<CheatCategoryEnum, string> s_forwardsCache = new Dictionary<CheatCategoryEnum, string>();

		private static Dictionary<string, CheatCategoryEnum> s_backwardsCache = new Dictionary<string, CheatCategoryEnum>();
	}
}
