using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace InsurerServices.CommonBusiness.Helpers
{
	public static class Files
	{
		public const string NewLineWindows = "\r\n";
		public const string NewLineUnix = "\n";
		public static void MoveFile(string fromPath, string toPath)
		{

		}


		public static string[] ConvertFromByteToStringArray(byte[] bytes)
		{
			var file = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
			if (file.Contains(Files.NewLineWindows))
				return Encoding.ASCII.GetString(bytes, 0, bytes.Length).Split(Files.NewLineWindows);
			else if (file.Contains(Files.NewLineUnix))
				return Encoding.ASCII.GetString(bytes, 0, bytes.Length).Split(Files.NewLineUnix);

			return null;

		}


		public static void CreateDirectory(string path)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		public static void UnzipDeleteZipFile(string path)
		{
			if (path.Contains(".zip"))
			{
				using (ZipArchive archive = ZipFile.OpenRead(path))
				{
					if(archive.Entries.Count>1)
						throw new Exception("inumeros_arquivos_no_zip");

					foreach (ZipArchiveEntry entry in archive.Entries)
					{
						entry.ExtractToFile(path.Replace(".zip",".txt"));
						
					}
				}
				File.Delete(path);
			}
		}
	}
}
