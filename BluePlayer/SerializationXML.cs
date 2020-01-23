using BluePlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public static class SerializationXML
{
	public static void SaveFile<T>(string path, T o)
		where T : class, new()
	{
		XmlSerializer xsl = new XmlSerializer(typeof(T));
		using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
		{
			xsl.Serialize(fs, o);
			fs.Close();
		}
	}

	public static object LoadFile<T>(string path)
	{
		XmlSerializer xsl = new XmlSerializer(typeof(T));

		StreamReader stream = new StreamReader(path);

		object obj = xsl.Deserialize(stream);
		stream.Close();

		return obj;

	}

}
