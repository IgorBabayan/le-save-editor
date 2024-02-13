using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models
{
	public class Island
	{
		[JsonProperty("data")]
		public List<int> Data { get; set; }
	}
}
