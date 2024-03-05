namespace LastEpochSaveEditor.Factories;

public interface IUniqueRepositoryFactory : IRepositoryFactory<Unique> { }

public class UniqueRepositoryFactory : IUniqueRepositoryFactory
{
	private readonly IDatabaseFactory _databaseFactory;

	public UniqueRepositoryFactory(IDatabaseFactory databaseFactory) => _databaseFactory = databaseFactory;

	public async Task<IRepository<Unique>> Create()
	{
		var database = await _databaseFactory.Create();
		return new UniqueRepository(database);
	}
}
