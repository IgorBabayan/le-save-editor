namespace LastEpochSaveEditor.Services;

internal static class FileDownloader
{
    internal static async Task DownloadImage(string type, string path)
    {
        using (var httpClinet = new HttpClient())
        {
            var url = string.Format(Const.IMAGE_URL, type, Path.GetFileName(path));
            using (var message = await httpClinet.GetAsync(url))
            {
                message.EnsureSuccessStatusCode();
                using (var contentStream = await message.Content.ReadAsStreamAsync())
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                        await contentStream.CopyToAsync(stream);
                }
            }
        }
    }

    internal static async Task<string> DownloadDatabase()
    {
		string response;
		using (var client = new HttpClient())
		{
			using (var request = await client.GetAsync(Const.DATA_URL))
				response = await request.Content.ReadAsStringAsync();
		}

        return response;
	}
}