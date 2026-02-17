using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CheatMenu
{
	public class UnityAnnotationHelper
	{
		public UnityAnnotationHelper()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Type[] array;
			try
			{
				array = executingAssembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				array = ex.Types.Where((Type t) => t != null).ToArray<Type>();
			}
			List<ValueTuple<MethodInfo, int>> list = new List<ValueTuple<MethodInfo, int>>();
			List<MethodInfo> list2 = new List<MethodInfo>();
			List<MethodInfo> list3 = new List<MethodInfo>();
			Type[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				foreach (MethodInfo methodInfo in array2[i].GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (methodInfo.GetCustomAttribute<Init>() != null)
					{
						EnforceOrderFirst customAttribute = methodInfo.GetCustomAttribute<EnforceOrderFirst>();
						EnforceOrderLast customAttribute2 = methodInfo.GetCustomAttribute<EnforceOrderLast>();
						if (customAttribute != null)
						{
							list.Add(new ValueTuple<MethodInfo, int>(methodInfo, customAttribute.Order));
						}
						else if (customAttribute2 != null)
						{
							list3.Add(methodInfo);
						}
						else
						{
							list2.Add(methodInfo);
						}
					}
					if (methodInfo.GetCustomAttribute<Unload>() != null)
					{
						this._unloadMethods.Add(methodInfo);
					}
					if (methodInfo.GetCustomAttribute<OnGui>() != null)
					{
						this._onGuiMethods.Add(methodInfo);
					}
					if (methodInfo.GetCustomAttribute<Update>() != null)
					{
						this._updateMethods.Add(methodInfo);
					}
				}
			}
			list.Sort(([TupleElementNames(new string[] { "method", "order" })] ValueTuple<MethodInfo, int> a, [TupleElementNames(new string[] { "method", "order" })] ValueTuple<MethodInfo, int> b) => a.Item2.CompareTo(b.Item2));
			this._initMethods.AddRange(list.Select(([TupleElementNames(new string[] { "method", "order" })] ValueTuple<MethodInfo, int> x) => x.Item1));
			this._initMethods.AddRange(list2);
			this._initMethods.AddRange(list3);
		}

		public void RunAllInit()
		{
			foreach (MethodInfo methodInfo in this._initMethods)
			{
				try
				{
					object obj = null;
					if (!methodInfo.IsStatic)
					{
						PropertyInfo property = methodInfo.DeclaringType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
						if (property != null)
						{
							obj = property.GetValue(null);
						}
					}
					methodInfo.Invoke(obj, null);
				}
				catch (Exception ex)
				{
					string text = "[UnityAnnotationHelper] Failed to run Init on {0}.{1}: {2}";
					Type declaringType = methodInfo.DeclaringType;
					Debug.LogError(string.Format(text, (declaringType != null) ? declaringType.Name : null, methodInfo.Name, ex));
				}
			}
		}

		public void RunAllUnload()
		{
			foreach (MethodInfo methodInfo in this._unloadMethods)
			{
				try
				{
					object obj = null;
					if (!methodInfo.IsStatic)
					{
						PropertyInfo property = methodInfo.DeclaringType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
						if (property != null)
						{
							obj = property.GetValue(null);
						}
					}
					methodInfo.Invoke(obj, null);
				}
				catch (Exception ex)
				{
					string text = "[UnityAnnotationHelper] Failed to run Unload on {0}.{1}: {2}";
					Type declaringType = methodInfo.DeclaringType;
					Debug.LogError(string.Format(text, (declaringType != null) ? declaringType.Name : null, methodInfo.Name, ex));
				}
			}
		}

		public Action BuildRunAllOnGuiDelegate()
		{
			return delegate
			{
				foreach (MethodInfo methodInfo in this._onGuiMethods)
				{
					try
					{
						methodInfo.Invoke(null, null);
					}
					catch (Exception ex)
					{
						string text = "[UnityAnnotationHelper] OnGui error in {0}.{1}: {2}";
						Type declaringType = methodInfo.DeclaringType;
						Debug.LogError(string.Format(text, (declaringType != null) ? declaringType.Name : null, methodInfo.Name, ex));
					}
				}
			};
		}

		public Action BuildRunAllUpdateDelegate()
		{
			return delegate
			{
				foreach (MethodInfo methodInfo in this._updateMethods)
				{
					try
					{
						methodInfo.Invoke(null, null);
					}
					catch (Exception ex)
					{
						string text = "[UnityAnnotationHelper] Update error in {0}.{1}: {2}";
						Type declaringType = methodInfo.DeclaringType;
						Debug.LogError(string.Format(text, (declaringType != null) ? declaringType.Name : null, methodInfo.Name, ex));
					}
				}
			};
		}

		private readonly List<MethodInfo> _initMethods = new List<MethodInfo>();

		private readonly List<MethodInfo> _unloadMethods = new List<MethodInfo>();

		private readonly List<MethodInfo> _onGuiMethods = new List<MethodInfo>();

		private readonly List<MethodInfo> _updateMethods = new List<MethodInfo>();
	}
}
