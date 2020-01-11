using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PropertyHandler
{
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
