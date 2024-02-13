using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models
{
	public class SavedCharacterTree
	{
		[JsonProperty("treeID")]
		public string TreeID { get; set; }

		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonProperty("nodeIDs")]
		public List<int> NodeIDs { get; set; }

		[JsonProperty("nodePoints")]
		public List<int> NodePoints { get; set; }

		[JsonProperty("unspentPoints")]
		public int UnspentPoints { get; set; }

		[JsonProperty("nodesTaken")]
		public object NodesTaken { get; set; }
	}
}
