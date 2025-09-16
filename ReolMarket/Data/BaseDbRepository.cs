using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ReolMarket.Data
{
    internal abstract class BaseDbRepository<T, TKey> : IBaseRepository<T, TKey> where T : class where TKey : notnull
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
        protected abstract string SqlInsert { get; }
        protected abstract string SqlUpdate { get; }
        protected abstract string SqlDeleteById { get; }

        // Mapping and bindings
        protected abstract T Map(IDataRecord r);
        protected abstract void BindId(SqlCommand cmd, TKey id);
        protected abstract void BindInsert(SqlCommand cmd, T entity);
        protected abstract void BindUpdate(SqlCommand cmd, T entity);

        // Key helpers
        protected abstract TKey GetKey(T entity);
        //protected virtual void AssignGeneratedIdIfAny(T entity, object? id)
        //{

        //}

        // --------------------- Public API ---------------------

        public IEnumerable<T> GetAll() => QueryAll();

        public T? GetById(TKey id)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlSelectById, con);
            BindId(cmd, id);
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        public void Add(T entity)
        {
            if (GetKey(entity) is Guid g && g == Guid.Empty)
                throw new InvalidOperationException("Entity key must be set before insert.");

            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlInsert, con);
            BindInsert(cmd, entity);

            //if (cmd.ExecuteNonQuery() != 1)
            //    throw new InvalidOperationException("Insert failed.");

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

        public void Delete(TKey id)
        {
            using var con = Db.OpenConnection();
            using var cmd = new SqlCommand(SqlDeleteById, con);
            BindId(cmd, id);
            cmd.ExecuteNonQuery();

            var existing = _items.FirstOrDefault(x =>
                        Equals(GetKey(x), id));
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
