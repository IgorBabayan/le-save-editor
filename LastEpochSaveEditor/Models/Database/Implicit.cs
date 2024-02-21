namespace LastEpochSaveEditor.Models.Database
{
	internal class Implicit
	{
		[JsonProperty("property")]
		public int Property { get; set; }

		[JsonProperty("specialTag")]
		public int SpecialTag { get; set; }

		[JsonProperty("tags")]
		public int Tags { get; set; }

		[JsonProperty("type")]
		public int Type { get; set; }

		[JsonProperty("implicitValue")]
		public double ImplicitValue { get; set; }

		[JsonProperty("implicitMaxValue")]
		public double ImplicitMaxValue { get; set; }
	}
}
