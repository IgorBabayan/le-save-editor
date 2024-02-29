namespace LastEpochSaveEditor.Services;

internal static class WebPConverter
{
    internal static async Task Convert(string webpFilePath)
    {
        if (!File.Exists(webpFilePath))
            throw new FileNotFoundException($"File {Path.GetFileName(webpFilePath)} does not exists");

        var rootPath = Directory.GetParent(webpFilePath);
        var fileName = Path.GetFileNameWithoutExtension(webpFilePath);
        var fullPath = Path.Combine(rootPath!.FullName, $"{fileName}.png");
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        using (var webpFileStream = new FileStream(webpFilePath, FileMode.Open))
        {
            using (var pngFileStream = new FileStream(fullPath, FileMode.Create))
            {
                using (var image = SixLabors.ImageSharp.Image.Load(webpFileStream))
                    await image.SaveAsPngAsync(pngFileStream);
            }
        }
    }
}