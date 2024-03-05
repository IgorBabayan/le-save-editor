namespace LastEpochSaveEditor.Factories;

public interface IRepositoryFactory<TEntity>
	where TEntity : class
{
	Task<IRepository<TEntity>> Create();
}
