using LastEpochSaveEditor.Models.Utils;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
	public class CharacterInventory
	{
        public ItemDataInfo Helm { get; set; }
        public ItemDataInfo Body { get; set; }
        public ItemDataInfo Weapon { get; set; }
        public ItemDataInfo OffHand { get; set; }
        public ItemDataInfo Gloves { get; set; }
        public ItemDataInfo Belt { get; set; }
        public ItemDataInfo Boots { get; set; }
        public ItemDataInfo LeftRing { get; set; }
        public ItemDataInfo RightRing { get; set; }
        public ItemDataInfo Amulet { get; set; }
        public ItemDataInfo Relic { get; set; }

        public CharacterInventory(Dictionary<int, List<int>> collection)
        {
            if (collection.ContainsKey(2))
                Helm = ItemDataParser.ParseData(collection[2]);

			if (collection.ContainsKey(3))
				Body = ItemDataParser.ParseData(collection[3]);

			if (collection.ContainsKey(4))
				Weapon = ItemDataParser.ParseData(collection[4]);

			if (collection.ContainsKey(5))
				OffHand = ItemDataParser.ParseData(collection[5]);

			if (collection.ContainsKey(6))
				Gloves = ItemDataParser.ParseData(collection[6]);

			if (collection.ContainsKey(7))
				Belt = ItemDataParser.ParseData(collection[7]);

			if (collection.ContainsKey(8))
				Boots = ItemDataParser.ParseData(collection[8]);

			if (collection.ContainsKey(9))
				LeftRing = ItemDataParser.ParseData(collection[9]);

			if (collection.ContainsKey(10))
				RightRing = ItemDataParser.ParseData(collection[10]);

			if (collection.ContainsKey(11))
				Amulet = ItemDataParser.ParseData(collection[11]);

			if (collection.ContainsKey(12))
				Relic = ItemDataParser.ParseData(collection[12]);
        }
    }
}
