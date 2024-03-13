namespace LastEpochSaveEditor.Utils;

public class BitmapImageJsonConverter : JsonConverter<BitmapImage>
{
	public override void WriteJson(JsonWriter writer, BitmapImage? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		using (var memoryStream = new MemoryStream())
		{
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(value));
			encoder.Save(memoryStream);
			
			var imageData = memoryStream.ToArray();
			writer.WriteValue(Convert.ToBase64String(imageData));
		}
	}

	public override BitmapImage? ReadJson(JsonReader reader, Type objectType, BitmapImage? existingValue, bool hasExistingValue,
		JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
			return null;

		var base64String = (string)reader.Value!;
		var imageData = Convert.FromBase64String(base64String);

		var bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = new MemoryStream(imageData);
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();

		return bitmapImage;
	}
}