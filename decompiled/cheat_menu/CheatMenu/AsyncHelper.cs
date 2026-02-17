using System;
using System.Threading.Tasks;

namespace CheatMenu
{
	public static class AsyncHelper
	{
		public static global::System.Threading.Tasks.Task WaitSeconds(int seconds)
		{
			return global::System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds((double)seconds));
		}
	}
}
