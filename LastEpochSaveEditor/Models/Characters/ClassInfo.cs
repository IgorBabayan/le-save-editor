namespace LastEpochSaveEditor.Models.Characters;

public class ClassInfo
{
	private static Dictionary<int, List<ClassInfo>> _classes = new()
	{
		[0] = new List<ClassInfo>
		{
			new("Primalist", "/LastEpochSaveEditor;component/Assets/Images/Classes/Primalist.png"),
			new("Beastmaster", "/LastEpochSaveEditor;component/Assets/Images/Classes/Beastmaster.png"),
			new("Shaman", "/LastEpochSaveEditor;component/Assets/Images/Classes/Shaman.png"),
			new("Druid", "/LastEpochSaveEditor;component/Assets/Images/Classes/Druid.png")
		},
		[1] = new List<ClassInfo>
		{
			new("Mage", "/LastEpochSaveEditor;component/Assets/Images/Classes/Mage.png"),
			new("Sorcerer", "/LastEpochSaveEditor;component/Assets/Images/Classes/Sorcerer.png"),
			new("Spellblade", "/LastEpochSaveEditor;component/Assets/Images/Classes/Spellblade.png"),
			new("Runemaster", "/LastEpochSaveEditor;component/Assets/Images/Classes/Runemaster.png")
		},
		[2] = new List<ClassInfo>
		{
			new("Sentinel", "/LastEpochSaveEditor;component/Assets/Images/Classes/Sentinel.png"),
			new("Void Knight", "/LastEpochSaveEditor;component/Assets/Images/Classes/VoidKnight.png"),
			new("Forge Guard", "/LastEpochSaveEditor;component/Assets/Images/Classes/ForgeGuard.png"),
			new("Paladin", "/LastEpochSaveEditor;component/Assets/Images/Classes/Paladin.png")
		},
		[3] = new List<ClassInfo>
		{
			new("Acolyte", "/LastEpochSaveEditor;component/Assets/Images/Classes/Acolyte.png"),
			new("Necromancer", "/LastEpochSaveEditor;component/Assets/Images/Classes/Necromancer.png"),
			new("Lich", "/LastEpochSaveEditor;component/Assets/Images/Classes/Lich.png"),
			new("Warlock", "/LastEpochSaveEditor;component/Assets/Images/Classes/Warlock.png")
		},
		[4] = new List<ClassInfo>
		{
			new("Rogue", "/LastEpochSaveEditor;component/Assets/Images/Classes/Rogue.png"),
			new("Bladedancer", "/LastEpochSaveEditor;component/Assets/Images/Classes/Bladedancer.png"),
			new("Marksman", "/LastEpochSaveEditor;component/Assets/Images/Classes/Marksman.png"),
			new("Falconer", "/LastEpochSaveEditor;component/Assets/Images/Classes/Falconer.png")
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