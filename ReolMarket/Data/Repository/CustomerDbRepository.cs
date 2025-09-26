using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class CustomerDbRepository : BaseDbRepository<Customer, Guid>
    {
        // Read all columns needed by the model
        protected override string SqlSelectAll => @"
            SELECT Customer_ID, CustomerName, Email, PhoneNumber, Address, PostalCode
            FROM Customer";
        protected override string SqlSelectById => @"
            SELECT Customer_ID, CustomerName, Email, PhoneNumber, Address, PostalCode
            FROM Customer
            WHERE Customer_ID = @Customer_ID";

        // Return the new identity as the first column so BaseDbRepository can capture it
        protected override string SqlInsert => @"
            INSERT INTO Customer (Customer_ID, CustomerName, Email, PhoneNumber, Address, PostalCode)
            VALUES (@Customer_ID, @CustomerName, @Email, @PhoneNumber, @Address, @PostalCode);";

        protected override string SqlUpdate => @"
            UPDATE Customer
                SET CustomerName = @CustomerName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                Address = @Address,
                PostalCode = @PostalCode
            WHERE Customer_ID = @Customer_ID;";

        protected override string SqlDeleteById => @"
            DELETE FROM Customer
            WHERE Customer_ID = @Customer_ID";

        protected override string SqlUpdateRange => throw new NotImplementedException();

        protected override Customer Map(IDataRecord r) => new Customer
        {
            CustomerID = r.GetGuid(r.GetOrdinal("Customer_ID")),
            CustomerName = r.GetString(r.GetOrdinal("CustomerName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            PhoneNumber = r.GetString(r.GetOrdinal("PhoneNumber")),
            Address = r.GetString(r.GetOrdinal("Address")),
            PostalCode = r.GetString(r.GetOrdinal("PostalCode"))
        };


        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Customer e)
        {
            cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier).Value = e.CustomerID;
            cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 255).Value = e.CustomerName;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = e.Email;
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = e.PhoneNumber;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = e.Address;
            cmd.Parameters.Add("@PostalCode", SqlDbType.Int).Value = e.PostalCode;
        }

        protected override void BindUpdate(SqlCommand cmd, Customer e)
        {
            cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 255).Value = e.CustomerName;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = e.Email;
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = e.PhoneNumber;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = e.Address;
            cmd.Parameters.Add("@PostalCode", SqlDbType.Int).Value = e.PostalCode;
            cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier).Value = e.CustomerID;
        }

        protected override Guid GetKey(Customer e) => e.CustomerID;

        protected override void BindUpdateRange(SqlCommand cmd, IEnumerable<Customer> entities)
        {
            throw new NotImplementedException();
        }

        //protected override void AssignGeneratedIdIfAny(Customer e, object? id)
        //{
        //    if (id is Guid i) e.CustomerID = i;
        //}
    }
}
