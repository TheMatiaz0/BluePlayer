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
	public static Icon GetIcon(IShellProperty value)
	{
		return (Icon)value.ValueAsObject;
	}


	public static IconReference GetIconReference (IShellProperty value)
	{
		return (IconReference)value.ValueAsObject;
	}

	public static Bitmap GetBitmap (IShellProperty value)
	{
		return (Bitmap)value.ValueAsObject;
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
