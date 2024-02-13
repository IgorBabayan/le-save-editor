﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models
{
	public class TimelineCompletion
	{
		[JsonProperty("timelineID")]
		public int TimelineID { get; set; }

		[JsonProperty("progress")]
		public List<int> Progress { get; set; }
	}
}
