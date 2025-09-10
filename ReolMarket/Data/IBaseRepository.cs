using System.Collections.ObjectModel;

namespace ReolMarket.Data
{
    internal interface IBaseRepository<T> where T : class
    {
        ObservableCollection<T> Items { get; }
        IEnumerable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
    }
}
