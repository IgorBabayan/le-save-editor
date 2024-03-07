namespace LastEpochSaveEditor.Factories;

public interface ISetRepositoryFactory : IRepositoryFactory<Unique>;

public class SetRepositoryFactory(IDatabaseFactory databaseFactory) : ISetRepositoryFactory
{
	public async Task<IRepository<Unique>> Create()
	{
		var database = await databaseFactory.Create();
		return new SetRepository(database);
	}
}
