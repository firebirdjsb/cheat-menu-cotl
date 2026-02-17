using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CheatMenu
{
	public static class DefinitionManager
	{
		public static List<Definition> GetAllCheatMethods()
		{
			List<Definition> list = new List<Definition>();
			foreach (Type type in ReflectionHelper.GetLoadableTypes(typeof(DefinitionManager).Assembly))
			{
				if (typeof(IDefinition).IsAssignableFrom(type) && type.IsClass)
				{
					CheatCategory cheatCategory = ReflectionHelper.HasAttribute<CheatCategory>(type);
					foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
					{
						if (Definition.IsCheatMethod(methodInfo))
						{
							Definition definition = new Definition(methodInfo, cheatCategory.Category);
							list.Add(definition);
						}
					}
				}
			}
			return list;
		}

		public static Dictionary<string, Definition> CheatFunctionToDetails(List<Definition> allCheats)
		{
			Dictionary<string, Definition> dictionary = new Dictionary<string, Definition>();
			foreach (Definition definition in allCheats)
			{
				if (dictionary.ContainsKey(definition.MethodInfo.Name))
				{
					throw new Exception("MethodInfo conflict with name " + definition.MethodInfo.Name + ", please fix!");
				}
				dictionary[definition.MethodInfo.Name] = definition;
			}
			return dictionary;
		}

		public static Dictionary<CheatCategoryEnum, List<Definition>> GroupCheatsByCategory(List<Definition> allCheats)
		{
			Dictionary<CheatCategoryEnum, List<Definition>> dictionary = new Dictionary<CheatCategoryEnum, List<Definition>>();
			foreach (Definition definition in allCheats)
			{
				List<Definition> list;
				if (!dictionary.TryGetValue(definition.CategoryEnum, out list))
				{
					list = new List<Definition>();
				}
				list.Add(definition);
				dictionary[definition.CategoryEnum] = list;
			}
			foreach (KeyValuePair<CheatCategoryEnum, List<Definition>> keyValuePair in dictionary)
			{
				keyValuePair.Value.Sort(delegate(Definition a, Definition b)
				{
					int num = a.Details.SortOrder.CompareTo(b.Details.SortOrder);
					if (num != 0)
					{
						return num;
					}
					return string.Compare(a.Details.Title, b.Details.Title);
				});
			}
			return dictionary;
		}

		public static Action BuildGUIContentFn()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("", typeof(void), new Type[0]);
			MethodInfo method = typeof(CheatMenuGui).GetMethod("CategoryButton", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method2 = typeof(CheatMenuGui).GetMethod("Button", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method3 = typeof(CheatMenuGui).GetMethod("ButtonWithFlagS", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method4 = typeof(CheatMenuGui).GetMethod("ButtonWithFlag", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method5 = typeof(CheatMenuGui).GetMethod("IsWithinCategory", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method6 = typeof(CheatMenuGui).GetMethod("IsWithinSpecificCategory", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method7 = typeof(FlagManager).GetMethod("IsFlagEnabledStr", BindingFlags.Static | BindingFlags.Public);
			MethodInfo method8 = typeof(CheatMenuGui).GetMethod("BackButton", BindingFlags.Static | BindingFlags.Public);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			Dictionary<CheatCategoryEnum, List<Definition>> dictionary = DefinitionManager.GroupCheatsByCategory(DefinitionManager.GetAllCheatMethods());
			List<CheatCategoryEnum> list = new List<CheatCategoryEnum>(dictionary.Keys);
			list.Sort(delegate(CheatCategoryEnum a, CheatCategoryEnum b)
			{
				int num = (int)a;
				return num.CompareTo((int)b);
			});
			Label label = ilgenerator.DefineLabel();
			Label label2 = ilgenerator.DefineLabel();
			ilgenerator.EmitCall(OpCodes.Call, method5, null);
			ilgenerator.Emit(OpCodes.Brtrue, label);
			foreach (CheatCategoryEnum cheatCategoryEnum in list)
			{
				ilgenerator.Emit(OpCodes.Ldstr, cheatCategoryEnum.GetCategoryName());
				ilgenerator.EmitCall(OpCodes.Call, method, null);
				ilgenerator.Emit(OpCodes.Pop);
			}
			ilgenerator.Emit(OpCodes.Br, label2);
			ilgenerator.MarkLabel(label);
			ilgenerator.EmitCall(OpCodes.Call, method8, null);
			ilgenerator.Emit(OpCodes.Pop);
			foreach (KeyValuePair<CheatCategoryEnum, List<Definition>> keyValuePair in dictionary)
			{
				foreach (Definition definition in keyValuePair.Value)
				{
					if (!definition.IsWIPCheat || CheatUtils.IsDebugMode)
					{
						Label label3 = ilgenerator.DefineLabel();
						ilgenerator.Emit(OpCodes.Ldstr, definition.CategoryName);
						ilgenerator.EmitCall(OpCodes.Call, method6, null);
						ilgenerator.Emit(OpCodes.Brfalse, label3);
						if (!definition.Details.IsMultiNameFlagCheat)
						{
							ilgenerator.Emit(OpCodes.Ldstr, definition.Details.Title);
							if (!definition.IsModeCheat)
							{
								ilgenerator.EmitCall(OpCodes.Callvirt, method2, null);
							}
							else
							{
								ilgenerator.Emit(OpCodes.Ldstr, definition.FlagName);
								ilgenerator.EmitCall(OpCodes.Callvirt, method3, null);
							}
						}
						else
						{
							ilgenerator.Emit(OpCodes.Ldstr, definition.Details.OnTitle);
							ilgenerator.Emit(OpCodes.Ldstr, definition.Details.OffTitle);
							ilgenerator.Emit(OpCodes.Ldstr, definition.FlagName);
							ilgenerator.EmitCall(OpCodes.Callvirt, method4, null);
						}
						ilgenerator.Emit(OpCodes.Brfalse, label3);
						if (definition.MethodInfo.GetParameters().Length == 1 && definition.IsModeCheat)
						{
							ilgenerator.Emit(OpCodes.Ldstr, definition.FlagName);
							ilgenerator.EmitCall(OpCodes.Call, method7, null);
						}
						ilgenerator.EmitCall(OpCodes.Call, definition.MethodInfo, null);
						ilgenerator.MarkLabel(label3);
					}
				}
			}
			ilgenerator.MarkLabel(label2);
			ilgenerator.Emit(OpCodes.Ret);
			return (Action)dynamicMethod.CreateDelegate(typeof(Action));
		}
	}
}
