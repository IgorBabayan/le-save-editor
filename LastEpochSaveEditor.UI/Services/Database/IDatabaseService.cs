namespace LastEpochSaveEditor.Services.Database;

public interface IDatabaseService
{
	Task<int> Count();

	Task<object> Get(QualityType quality, ItemInfoTypeEnum type, int id);
	Task<IEnumerable<object>> Get(QualityType quality, ItemInfoTypeEnum type);

	Task<object> GetWeapon(QualityType quality, int id, ItemInfoTypeEnum type);
	Task<IEnumerable<object>> GetWeapons(QualityType quality);
}
