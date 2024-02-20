using LastEpochSaveEditor.Models.Database;
using LastEpochSaveEditor.Models.Utils;
using LastEpochSaveEditor.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace LastEpochSaveEditor.Models.Characters
{
	public interface ICharacterInventory : IInventory
	{
		ItemDataInfo Helm { get; set; }
		ItemDataInfo Body { get; set; }
		ItemDataInfo Weapon { get; set; }
		ItemDataInfo OffHand { get; set; }
		ItemDataInfo Gloves { get; set; }
		ItemDataInfo Belt { get; set; }
		ItemDataInfo Boots { get; set; }
		ItemDataInfo LeftRing { get; set; }
		ItemDataInfo RightRing { get; set; }
		ItemDataInfo Amulet { get; set; }
		ItemDataInfo Relic { get; set; }
	}

	public class CharacterInventory : ICharacterInventory
	{
		private readonly ILogger<CharacterInventory> _logger;
		private readonly IDB _db;

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

		public CharacterInventory(ILogger<CharacterInventory> logger)
		{
			_logger = logger;
			_db = App.GetService<IDB>();
		}

		public void Parse(IDictionary<int, List<int>> data)
        {
			ParseHelm(data);
			ParseBody(data);
			ParseWeapon(data);
			ParseOffHand(data);
			ParseGloves(data);
			ParseBelt(data);
			ParseBoots(data);
			ParseLeftRing(data);
			ParseRightRing(data);
			ParseAmulet(data);
			ParseRelic(data);
		}

		private static string GetQualityFolderName(QualityType quality)
		{
			switch (quality)
			{

				case QualityType.Basic:
				case QualityType.Magic:
				case QualityType.Rare:
				case QualityType.Exalted:
					return Consts.BASIC_FOLDER_NAME;

				case QualityType.Unique:
				case QualityType.Legendary:
					return Consts.UNIQUE_FOLDER_NAME;

				default:
					return Consts.SET_FOLDER_NAME;
			}
		}

		private static string GetIcon(Item item, ItemDataInfo itemInfo, string basePath, string defaultPath, string? baseTypeName = null)
		{
			var id = itemInfo.Quality >= QualityType.Unique ? itemInfo.UniqueOrSetId : itemInfo.Id;
			var name = item.FindName(itemInfo.Quality, id);

			if (string.IsNullOrEmpty(name))
				return defaultPath;

			var quality = GetQualityFolderName(itemInfo.Quality);
			if (string.IsNullOrEmpty(basePath))
				return Path.GetFullPath($"{basePath}{quality}/{name.ToLowerInvariant().Replace(" ", "_")}.png");

			return Path.GetFullPath($"{basePath}{baseTypeName}/{quality}/{name.ToLowerInvariant().Replace(" ", "_")}.png");
		}

		private static string GetIcon(IEnumerable<Item> items, ItemDataInfo itemInfo, string basePath, string defaultPath)
		{
			var path = string.Empty;
			foreach (var item in items)
			{
				path = GetIcon(item, itemInfo, basePath, defaultPath, item.Base.BaseTypeName);
				if (!string.Equals(path, defaultPath, StringComparison.OrdinalIgnoreCase))
					break;
			}

			return path;
		}

		private void ParseHelm(IDictionary<int, List<int>> data)
		{
			Helm = Parse(data, 2, "Helm");
			Helm.Icon = GetIcon(_db.GetHelmets(), Helm, Consts.HELMETS, Consts.DEFAULT_HELMET);
		}

		private void ParseBody(IDictionary<int, List<int>> data)
		{
			Body = Parse(data, 3, "Body");
			Body.Icon = GetIcon(_db.GetBodies(), Body, Consts.BODY_ARMOR, Consts.DEFAULT_BODY_ARMOR);
		}

		private void ParseWeapon(IDictionary<int, List<int>> data)
		{
			Weapon = Parse(data, 4, "Weapon");

			var icon = GetIcon(_db.Get1HandWeapons(), Weapon, Consts.ONE_HAND_WEAPONS, Consts.DEFAULT_WEAPON);
			if (string.Equals(icon, Consts.DEFAULT_WEAPON, StringComparison.OrdinalIgnoreCase))
				icon = GetIcon(_db.Get2HandWeapons(), Weapon, Consts.TWO_HAND_WEAPONS, Consts.DEFAULT_WEAPON);
			
			Weapon.Icon = icon;
		}

		private void ParseOffHand(IDictionary<int, List<int>> data)
		{
			OffHand = Parse(data, 5, "Off-hand");
			OffHand.Icon = GetIcon(_db.GetOffHands(), OffHand, Consts.OFF_HAND, Consts.DEFAULT_OFF_HAND);
		}

		private void ParseGloves(IDictionary<int, List<int>> data)
		{
			Gloves = Parse(data, 6, "Gloves");
			Gloves.Icon = GetIcon(_db.GetGloves(), Gloves, Consts.GLOVES, Consts.DEFAULT_GLOVES);
		}

		private void ParseBelt(IDictionary<int, List<int>> data)
		{
			Belt = Parse(data, 7, "Belt");
			Belt.Icon = GetIcon(_db.GetBelts(), Belt, Consts.BELTS, Consts.DEFAULT_BELTS);
		}

		private void ParseBoots(IDictionary<int, List<int>> data)
		{
			Boots = Parse(data, 8, "Boots");
			Boots.Icon = GetIcon(_db.GetBoots(), Boots, Consts.BOOTS, Consts.DEFAULT_BOOTS);
		}

		private void ParseLeftRing(IDictionary<int, List<int>> data)
		{
			LeftRing = Parse(data, 9, "Left ring");
			LeftRing.Icon = GetIcon(_db.GetRings(), LeftRing, Consts.RING, Consts.DEFAULT_LEFT_RING);
		}

		private void ParseRightRing(IDictionary<int, List<int>> data)
		{
			RightRing = Parse(data, 10, "Right ring");
			RightRing.Icon = GetIcon(_db.GetRings(), RightRing, Consts.RING, Consts.DEFAULT_RIGHT_RING);
		}

		private void ParseAmulet(IDictionary<int, List<int>> data)
		{
			Amulet = Parse(data, 11, "Amulet");
			Amulet.Icon = GetIcon(_db.GetAmulets(), Amulet, Consts.AMULET, Consts.DEFAULT_AMULET);
		}

		private void ParseRelic(IDictionary<int, List<int>> data)
		{
			Relic = Parse(data, 12, "Relic");
			Relic.Icon = GetIcon(_db.GetRelics(), Relic, Consts.RELIC, Consts.DEFAULT_RELIC);
		}

		private ItemDataInfo Parse(IDictionary<int, List<int>> data, int index, string name)
		{
			var hasError = false;
			var result = ItemDataInfo.Empty;
			try
			{
				_logger.LogInformation($"Begin parse '{name}' data.");

				if (data.ContainsKey(index))
					result = ItemDataParser.ParseData(data[index]);
			}
			catch (Exception exception)
			{
				hasError = true;
				_logger.LogError(exception, $"Can't parse '{name}' data.");
			}
			finally
			{
				if (hasError)
					_logger.LogWarning($"Parsing '{name}' data ended with error.");
				else
					_logger.LogInformation($"Parsing '{name}' data ended successfully.");
			}
			
			return result;
		}
	}
}
