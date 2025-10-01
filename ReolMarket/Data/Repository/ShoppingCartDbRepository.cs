using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class ShoppingCartDbRepository : BaseDbRepository<ShoppingCart, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT ShoppingCart_ID, Quantity, TotalPrice
        FROM ShoppingCart";

        protected override string SqlSelectById => @"
        SELECT ShoppingCart_ID, Quantity, TotalPrice
        FROM ShoppingCart
        WHERE ShoppingCart_ID = @ShoppingCart_ID";

        protected override string SqlInsert => @"
        INSERT INTO ShoppingCart (ShoppingCart_ID)
        VALUES (@ShoppingCart_ID);";

        protected override string SqlUpdate => @"
        UPDATE ShoppingCart
           SET Quantity  = @Quantity,
               TotalPrice = @TotalPrice
         WHERE ShoppingCart_ID = @ShoppingCart_ID;";

        protected override string SqlDeleteById => @"
        DELETE FROM ShoppingCart
         WHERE ShoppingCart_ID = @ShoppingCart_ID;";

        protected override ShoppingCart Map(IDataRecord r) => new ShoppingCart
        {
            ShoppingCartID = r.GetGuid(r.GetOrdinal("ShoppingCart_ID")),
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
        }

        protected override void BindUpdate(SqlCommand cmd, ShoppingCart e)
        {
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
        }

        protected override Guid GetKey(ShoppingCart e) => e.ShoppingCartID;

    }

}
