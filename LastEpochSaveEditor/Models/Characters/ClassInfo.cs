using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
    public class ClassInfo
    {
		private static Dictionary<int, List<ClassInfo>> _classes = new Dictionary<int, List<ClassInfo>>
		{
			[0] = new List<ClassInfo>
			{
				new ClassInfo("Primalist", "/LastEpochSaveEditor;component/Icons/Classes/Primalist.png"),
				new ClassInfo("Beastmaster", "/LastEpochSaveEditor;component/Icons/Classes/Beastmaster.png"),
				new ClassInfo("Shaman", "/LastEpochSaveEditor;component/Icons/Classes/Shaman.png"),
				new ClassInfo("Druid", "/LastEpochSaveEditor;component/Icons/Classes/Druid.png")
			},
			[1] = new List<ClassInfo>
			{
				new ClassInfo("Mage", "/LastEpochSaveEditor;component/Icons/Classes/Mage.png"),
				new ClassInfo("Sorcerer", "/LastEpochSaveEditor;component/Icons/Classes/Sorcerer.png"),
				new ClassInfo("Spellblade", "/LastEpochSaveEditor;component/Icons/Classes/Spellblade.png"),
				new ClassInfo("Runemaster", "/LastEpochSaveEditor;component/Icons/Classes/Runemaster.png")
			},
			[2] = new List<ClassInfo>
			{
				new ClassInfo("Sentinel", "/LastEpochSaveEditor;component/Icons/Classes/Sentinel.png"),
				new ClassInfo("Void Knight", "/LastEpochSaveEditor;component/Icons/Classes/VoidKnight.png"),
				new ClassInfo("Forge Guard", "/LastEpochSaveEditor;component/Icons/Classes/ForgeGuard.png"),
				new ClassInfo("Paladin", "/LastEpochSaveEditor;component/Icons/Classes/Paladin.png")
			},
			[3] = new List<ClassInfo>
			{
				new ClassInfo("Acolyte", "/LastEpochSaveEditor;component/Icons/Classes/Acolyte.png"),
				new ClassInfo("Necromancer", "/LastEpochSaveEditor;component/Icons/Classes/Necromancer.png"),
				new ClassInfo("Lich", "/LastEpochSaveEditor;component/Icons/Classes/Lich.png"),
				new ClassInfo("Warlock", "/LastEpochSaveEditor;component/Icons/Classes/Warlock.png")
			},
			[4] = new List<ClassInfo>
			{
				new ClassInfo("Rogue", "/LastEpochSaveEditor;component/Icons/Classes/Rogue.png"),
				new ClassInfo("Bladedancer", "/LastEpochSaveEditor;component/Icons/Classes/Bladedancer.png"),
				new ClassInfo("Marksman", "/LastEpochSaveEditor;component/Icons/Classes/Marksman.png"),
				new ClassInfo("Falconer", "/LastEpochSaveEditor;component/Icons/Classes/Falconer.png")
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
