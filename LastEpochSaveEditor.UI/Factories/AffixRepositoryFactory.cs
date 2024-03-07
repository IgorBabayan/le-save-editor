namespace LastEpochSaveEditor.Factories;

public interface IAffixRepositoryFactory : IRepositoryFactory<Affix>;

public class AffixRepositoryFactory(IDatabaseFactory databaseFactory) : IAffixRepositoryFactory
{
	public async Task<IRepository<Affix>> Create()
	{
		var database = await databaseFactory.Create();
		return new AffixRepository(database);
	}
}