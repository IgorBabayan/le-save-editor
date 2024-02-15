using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Database
{
	internal class ItemCategory
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("categories")]
		public List<Category> Categories { get; set; }
	}
}
