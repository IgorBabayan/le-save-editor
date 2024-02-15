using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Database
{
	internal class Database
	{
		[JsonProperty("itemTypes")]
		public List<ItemType> ItemTypes { get; set; }

		[JsonProperty("affixes")]
		public List<Affix> Affixes { get; set; }

		[JsonProperty("itemCategories")]
		public List<ItemCategory> ItemCategories { get; set; }
	}
}
