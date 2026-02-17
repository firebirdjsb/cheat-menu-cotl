using System;
using System.Collections.Generic;

namespace CheatMenu
{
	public static class CheatUtils
	{
		public static List<T> CloneList<T>(List<T> list)
		{
			List<T> list2 = new List<T>();
			foreach (T t in list)
			{
				list2.Add(t);
			}
			return list2;
		}

		public static bool IsDebugMode
		{
			get
			{
				return false;
			}
		}

		public static bool InvertBool(bool value)
		{
			return !value;
		}

		public static T[] Concat<T>(T[] arrayOne, T[] arrayTwo)
		{
			T[] array = new T[arrayOne.Length + arrayTwo.Length];
			arrayOne.CopyTo(array, 0);
			arrayTwo.CopyTo(array, arrayOne.Length);
			return array;
		}
	}
}
