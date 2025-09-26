using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class ItemDbRepository : BaseDbRepository<Item, Guid>
    {

        // Read all columns needed by the model
        protected override string SqlSelectAll => @"
            SELECT Item_ID, ItemName, ItemPrice, Booth_ID
            FROM Item";
        protected override string SqlSelectById => @"
            SELECT Item_ID, ItemName, ItemPrice, Booth_ID
            FROM Item
            WHERE Item_ID = @Item_ID";

        // Return the new identity as the first column so BaseDbRepository can capture it
        protected override string SqlInsert => @"
            INSERT INTO Item (Item_ID, ItemName, ItemPrice, Booth_ID)
            VALUES (@Item_ID, @ItemName, @ItemPrice, @Booth_ID);";

        protected override string SqlUpdate => @"
            UPDATE Item
                SET ItemName = @ItemName,
                ItemPrice = @ItemPrice,
                Booth_ID = @Booth_ID
            WHERE Item_ID = @Item_ID;";

        protected override string SqlDeleteById => @"
            DELETE FROM Item
            WHERE Item_ID = @Item_ID";

        protected override string SqlUpdateRange => throw new NotImplementedException();

        protected override Item Map(IDataRecord r) => new Item
        {
            ItemID = r.GetGuid(r.GetOrdinal("Item_ID")),
            ItemName = r.GetString(r.GetOrdinal("ItemName")),
            ItemPrice = r.GetDecimal(r.GetOrdinal("ItemPrice")),
            BoothID = r.GetGuid(r.GetOrdinal("Booth_ID"))
        };


        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@Item_ID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Item e)
        {
            cmd.Parameters.Add("@Item_ID", SqlDbType.UniqueIdentifier).Value = e.ItemID;
            cmd.Parameters.Add("@ItemName", SqlDbType.NVarChar, 255).Value = e.ItemName;
            var p = cmd.Parameters.Add("@ItemPrice", SqlDbType.Decimal);
            p.Precision = 10;
            p.Scale = 2;
            p.Value = e.ItemPrice;
            cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = e.BoothID;
        }

        protected override void BindUpdate(SqlCommand cmd, Item e)
        {
            cmd.Parameters.Add("@ItemName", SqlDbType.NVarChar, 255).Value = e.ItemName;
            var p = cmd.Parameters.Add("@ItemPrice", SqlDbType.Decimal);
            p.Precision = 10;
            p.Scale = 2;
            p.Value = e.ItemPrice;
            cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = e.BoothID;
            cmd.Parameters.Add("@Item_ID", SqlDbType.UniqueIdentifier).Value = e.ItemID;
        }

        protected override Guid GetKey(Item e) => e.ItemID;

        protected override void BindUpdateRange(SqlCommand cmd, IEnumerable<Item> entities)
        {
            throw new NotImplementedException();
        }

        //protected override void AssignGeneratedIdIfAny(Item e, object? id)
        //{
        //    if (id is Guid i) e.ItemID = i;
        //}
    }
}
