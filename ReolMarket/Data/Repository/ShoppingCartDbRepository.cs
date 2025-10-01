using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class ShoppingCartDbRepository : BaseDbRepository<ShoppingCart, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT ShoppingCart_Id, Quantity, TotalPrice
        FROM ShoppingCart";

        protected override string SqlSelectById => @"
        SELECT ShoppingCart_Id, Quantity, TotalPrice
        FROM ShoppingCart
        WHERE ShoppingCart_Id = @ShoppingCart_Id";

        protected override string SqlInsert => @"
        INSERT INTO ShoppingCart (ShoppingCart_Id)
        VALUES (@ShoppingCart_Id);";

        protected override string SqlUpdate => @"
        UPDATE ShoppingCart
           SET Quantity  = @Quantity,
               TotalPrice = @TotalPrice
         WHERE ShoppingCart_Id = @ShoppingCart_Id;";

        protected override string SqlDeleteById => @"
        DELETE FROM ShoppingCart
         WHERE ShoppingCart_Id = @ShoppingCart_Id;";

        protected override ShoppingCart Map(IDataRecord r) => new ShoppingCart
        {
            ShoppingCartId = r.GetGuid(r.GetOrdinal("ShoppingCart_Id")),
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@ShoppingCart_Id", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCart_Id", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartId;
        }

        protected override void BindUpdate(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCart_Id", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartId;
        }

        protected override Guid GetKey(ShoppingCart e) => e.ShoppingCartId;

    }

}
