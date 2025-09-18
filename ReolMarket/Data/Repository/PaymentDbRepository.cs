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
    internal sealed class PaymentDbRepository : BaseDbRepository<Payment, Guid>
    {
        protected override string SqlSelectAll => @"
        SELECT PaymentID, PaymentMethod, PaymentDate
        FROM Payment";

        protected override string SqlSelectById => @"
        SELECT PaymentID, PaymentMethod, PaymentDate
        FROM Payment
        WHERE PaymentID = @PaymentID";

        protected override string SqlInsert => @"
        INSERT INTO Payment (PaymentID, PaymentMethod, PaymentDate)
        VALUES (@PaymentID, @PaymentMethod, @PaymentDate);";

        protected override string SqlUpdate => @"
        UPDATE Payment
           SET PaymentMethod = @PaymentMethod,
               PaymentDate   = @PaymentDate
         WHERE PaymentID     = @PaymentID;";

        protected override string SqlDeleteById => @"
        DELETE FROM Payment
         WHERE PaymentID = @PaymentID;";

        protected override Payment Map(IDataRecord r) => new Payment
        {
            PaymentID = r.GetGuid(r.GetOrdinal("PaymentID")),
            PaymentMethod = r.GetString(r.GetOrdinal("PaymentMethod")),
            PaymentDate = r.GetDateTime(r.GetOrdinal("PaymentDate"))
        };

        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Payment e)
        {
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 255).Value = e.PaymentMethod ?? (object)DBNull.Value;
            cmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime2).Value = e.PaymentDate;
        }

        protected override void BindUpdate(SqlCommand cmd, Payment e)
        {
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 255).Value = e.PaymentMethod ?? (object)DBNull.Value;
            cmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime2).Value = e.PaymentDate;
            cmd.Parameters.Add("@PaymentID", SqlDbType.UniqueIdentifier).Value = e.PaymentID;
        }

        protected override Guid GetKey(Payment e) => e.PaymentID;
    }

}
