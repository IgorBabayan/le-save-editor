using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LastEpochSaveEditor.Utils
{
	internal class FileDownloader
	{
		private const string URL_BASE = "https://assets-ng.maxroll.gg/last-epoch/webp/{0}/{1}";

		internal static async Task DownloadFile(string type, string path)
		{
			using (var httpClinet = new HttpClient())
			{
				var url = string.Format(URL_BASE, type, Path.GetFileName(path));
				using (var message = await httpClinet.GetAsync(url))
				{
					try
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
					catch (System.Exception e)
					{

					}
				}
			}
		}
	}
}
