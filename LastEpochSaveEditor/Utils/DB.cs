namespace LastEpochSaveEditor.Utils;

internal sealed class DB : IDB
{
	private Database _database;

	private async Task LoadImpl()
	{
		string response;
		using (var client = new HttpClient())
		{
			using (var request = await client.GetAsync(Consts.DATA_URL))
				response = await request.Content.ReadAsStringAsync();
		}

		_database = JsonConvert.DeserializeObject<Database>(response)!;
		foreach (var itemType in _database!.ItemTypes)
			itemType.SubItems = itemType.SubItems.Where(x => x.CannotDrop == false).ToList();
	}

	public async Task Load()
	{
		await LoadImpl();
		if (!File.Exists(Consts.DATA_FILE_PATH))
			await File.WriteAllTextAsync(Consts.DATA_FILE_PATH, JsonConvert.SerializeObject(_database, Formatting.Indented));
	}

	public async Task Reload()
	{
		if (File.Exists(Consts.DATA_FILE_PATH))
			File.Delete(Consts.DATA_FILE_PATH);

		await Load();
	}

	private IEnumerable<ItemType> Get(string type, string subType)
	{
		var categories = _database?.ItemCategories?.FirstOrDefault(x => string.Equals(x.Name, type, StringComparison.OrdinalIgnoreCase))?.Categories;
		if (categories == null)
			throw new CategoryNotFoundException(type, subType);

		var subCategories = categories.FirstOrDefault(x => string.Equals(x.Name, subType, StringComparison.OrdinalIgnoreCase))?.Entries;
		if (subCategories == null)
			throw new CategoryNotFoundException(type, subType);

		var itemTypes = _database!.ItemTypes.Where(x => subCategories.Contains(x.BaseTypeID));
		if (!itemTypes.Any())
			throw new CategoryNotFoundException(type, subType);

		return itemTypes;
	}

	private IEnumerable<Unique> GetUniques(int baseType, bool isSet)
	{
		var uniques = _database!.Uniques.Where(x => x.BaseType == baseType && x.IsSetItem == isSet);
		if (uniques?.Any() == false)
			Enumerable.Empty<Unique>();

		return uniques!;
	}

	private ItemType GetBaseItemsGroup(string type, string subType, int subTypeId)
	{
		var baseItemsGroup = Get(type, subType);
		var baseItems = baseItemsGroup.FirstOrDefault(x => x.BaseTypeID == subTypeId);
		if (baseItems == null)
			throw new CategoryNotFoundException(type, subType, subTypeId);

		return baseItems;
	}

	private IEnumerable<Unique> GetUniquesGroup(int subTypeId, bool isSet)
	{
		var uniques = GetUniques(subTypeId, isSet);
		return uniques;
	}

	private Item Get(string type, string subType, int subTypeId)
	{
		var baseItems = GetBaseItemsGroup(type, subType, subTypeId);
		var uniques = GetUniquesGroup(subTypeId, false);
		var sets = GetUniquesGroup(subTypeId, true);

		return new()
		{
			Base = baseItems,
			Uniques = uniques,
			Sets = sets
		};
	}

	public Item GetOneHandAxes() => Get("Weapons", "One Handed Weapons", 5);

	public Item GetOneHandDaggers() => Get("Weapons", "One Handed Weapons", 6);

	public Item GetOneHandMaces() => Get("Weapons", "One Handed Weapons", 7);

	public Item GetOneHandScepters() => Get("Weapons", "One Handed Weapons", 8);

	public Item GetOneHandSwords() => Get("Weapons", "One Handed Weapons", 9);

	public Item GetWands() => Get("Weapons", "One Handed Weapons", 10);

	public Item GetTwoHandAxes() => Get("Weapons", "Two Handed Weapons", 12);

	public Item GetTwoHandMaces() => Get("Weapons", "Two Handed Weapons", 13);

	public Item GetTwoHandSpears() => Get("Weapons", "Two Handed Weapons", 14);

	public Item GetStaffs() => Get("Weapons", "Two Handed Weapons", 15);

	public Item GetTwoHandSwords() => Get("Weapons", "Two Handed Weapons", 16);

	public Item GetBows() => Get("Weapons", "Two Handed Weapons", 23);

	public Item GetQuivers() => Get("Off-Hand", "Off-Hand", 17);

	public Item GetShields() => Get("Off-Hand", "Off-Hand", 18);

	public Item GetCatalysts() => Get("Off-Hand", "Off-Hand", 19);

	public Item GetSmallIdols() => Get("Misc", "Idols", 25);

	public Item GetSmallLagonianIdols() => Get("Misc", "Idols", 26);

	public Item GetHumbleIdols() => Get("Misc", "Idols", 27);

	public Item GetStoutIdols() => Get("Misc", "Idols", 28);

	public Item GetGrandIdols() => Get("Misc", "Idols", 29);

	public Item GetLargeIdols() => Get("Misc", "Idols", 30);

	public Item GetOrnateIdols() => Get("Misc", "Idols", 31);

	public Item GetHugeIdols() => Get("Misc", "Idols", 31);

	public Item GetAdornedIdols() => Get("Misc", "Idols", 33);

	public Item GetHelmets() => Get("Armour", "Armour", 0);

	public Item GetAmulets() => Get("Misc", "Accessories", 20);

	public Item GetBodies() => Get("Armour", "Armour", 1);

	public Item GetRings() => Get("Misc", "Accessories", 21);

	public Item GetBelts() => Get("Armour", "Armour", 2);

	public Item GetGloves() => Get("Armour", "Armour", 4);

	public Item GetBoots() => Get("Armour", "Armour", 3);

	public Item GetRelics() => Get("Misc", "Accessories", 22);
}