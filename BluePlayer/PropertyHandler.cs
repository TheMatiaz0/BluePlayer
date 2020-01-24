using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

public class PropertyHandler
{
	public static TimeSpan GetDuration(IShellProperty value)
	{
		ulong ticks = (ulong)value.ValueAsObject;
		return TimeSpan.FromTicks((long)ticks);
	}

	public static uint GetNumber (IShellProperty value)
	{
		if (value == null || value.ValueAsObject == null)
		{
			return 0;
		}

		return (uint)value.ValueAsObject;
	}

	public static string GetValue(IShellProperty value)
	{
		if (value == null || value.ValueAsObject == null)
		{
			return String.Empty;
		}

		return value.ValueAsObject.ToString();
	}

	public static string[] GetValues(IShellProperty value)
	{
		if (value == null || value.ValueAsObject == null)
		{
			return null;
		}

		return (string[])value.ValueAsObject;
	}
}
