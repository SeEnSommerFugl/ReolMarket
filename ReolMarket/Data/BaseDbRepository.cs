using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ReolMarket.Data
{
    internal abstract class BaseDbRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ObservableCollection<T> _items;

        public ObservableCollection<T> Items => _items;

        protected BaseDbRepository()
        {
            _items = new ObservableCollection<T>(QueryAll());
        }

        // --------- Template members you implement in each concrete repo ---------

        // SQL templates
        protected abstract string SqlSelectAll { get; }
        protected abstract string SqlSelectById { get; }
        protected abstract string SqlInsert { get; }        // return inserted id via OUTPUT or SELECT SCOPE_IDENTITY()
        protected abstract string SqlUpdate { get; }
        protected abstract string SqlDeleteById { get; }

        // Mapping and bindings
        protected abstract T Map(IDataRecord r);
        protected abstract void BindId(SqlCommand cmd, int id);
        protected abstract void BindInsert(SqlCommand cmd, T entity);
        protected abstract void BindUpdate(SqlCommand cmd, T entity);

        // Key helpers
        protected abstract int GetKey(T entity);                               // read key from entity
        protected virtual void AssignGeneratedIdIfAny(T entity, object? id)    // assign key after INSERT if you return one
        {
            // Optional to override in concrete repo
        }

        // --------------------- Public API ---------------------

        public IEnumerable<T> GetAll() => QueryAll();

        public T? GetById(int id)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlSelectById, con);
            BindId(cmd, id);
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        public void Add(T entity)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlInsert, con);
            BindInsert(cmd, entity);


            // If your INSERT returns the new id (recommended), capture it:
            object? newId = null;
            using (var rd = cmd.ExecuteReader())
            {
                if (rd.Read())
                {
                    // Take the first column of the first row as the new key
                    newId = rd.GetValue(0);
                }
            }
            AssignGeneratedIdIfAny(entity, newId);

            _items.Add(entity);
        }

        public void Update(T entity)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlUpdate, con);
            BindUpdate(cmd, entity);
            cmd.ExecuteNonQuery();

            // simplest: resync Items so WPF stays consistent
            ReloadItems();
        }

        public void Delete(int id)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlDeleteById, con);
            BindId(cmd, id);
            cmd.ExecuteNonQuery();

            // remove from in-memory list
            var existing = _items.FirstOrDefault(x => GetKey(x) == id);
            if (existing != null) _items.Remove(existing);
        }

        // --------------------- Helpers ---------------------

        protected void ReloadItems()
        {
            var fresh = QueryAll();
            _items.Clear();
            foreach (var e in fresh) _items.Add(e);
        }

        private List<T> QueryAll()
        {
            var list = new List<T>();
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlSelectAll, con);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }
    }
}
