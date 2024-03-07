namespace LastEpochSaveEditor.Factories;

public interface IUniqueRepositoryFactory : IRepositoryFactory<Unique>;

public class UniqueRepositoryFactory(IDatabaseFactory databaseFactory) : IUniqueRepositoryFactory
{
	public async Task<IRepository<Unique>> Create()
	{
		var database = await databaseFactory.Create();
		return new UniqueRepository(database);
	}
}
