﻿using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LastEpochSaveEditor.Utils
{
	internal class FileDownloader
	{
		internal static async Task DownloadFile(string type, string path)
		{
			using (var httpClinet = new HttpClient())
			{
				var url = string.Format(Consts.IMAGE_URL, type, Path.GetFileName(path));
				using (var message = await httpClinet.GetAsync(url))
				{
					message.EnsureSuccessStatusCode();
					using (var contentStream = await message.Content.ReadAsStreamAsync())
					{
						using (var stream = new FileStream(path, FileMode.Create))
						{
							await contentStream.CopyToAsync(stream);
						}
					}
				}
			}
		}
	}
}