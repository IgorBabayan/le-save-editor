using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Database
{
	internal class Item
	{
        public ItemType Base { get; set; }
        public IEnumerable<Unique> Uniques { get; set; }
        public IEnumerable<Unique> Sets { get; set; }
    }
}
