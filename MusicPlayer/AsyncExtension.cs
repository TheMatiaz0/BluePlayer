using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public static class AsyncExtension
	{
		public static async Task WaitUntil (Func<bool> conditions)
		{
			while (conditions())
			{
				await Task.Yield();
			}
		}
	}
}
