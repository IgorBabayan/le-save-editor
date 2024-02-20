﻿using LastEpochSaveEditor.Models.Characters;
using System.Collections.Generic;
using System.Linq;

namespace LastEpochSaveEditor.Models.Database
{
	internal class Item
	{
        public ItemType Base { get; set; }
        public IEnumerable<Unique> Uniques { get; set; }
        public IEnumerable<Unique> Sets { get; set; }

		public string FindName(QualityType itemType, int id)
		{
			switch (itemType)
			{
				case QualityType.Unique:
				case QualityType.Legendary:
					return Uniques.FirstOrDefault(x => x.UniqueID == id && x.IsSetItem == false)?.Name;
				
				case QualityType.Set:
					return Sets.FirstOrDefault(x => x.UniqueID == id && x.IsSetItem == true)?.Name;

				case QualityType.Basic:
				case QualityType.Magic:
				case QualityType.Rare:
				case QualityType.Exalted:
					return Base.SubItems.FirstOrDefault(x => x.SubTypeID == id)?.Name;
			}

			return string.Empty;
		}
	}
}
