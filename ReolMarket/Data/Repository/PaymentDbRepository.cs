using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class PaymentDbRepository : BaseDbRepository<Payment, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT PaymentID, PaymentMethod
        FROM Payment";

        protected override string SqlSelectById => @"
        SELECT PaymentID, PaymentMethod
        FROM Payment
        WHERE PaymentID = @PaymentID";

        protected override string SqlInsert => @"
        INSERT INTO Payment (PaymentID, PaymentMethod)
        VALUES (@PaymentID, @PaymentMethod);";

        protected override string SqlUpdate => @"
        UPDATE Payment
           SET PaymentMethod = @PaymentMethod
               PaymentDate   = @PaymentDate
         WHERE PaymentID     = @PaymentID;";

        protected override string SqlDeleteById => @"
        DELETE FROM Payment
         WHERE PaymentID = @PaymentID;";

        protected override Payment Map(IDataRecord r) => new Payment
        {
            PaymentID = r.GetGuid(r.GetOrdinal("PaymentID")),
            PaymentMethod = r.GetString(r.GetOrdinal("PaymentMethod"))
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Payment e)
        {
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 255).Value = e.PaymentMethod ?? (object)DBNull.Value;
        }

        protected override void BindUpdate(SqlCommand cmd, Payment e)
        {
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 255).Value = e.PaymentMethod ?? (object)DBNull.Value;
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
        }

        protected override Guid GetKey(Payment e) => e.PaymentID;
    }

}
