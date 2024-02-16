using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
    public class ClassInfo
    {
		private static Dictionary<int, List<ClassInfo>> _classes = new Dictionary<int, List<ClassInfo>>
		{
			[0] = new List<ClassInfo>
			{
				new ClassInfo("Primalist", "/LastEpochSaveEditor;component/Images/Classes/Primalist.png"),
				new ClassInfo("Beastmaster", "/LastEpochSaveEditor;component/Images/Classes/Beastmaster.png"),
				new ClassInfo("Shaman", "/LastEpochSaveEditor;component/Images/Classes/Shaman.png"),
				new ClassInfo("Druid", "/LastEpochSaveEditor;component/Images/Classes/Druid.png")
			},
			[1] = new List<ClassInfo>
			{
				new ClassInfo("Mage", "/LastEpochSaveEditor;component/Images/Classes/Mage.png"),
				new ClassInfo("Sorcerer", "/LastEpochSaveEditor;component/Images/Classes/Sorcerer.png"),
				new ClassInfo("Spellblade", "/LastEpochSaveEditor;component/Images/Classes/Spellblade.png"),
				new ClassInfo("Runemaster", "/LastEpochSaveEditor;component/Images/Classes/Runemaster.png")
			},
			[2] = new List<ClassInfo>
			{
				new ClassInfo("Sentinel", "/LastEpochSaveEditor;component/Images/Classes/Sentinel.png"),
				new ClassInfo("Void Knight", "/LastEpochSaveEditor;component/Images/Classes/VoidKnight.png"),
				new ClassInfo("Forge Guard", "/LastEpochSaveEditor;component/Images/Classes/ForgeGuard.png"),
				new ClassInfo("Paladin", "/LastEpochSaveEditor;component/Images/Classes/Paladin.png")
			},
			[3] = new List<ClassInfo>
			{
				new ClassInfo("Acolyte", "/LastEpochSaveEditor;component/IImagescons/Classes/Acolyte.png"),
				new ClassInfo("Necromancer", "/LastEpochSaveEditor;component/Images/Classes/Necromancer.png"),
				new ClassInfo("Lich", "/LastEpochSaveEditor;component/Images/Classes/Lich.png"),
				new ClassInfo("Warlock", "/LastEpochSaveEditor;component/Images/Classes/Warlock.png")
			},
			[4] = new List<ClassInfo>
			{
				new ClassInfo("Rogue", "/LastEpochSaveEditor;component/Images/Classes/Rogue.png"),
				new ClassInfo("Bladedancer", "/LastEpochSaveEditor;component/Images/Classes/Bladedancer.png"),
				new ClassInfo("Marksman", "/LastEpochSaveEditor;component/Images/Classes/Marksman.png"),
				new ClassInfo("Falconer", "/LastEpochSaveEditor;component/Images/Classes/Falconer.png")
			}
		};

        public string Name { get; private set; }
        public string Icon { get; private set; }

		private ClassInfo(string name, string icon)
		{
			Name = name;
			Icon = icon;
		}

		public static ClassInfo Parse(int classId, int mastery)
		{
			var result = _classes[classId][mastery];
			return result;
		}
	}
}
