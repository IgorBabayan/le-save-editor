namespace LastEpochSaveEditor.Services.Factories;

public interface IRepositoryFactory<TEntity>
	where TEntity : class
{
	IRepository<TEntity> Create();
}

public interface ISubItemRepositoryFactory : IRepositoryFactory<SubItem> { }
public interface IUniqueRepositoryFactory : IRepositoryFactory<Unique> { }
public interface ISetRepositoryFactory : IRepositoryFactory<Unique> { }

public class SubItemRepositoryFactory : ISubItemRepositoryFactory
{
	private readonly Database _database;

	public SubItemRepositoryFactory(Database database) => _database = database;

	public IRepository<SubItem> Create() => new SubItemRepository(_database);
}

public class UniqueRepositoryFactory : IUniqueRepositoryFactory
{
	private readonly Database _database;

	public UniqueRepositoryFactory(Database database) => _database = database;

	public IRepository<Unique> Create() => new UniqueRepository(_database);
}

public class SetRepositoryFactory : ISetRepositoryFactory
{
	private readonly Database _database;

	public SetRepositoryFactory(Database database) => _database = database;

	public IRepository<Unique> Create() => new SetRepository(_database);
}