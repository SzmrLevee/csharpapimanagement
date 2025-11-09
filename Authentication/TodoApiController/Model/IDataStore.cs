namespace TodoApiController.Model;

public interface IDataStore : IItemStore<TodoItem>, IItemStore<User>{
}

public interface IItemStore<T>
{
    IEnumerable<T> GetAll();
    bool Delete(T item);
    bool Update(T item);
    bool Add(T item);
}
