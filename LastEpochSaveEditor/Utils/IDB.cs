using LastEpochSaveEditor.Models.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LastEpochSaveEditor.Utils
{
	internal interface IDB
	{
		Task Load();
		Task Reload();

		IEnumerable<ItemType> Get1HWeapons();
		IEnumerable<ItemType> Get2HWeapons();
		IEnumerable<ItemType> GetIdols();
		IEnumerable<ItemType> GetOffHands();
		IEnumerable<ItemType> GetWeapons();
	
		ItemType GetAmulets();
		ItemType GetArmors();
		ItemType GetBelts();
		ItemType GetBoots();
		ItemType GetGloves();
		ItemType GetHelmets();
		ItemType GetRelics();
		ItemType GetRings();
	}
}