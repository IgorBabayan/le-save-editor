namespace LastEpochSaveEditor.Models.Utils;

internal static class ItemDataParser
{
	internal static readonly IEnumerable<int> CharacterInventoryIds;

	static ItemDataParser() => CharacterInventoryIds = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

	public static ItemDataInfo ParseData(IList<int> data)
	{
		var hasQuality = false;
		var result = new ItemDataInfo();
		if (data.Count > 2)
		{
			result.Patch = data[0];
			result.Type = Enum.Parse<ItemInfoTypeEnum>(data[1].ToString(CultureInfo.InvariantCulture), true);
			result.BaseId = data[2];
		}

		if (data.Count > 3)
		{
			result.Quality = data[3] == 1 || data[3] == 2
				? QualityType.Magic
				: (QualityType)data[3];
			hasQuality = true;
		}

		if (hasQuality)
		{
			if (data.Count > 6)
			{
				result.Prefixes.Add(data[4]);
				result.Prefixes.Add(data[5]);
				result.Prefixes.Add(data[6]);
			}

			if (data.Count > 7 && result.Quality < QualityType.Unique)
			{
				switch (result.Patch)
				{
					case 0:
						result.Instability = data[7];
						break;

					case 1:
					case 2:
						result.ForgingPotencial = data[result.Patch == 0 ? 7 : 8];
						break;
				}
			}

			if (data.Count > 8)
			{
				if (result.Quality < QualityType.Unique)
				{
					var index = 9;
					if (data[8] > 128 & data[8] < 134)
					{
						result.AffixCount = data[8] - 129;
						if (data.Count > 11)
						{
							index = 12;
							result.IsSeal = true;
							result.SealedAffixes.Add(new AffixInfo(data[9], data[10], data[11]));
						}
					}
					else
						result.AffixCount = data[result.Patch == 0 ? 8 : 9];

					for (var position = 0; position < result.AffixCount && data.Count > index + 2; ++position)
					{
						result.Affixes.Add(new AffixInfo(data[index], data[index + 1], data[index + 2]));
						index += 3;
					}
				}
				else
				{
					result.UniqueOrSetId = result.Patch == 0 && data.Count > 8
						? data[7] * 256 + data[8]
						: data[8] * 256 + data[9];
					if (data.Count > 16)
					{
						for (var index = data[8] == 0 ? 10 : 9; index < 17; ++index)
							result.UniqueModifiers.Add(data[index]);
					}

					if (data.Count > 17 && result.Quality != QualityType.Set)
						result.LegendaryPotencial = data[17];

					if (result.Quality == QualityType.Legendary)
					{
						result.AffixCount = data[18];
						for (var index = result.Patch == 1 ? 18 : 19; index < index * 3 && index < data.Count; index += 3)
							result.LegendaryAffixes.Add(new AffixInfo(data[index], data[index + 1], data[index + 2]));
					}
				}
			}
		}

		return result;
	}
}