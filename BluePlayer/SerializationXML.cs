using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public static class SerializationXML
{
	public static void SaveFile<T>(string path, object o)
	{
		XmlSerializer xsl = new XmlSerializer(typeof(T));
		using (FileStream fs = new FileStream(path, FileMode.Create))
		{
			xsl.Serialize(fs, o);
			fs.Close();
		}
	}

	public static object LoadFile<T>(StreamReader stream)
	{
		XmlSerializer xsl = new XmlSerializer(typeof(T));


		return xsl.Deserialize(stream);

	}

}
