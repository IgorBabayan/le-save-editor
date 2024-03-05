namespace LastEpochSaveEditor.Factories;

public interface ISetRepositoryFactory : IRepositoryFactory<Unique> { }

public class SetRepositoryFactory : ISetRepositoryFactory
{
	private readonly IDatabaseFactory _databaseFactory;

	public SetRepositoryFactory(IDatabaseFactory databaseFactory) => _databaseFactory = databaseFactory;

	public async Task<IRepository<Unique>> Create()
	{
		var database = await _databaseFactory.Create();
		return new SetRepository(database);
	}
}
