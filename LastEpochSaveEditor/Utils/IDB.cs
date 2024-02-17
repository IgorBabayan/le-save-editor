using LastEpochSaveEditor.Models.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LastEpochSaveEditor.Utils
{
	internal interface IDB
	{
		Task Load();
		Task Reload();

		Item GetOneHandAxes();
		Item GetOneHandDaggers();
		Item GetOneHandMaces();
		Item GetOneHandScepters();
		Item GetOneHandSwords();
		Item GetWands();
		Item GetTwoHandAxes();
		Item GetTwoHandMaces();
		Item GetTwoHandSpears();
		Item GetStaffs();
		Item GetBows();
		Item GetTwoHandSwords();
		Item GetAmulets();
		Item GetBody();
		Item GetBelts();
		Item GetBoots();
		Item GetGloves();
		Item GetHelmets();
		Item GetRelics();
		Item GetRings();
		Item GetQuivers();
		Item GetShields();
		Item GetCatalysts();
		Item GetSmallIdols();
		Item GetSmallLagonianIdols();
		Item GetHumbleIdols();
		Item GetStoutIdols();
		Item GetGrandIdols();
		Item GetLargeIdols();
		Item GetOrnateIdols();
		Item GetHugeIdols();
		Item GetAdornedIdols();

		IEnumerable<Item> Get1HandWeapons() => new[] { GetOneHandAxes(), GetOneHandDaggers(), GetOneHandMaces(), GetOneHandScepters(),
			GetOneHandSwords(), GetWands() };

		IEnumerable<Item> Get2HandWeapons() => new[] { GetTwoHandAxes(), GetTwoHandMaces(), GetTwoHandSpears(), GetStaffs(),
			GetTwoHandSwords(), GetBows() };

		IEnumerable<Item> GetIdols() => new[] { GetSmallIdols(), GetSmallLagonianIdols(), GetHumbleIdols(), GetStoutIdols(), GetGrandIdols(),
			GetLargeIdols(), GetOrnateIdols(), GetHugeIdols(), GetAdornedIdols() };

		IEnumerable<Item> GetAccessories() => new[] { GetAmulets(), GetRings(), GetRelics() };

		IEnumerable<Item> GetArmours() => new[] { GetHelmets(), GetBody(), GetBelts(), GetBoots(), GetGloves() };

		IEnumerable<Item> GetOffHands() => new[] { GetQuivers(), GetShields(), GetCatalysts() };

		IEnumerable<Item> GetAll() => GetOffHands()
			.Union(GetArmours())
			.Union(GetAccessories())
			.Union(Get1HandWeapons())
			.Union(Get2HandWeapons())
			.Union(GetIdols());

		int Count()
		{
			var count = 0;
			foreach (var item in GetAll())
				count += item.Base.SubItems.Count + item.Uniques.Count() + item.Sets.Count();

			return count;
		}

		Dictionary<string, IEnumerable<Item>> GetFolderStructure() => new()
		{
			["Off Hands"] = GetOffHands(),
			["Armours"] = GetArmours(),
			["Accessories"] = GetAccessories(),
			["One hand weapons"] = Get1HandWeapons(),
			["Two hand weapons"] = Get2HandWeapons(),
			["Idols"] = GetIdols()
		};
	}
}