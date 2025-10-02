using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class SaleDbRepository : BaseDbRepository<Sale, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT Sale_ID, SaleDate, ShoppingCart_ID, Payment_ID, TotalPrice
        FROM Sale";

        protected override string SqlSelectById => @"
        SELECT Sale_ID, SaleDate, ShoppingCart_ID, Payment_ID, TotalPrice
        FROM Sale
        WHERE Sale_ID = @Sale_ID";

        protected override string SqlInsert => @"
        INSERT INTO Sale (Sale_ID, SaleDate, ShoppingCart_ID, Payment_ID, TotalPrice)
        VALUES (@Sale_ID, @SaleDate, @ShoppingCart_ID, @Payment_ID, @TotalPrice);";

        protected override string SqlUpdate => @"
        UPDATE Sale
           SET ShoppingCart_ID = @ShoppingCart_ID,
               Payment_ID      = @Payment_ID
               SaleDate        = @SaleDate
               TotalPrice      = @TotalPrice
         WHERE Sale_ID         = @Sale_ID;";

        protected override string SqlDeleteById => @"
        DELETE FROM Sale
         WHERE SaleID = @Sale_ID;";

        protected override Sale Map(IDataRecord r) => new Sale
        {
            SaleID = r.GetGuid(r.GetOrdinal("Sale_ID")),
            SaleDate = r.GetDateTime(r.GetOrdinal("SaleDate")),
            ShoppingCartID = r.GetGuid(r.GetOrdinal("ShoppingCart_ID")),
            PaymentID = r.GetGuid(r.GetOrdinal("Payment_ID")),
            TotalPrice = r.GetDecimal(r.GetOrdinal("TotalPrice"))
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@Sale_ID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Sale e)
        {
            cmd.Parameters.Add("@Sale_ID", SqlDbType.UniqueIdentifier).Value = e.SaleID;
            cmd.Parameters.Add(@"SaleDate", SqlDbType.DateTime2).Value = e.SaleDate;
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
            cmd.Parameters.Add("@Payment_ID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
            cmd.Parameters.Add("@TotalPrice", SqlDbType.Decimal).Value = e.TotalPrice;
        }

        protected override void BindUpdate(SqlCommand cmd, Sale e)
        {
            cmd.Parameters.Add("@ShoppingCart_ID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
            cmd.Parameters.Add("@SaleDate", SqlDbType.DateTime2).Value = e.SaleDate;
            cmd.Parameters.Add("@Payment_ID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
            cmd.Parameters.Add("@Sale_ID", SqlDbType.UniqueIdentifier).Value = e.SaleID;
            cmd.Parameters.Add("@TotalPrice", SqlDbType.Decimal).Value = e.TotalPrice;
        }

        protected override Guid GetKey(Sale e) => e.SaleID;

    }

}
