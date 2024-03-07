namespace LastEpochSaveEditor.Models.Characters;

public class ClassInfo
{
	private static readonly Dictionary<int, List<ClassInfo>> _classes;

	public string Name { get; private set; }
	public Bitmap Icon { get; private set; }

	static ClassInfo()
	{
		_classes = new()
		{
			[0] =
			[
				new ClassInfo("Primalist", "avares://LastEpochSaveEditor/Assets/Classes/Primalist.png"),
				new ClassInfo("Beastmaster", "avares://LastEpochSaveEditor/Assets/Classes/Beastmaster.png"),
				new ClassInfo("Shaman", "avares://LastEpochSaveEditor/Assets/Classes/Shaman.png"),
				new ClassInfo("Druid", "avares://LastEpochSaveEditor/Assets/Classes/Druid.png")
			],
			[1] =
			[
				new ClassInfo("Mage", "avares://LastEpochSaveEditor/Assets/Classes/Mage.png"),
				new ClassInfo("Sorcerer", "avares://LastEpochSaveEditor/Assets/Classes/Sorcerer.png"),
				new ClassInfo("Spellblade", "avares://LastEpochSaveEditor/Assets/Classes/Spellblade.png"),
				new ClassInfo("Runemaster", "avares://LastEpochSaveEditor/Assets/Classes/Runemaster.png")
			],
			[2] =
			[
				new ClassInfo("Sentinel", "avares://LastEpochSaveEditor/Assets/Classes/Sentinel.png"),
				new ClassInfo("Void Knight", "avares://LastEpochSaveEditor/Assets/Classes/VoidKnight.png"),
				new ClassInfo("Forge Guard", "avares://LastEpochSaveEditor/Assets/Classes/ForgeGuard.png"),
				new ClassInfo("Paladin", "avares://LastEpochSaveEditor/Assets/Classes/Paladin.png")
			],
			[3] =
			[
				new ClassInfo("Acolyte", "avares://LastEpochSaveEditor/Assets/Classes/Acolyte.png"),
				new ClassInfo("Necromancer", "avares://LastEpochSaveEditor/Assets/Classes/Necromancer.png"),
				new ClassInfo("Lich", "avares://LastEpochSaveEditor/Assets/Classes/Lich.png"),
				new ClassInfo("Warlock", "avares://LastEpochSaveEditor/Assets/Classes/Warlock.png")
			],
			[4] =
			[
				new ClassInfo("Rogue", "avares://LastEpochSaveEditor/Assets/Classes/Rogue.png"),
				new ClassInfo("Bladedancer", "avares://LastEpochSaveEditor/Assets/Classes/Bladedancer.png"),
				new ClassInfo("Marksman", "avares://LastEpochSaveEditor/Assets/Classes/Marksman.png"),
				new ClassInfo("Falconer", "avares://LastEpochSaveEditor/Assets/Classes/Falconer.png")
			]
		};
	}

	private ClassInfo(string name, string icon)
	{
		Name = name;
		Icon = new Bitmap(AssetLoader.Open(new Uri(icon)));
	}

	public static ClassInfo Parse(int classId, int mastery)
	{
		var result = _classes[classId][mastery];
		return result;
	}
}