namespace LastEpochSaveEditor.Data;

public interface IEnumerableRepository<TEntity>
	where TEntity : class
{
	IEnumerable<TEntity> GetOneHandAxes();
	IEnumerable<TEntity> GetOneHandDaggers();
	IEnumerable<TEntity> GetOneHandMaces();
	IEnumerable<TEntity> GetOneHandScepters();
	IEnumerable<TEntity> GetOneHandSwords();
	IEnumerable<TEntity> GetWands();
	IEnumerable<TEntity> GetTwoHandAxes();
	IEnumerable<TEntity> GetTwoHandMaces();
	IEnumerable<TEntity> GetTwoHandSpears();
	IEnumerable<TEntity> GetStaffs();
	IEnumerable<TEntity> GetBows();
	IEnumerable<TEntity> GetTwoHandSwords();
	IEnumerable<TEntity> GetAmulets();
	IEnumerable<TEntity> GetBodies();
	IEnumerable<TEntity> GetBelts();
	IEnumerable<TEntity> GetBoots();
	IEnumerable<TEntity> GetGloves();
	IEnumerable<TEntity> GetHelmets();
	IEnumerable<TEntity> GetRelics();
	IEnumerable<TEntity> GetRings();
	IEnumerable<TEntity> GetQuivers();
	IEnumerable<TEntity> GetShields();
	IEnumerable<TEntity> GetCatalysts();
	IEnumerable<TEntity> GetSmallIdols();
	IEnumerable<TEntity> GetSmallLagonianIdols();
	IEnumerable<TEntity> GetHumbleIdols();
	IEnumerable<TEntity> GetStoutIdols();
	IEnumerable<TEntity> GetGrandIdols();
	IEnumerable<TEntity> GetLargeIdols();
	IEnumerable<TEntity> GetOrnateIdols();
	IEnumerable<TEntity> GetHugeIdols();
	IEnumerable<TEntity> GetAdornedIdols();
}

public interface ISingleRepository<TEntity>
	where TEntity : class
{
	TEntity GetOneHandAxe(int id);
	TEntity GetOneHandDagger(int id);
	TEntity GetOneHandMace(int id);
	TEntity GetOneHandScepter(int id);
	TEntity GetOneHandSword(int id);
	TEntity GetWand(int id);
	TEntity GetTwoHandAxe(int id);
	TEntity GetTwoHandMace(int id);
	TEntity GetTwoHandSpear(int id);
	TEntity GetStaff(int id);
	TEntity GetBow(int id);
	TEntity GetTwoHandSword(int id);
	TEntity GetAmulet(int id);
	TEntity GetBody(int id);
	TEntity GetBelt(int id);
	TEntity GetBoot(int id);
	TEntity GetGlove(int id);
	TEntity GetHelmet(int id);
	TEntity GetRelic(int id);
	TEntity GetRing(int id);
	TEntity GetQuiver(int id);
	TEntity GetShield(int id);
	TEntity GetCatalyst(int id);
	TEntity GetSmallIdol(int id);
	TEntity GetSmallLagonianIdol(int id);
	TEntity GetHumbleIdol(int id);
	TEntity GetStoutIdol(int id);
	TEntity GetGrandIdol(int id);
	TEntity GetLargeIdol(int id);
	TEntity GetOrnateIdol(int id);
	TEntity GetHugeIdol(int id);
	TEntity GetAdornedIdol(int id);
}

public interface IRepository<TEntity> : ISingleRepository<TEntity>, IEnumerableRepository<TEntity>
	where TEntity : class
{
	TEntity Get(ItemInfoTypeEnum itemType, int id);
	IEnumerable<TEntity> Get(ItemInfoTypeEnum itemType);
}
