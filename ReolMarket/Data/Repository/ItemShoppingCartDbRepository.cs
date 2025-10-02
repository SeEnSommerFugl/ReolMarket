using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;
using static ReolMarket.MVVM.Model.ItemShoppingCart;

namespace ReolMarket.Data.Repository
{
    internal class ItemShoppingCartDbRepository : BaseDbRepository<ItemShoppingCart, ItemShoppingCartKey>
    {
        // Read all columns needed by the model
        protected override string SqlSelectAll => @"
            SELECT Item_ID, ShoppingCart_ID, Quantity,UnitPrice
            FROM ItemShoppingCart";
        protected override string SqlSelectById => @"
            SELECT Item_ID, ShoppingCart_ID, Quantity, UnitPrice
            FROM ItemShoppingCart
            WHERE Item_ID = @Item_ID AND ShoppingCart_ID = @ShoppingCart_ID";

        // Return the new identity as the first column so BaseDbRepository can capture it
        protected override string SqlInsert => @"
            INSERT INTO ItemShoppingCart (Item_ID, ShoppingCart_ID, Quantity, UnitPrice)
            VALUES (@Item_ID, @ShoppingCart_ID, @Quantity, @UnitPrice);";

        protected override string SqlUpdate => null!;

        protected override string SqlDeleteById => @"
            DELETE FROM ItemShoppingCart
            WHERE Item_ID = @Item_ID AND ShoppingCart_ID = @ShoppingCart_ID;";

        protected override ItemShoppingCart Map(IDataRecord r) => new ItemShoppingCart
        {
            ItemID = r.GetGuid(r.GetOrdinal("Item_ID")),
            ShoppingCartID = r.GetGuid(r.GetOrdinal("ShoppingCart_ID")),
            Quantity = r.GetInt32(r.GetOrdinal("Quantity")),
            UnitPrice = r.GetDecimal(r.GetOrdinal("UnitPrice"))
        };


        protected override void BindId(SqlCommand cmd, ItemShoppingCartKey id)
        {
            cmd.Parameters.Add("@Item_ID", SqlDbType.UniqueIdentifier).Value = id.ItemId;
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = id.CartId;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = 1; // Default quantity for new entries
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = 0; // Default price for new entries
        }

        protected override void BindInsert(SqlCommand cmd, ItemShoppingCart e)
        {
            cmd.Parameters.Add("@Item_ID", SqlDbType.UniqueIdentifier).Value = e.ItemID;
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = e.Quantity;
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = e.UnitPrice;
        }

        protected override void BindUpdate(SqlCommand cmd, ItemShoppingCart e)
            => throw new NotSupportedException("Update is not supported for this junction table.");

        protected override ItemShoppingCartKey GetKey(ItemShoppingCart e) => new ItemShoppingCartKey(e.ItemID, e.ShoppingCartID);

    }
}
