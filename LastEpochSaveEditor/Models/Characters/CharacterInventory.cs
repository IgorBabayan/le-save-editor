using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
	public class CharacterInventory
	{
        public int IsNewType { get; set; }
        public int Type { get; set; }
        public int Id { get; set; }
        public int Quality { get; set; }
		public IList<int> Prefixes { get; set; } = new List<int>();
        public int InstabilityOrForgingPotencial { get; set; }
        public int AffixNumberOrUniqueOrSetId { get; set; }
        public AffixInfo Affix1 { get; set; }
        public AffixInfo Affix2 { get; set; }
        public AffixInfo Affix3 { get; set; }
        public AffixInfo Affix4 { get; set; }
    }
}
