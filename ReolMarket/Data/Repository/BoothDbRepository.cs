using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal class BoothDbRepository : BaseDbRepository<Booth, Guid>
    {
        // Read all columns needed by the model
        protected override string SqlSelectAll => @"
            SELECT Booth_ID, BoothNumber, NumberOfShelves, HasHangerBar, IsRented, Status, Customer_ID
            FROM Booth";
        protected override string SqlSelectById => @"
            SELECT Booth_ID, BoothNumber, NumberOfShelves, HasHangerBar, IsRented, Status, Customer_ID
            FROM Booth
            WHERE Booth_ID = @Booth_ID";

        // Return the new identity as the first column so BaseDbRepository can capture it
        protected override string SqlInsert => @"
            INSERT INTO Booth (Booth_ID, BoothNumber, NumberOfShelves, HasHangerBar, IsRented, Status, Customer_ID)
            VALUES (@Booth_ID, @BoothNumber, @NumberOfShelves, @HasHangerBar, @IsRented, @Status, @Customer_ID);";

        protected override string SqlUpdate => @"
            UPDATE Booth
                SET BoothNumber = @BoothNumber,
                NumberOfShelves = @NumberOfShelves,
                HasHangerBar = @HasHangerBar,
                IsRented = @IsRented,
                Status = @Status,
                Customer_ID = @Customer_ID
            WHERE Booth_ID = @Booth_ID;";

        protected override string SqlDeleteById => @"
            DELETE FROM Booth
            WHERE Booth_ID = @Booth_ID";
        protected override Booth Map(IDataRecord r) => new Booth
        {
            BoothID = r.GetGuid(r.GetOrdinal("Booth_ID")),
            BoothNumber = r.GetInt32(r.GetOrdinal("BoothNumber")),
            NumberOfShelves = r.GetInt32(r.GetOrdinal("NumberOfShelves")),
            HasHangerBar = r.GetBoolean(r.GetOrdinal("HasHangerBar")),
            IsRented = r.GetBoolean(r.GetOrdinal("IsRented")),
            Status = (BoothStatus)r.GetInt32(r.GetOrdinal("Status")),
            CustomerID = r.IsDBNull(r.GetOrdinal("Customer_ID"))
                            ? (Guid?)null
                            : r.GetGuid(r.GetOrdinal("Customer_ID"))// 👈 map FK
        };


        protected override void BindId(SqlCommand cmd, Guid id)
        {
            cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Booth e)
        {
            cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = e.BoothID;
            cmd.Parameters.Add("@BoothNumber", SqlDbType.Int).Value = e.BoothNumber;
            cmd.Parameters.Add("@NumberOfShelves", SqlDbType.Int).Value = e.NumberOfShelves;
            cmd.Parameters.Add("@HasHangerBar", SqlDbType.Bit).Value = e.HasHangerBar;
            cmd.Parameters.Add("@IsRented", SqlDbType.Bit).Value = e.IsRented;
            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = (int)e.Status;
            cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier)
               .Value = (object?)e.CustomerID ?? DBNull.Value;  // 👈 FK
        }

        protected override void BindUpdate(SqlCommand cmd, Booth e)
        {
            cmd.Parameters.Add("@BoothNumber", SqlDbType.Int).Value = e.BoothNumber;
            cmd.Parameters.Add("@NumberOfShelves", SqlDbType.Int).Value = e.NumberOfShelves;
            cmd.Parameters.Add("@HasHangerBar", SqlDbType.Bit).Value = e.HasHangerBar;
            cmd.Parameters.Add("@IsRented", SqlDbType.Bit).Value = e.IsRented;
            cmd.Parameters.Add("@Status", SqlDbType.Int).Value = (int)e.Status;
            cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier)
               .Value = (object?)e.CustomerID ?? DBNull.Value; // 👈 FK
            cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = e.BoothID;
        }

        protected override Guid GetKey(Booth e) => e.BoothID;

        //protected override void AssignGeneratedIdIfAny(Booth e, object? id)
        //{
        //    if (id is Guid i) e.BoothID = i;
        //}
    }
}
