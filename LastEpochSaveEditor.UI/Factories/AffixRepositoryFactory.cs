namespace LastEpochSaveEditor.Factories;

public interface IAffixRepositoryFactory : IRepositoryFactory<Affix> { }

public class AffixRepositoryFactory : IAffixRepositoryFactory
{
	private readonly IDatabaseFactory _databaseFactory;

	public AffixRepositoryFactory(IDatabaseFactory databaseFactory) => _databaseFactory = databaseFactory;

	public async Task<IRepository<Affix>> Create()
	{
		var database = await _databaseFactory.Create();
		return new AffixRepository(database);
	}
}