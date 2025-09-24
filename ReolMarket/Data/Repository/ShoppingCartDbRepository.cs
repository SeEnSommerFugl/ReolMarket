using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class ShoppingCartDbRepository : BaseDbRepository<ShoppingCart, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT ShoppingCartId, Quantity, TotalPrice
        FROM ShoppingCart";

        protected override string SqlSelectById => @"
        SELECT ShoppingCartId, Quantity, TotalPrice
        FROM ShoppingCart
        WHERE ShoppingCartId = @ShoppingCartId";

        protected override string SqlInsert => @"
        INSERT INTO ShoppingCart (ShoppingCartId)
        VALUES (@ShoppingCartId);";

        protected override string SqlUpdate => @"
        UPDATE ShoppingCart
           SET Quantity  = @Quantity,
               TotalPrice = @TotalPrice
         WHERE ShoppingCartId = @ShoppingCartId;";

        protected override string SqlDeleteById => @"
        DELETE FROM ShoppingCart
         WHERE ShoppingCartId = @ShoppingCartId;";

        protected override ShoppingCart Map(IDataRecord r) => new ShoppingCart
        {
            ShoppingCartId = r.GetGuid(r.GetOrdinal("ShoppingCartId")),
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@ShoppingCartId", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCartId", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartId;
        }

        protected override void BindUpdate(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCartId", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartId;
        }

        protected override Guid GetKey(ShoppingCart e) => e.ShoppingCartId;
    }

}
