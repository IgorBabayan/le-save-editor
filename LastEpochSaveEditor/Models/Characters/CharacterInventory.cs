using LastEpochSaveEditor.Models.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
	public class CharacterInventory
	{
		private readonly ILogger<CharacterInventory> _logger;

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

		public CharacterInventory(ILogger<CharacterInventory> logger) => _logger = logger;

		public void Parse(Dictionary<int, List<int>> data)
        {
			Helm = Parse(data, 2, "Helm");
			Body = Parse(data, 3, "Body");
			Weapon = Parse(data, 4, "Weapon");
			OffHand = Parse(data, 5, "Off-hand");
			Gloves = Parse(data, 6, "Gloves");
			Belt = Parse(data, 7, "Belt");
			Boots = Parse(data, 8, "Boots");
			LeftRing = Parse(data, 9, "Left ring");
			RightRing = Parse(data, 10, "Right ring");
			Amulet = Parse(data, 11, "Amulet");
			Relic = Parse(data, 12, "Relic");
		}

		private ItemDataInfo Parse(Dictionary<int, List<int>> data, int index, string name)
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
