using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class SaleDbRepository : BaseDbRepository<Sale, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT SaleID, ShoppingCartID, PaymentID
        FROM Sale";

        protected override string SqlSelectById => @"
        SELECT SaleID, ShoppingCartID, PaymentID
        FROM Sale
        WHERE SaleID = @SaleID";

        protected override string SqlInsert => @"
        INSERT INTO Sale (SaleID, ShoppingCartID, PaymentID)
        VALUES (@SaleID, @ShoppingCartID, @PaymentID);";

        protected override string SqlUpdate => @"
        UPDATE Sale
           SET ShoppingCartID = @ShoppingCartID,
               PaymentID      = @PaymentID
         WHERE SaleID         = @SaleID;";

        protected override string SqlDeleteById => @"
        DELETE FROM Sale
         WHERE SaleID = @SaleID;";

        protected override Sale Map(IDataRecord r) => new Sale
        {
            SaleID = r.GetGuid(r.GetOrdinal("SaleID")),
            ShoppingCartID = r.GetGuid(r.GetOrdinal("ShoppingCartID")),
            PaymentID = r.GetGuid(r.GetOrdinal("PaymentID"))
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@SaleID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Sale e)
        {
            cmd.Parameters.Add("@SaleID", SqlDbType.UniqueIdentifier).Value = e.SaleID;
            cmd.Parameters.Add("@ShoppingCartID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
        }

        protected override void BindUpdate(SqlCommand cmd, Sale e)
        {
            cmd.Parameters.Add("@ShoppingCartID", SqlDbType.UniqueIdentifier).Value = e.ShoppingCartID;
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
            cmd.Parameters.Add("@SaleID", SqlDbType.UniqueIdentifier).Value = e.SaleID;
        }

        protected override Guid GetKey(Sale e) => e.SaleID;
    }

}
