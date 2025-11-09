
namespace TodoApiController.Model
{
    public class DataStore : IDataStore
    {
        readonly Dictionary<int, TodoItem> todoItems = [];
        readonly Dictionary<string, User> users = [];
        public bool Add(TodoItem item)
        {
            if (todoItems.ContainsKey(item.Id))
            {
                return false;
            }
            todoItems.Add(item.Id, item);
            return true;
        }

        public bool Add(User item)
        {
            if (users.ContainsKey(item.UserName))
            {
                return false;
            }
            users.Add(item.UserName, item);
            return true;
        }

        public bool Delete(TodoItem item)
        {
            if (!todoItems.ContainsKey(item.Id))
            {
                return false;
            }
            todoItems.Remove(item.Id);
            return true;
        }

        public bool Delete(User item)
        {
            if (!users.ContainsKey(item.UserName))
            {
                return false;
            }
            users.Remove(item.UserName);
            return true;
        }

        public IEnumerable<TodoItem> GetAll()
        {
            return todoItems.Values;
        }

        public bool Update(TodoItem item)
        {
            if (!todoItems.ContainsKey(item.Id))
            {
                return false;
            }
            todoItems[item.Id] = item;
            return true;
        }

        public bool Update(User item)
        {
            if (!users.ContainsKey(item.UserName))
            {
                return false;
            }
            users[item.UserName] = item;
            return true;
        }

        IEnumerable<User> IItemStore<User>.GetAll()
        {
            return users.Values;
        }
    }
}
