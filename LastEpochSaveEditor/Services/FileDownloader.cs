namespace LastEpochSaveEditor.Services;

internal static class FileDownloader
{
    internal static async Task DownloadImage(string type, string path)
    {
        using (var httpClient = new HttpClient())
        {
            var isUnique = path.Contains(UNIQUE_FOLDER_NAME) || path.Contains(SET_FOLDER_NAME);
            var url = string.Format(IMAGE_URL, isUnique ? UNIQUE_FOLDER_NAME.ToLowerInvariant() : type.GetItemName(), Path.GetFileName(path));
            using (var message = await httpClient.GetAsync(url))
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
			using (var request = await client.GetAsync(DATA_URL))
				response = await request.Content.ReadAsStringAsync();
		}

        return response;
	}
}