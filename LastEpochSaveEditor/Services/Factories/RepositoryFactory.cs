namespace LastEpochSaveEditor.Services.Factories;

public interface IRepositoryFactory<TEntity>
	where TEntity : class
{
	Task<IRepository<TEntity>> Create();
}

public interface ISubItemRepositoryFactory : IRepositoryFactory<SubItem> { }
public interface IUniqueRepositoryFactory : IRepositoryFactory<Unique> { }
public interface ISetRepositoryFactory : IRepositoryFactory<Unique> { }

public class SubItemRepositoryFactory : ISubItemRepositoryFactory
{
	private readonly IDatabaseFactory _databaseFactory;
	
	public SubItemRepositoryFactory(IDatabaseFactory databaseFactory) => _databaseFactory = databaseFactory;

	public async Task<IRepository<SubItem>> Create()
	{
		var database = await _databaseFactory.Create();
		return new SubItemRepository(database);
	}
}

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