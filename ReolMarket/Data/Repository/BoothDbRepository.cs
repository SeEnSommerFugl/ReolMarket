using System.Data;
using System.Text;
using System.Transactions;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal sealed class BoothDbRepository : BaseDbRepository<Booth, Guid>
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

        protected override string SqlUpdateRange => @"
            UPDATE Booth
                SET IsRented = @IsRented,
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

        //protected override void BindUpdateRange(SqlCommand cmd, IEnumerable<Booth> booths)
        //{
        //    var boothList = booths.ToList();

        //    foreach (var booth in boothList)
        //    {
        //        cmd.Parameters.Add("@IsRented", SqlDbType.Bit).Value = booth.IsRented;
        //        cmd.Parameters.Add("@Status", SqlDbType.Int).Value = (int)booth.Status;
        //        cmd.Parameters.Add("@Customer_ID", SqlDbType.UniqueIdentifier)
        //           .Value = (object?)booth.CustomerID ?? DBNull.Value;
        //        cmd.Parameters.Add("@Booth_ID", SqlDbType.UniqueIdentifier).Value = booth.BoothID;
        //    }
        //}
        protected override void BindUpdateRange(SqlCommand cmd, IEnumerable<Booth> booths)
        {
            var boothList = booths.ToList();
            if (!boothList.Any()) return;

            // Clear any existing parameters
            cmd.Parameters.Clear();

            // Build dynamic SQL for multiple updates
            var sql = new StringBuilder();
            for (int i = 0; i < boothList.Count; i++)
            {
                if (i > 0) sql.AppendLine();

                sql.AppendLine($@"UPDATE Booth
                    SET IsRented = @IsRented{i},
                        Status = @Status{i},
                        Customer_ID = @Customer_ID{i}
                    WHERE Booth_ID = @Booth_ID{i};");

                var booth = boothList[i];
                cmd.Parameters.Add($"@IsRented{i}", SqlDbType.Bit).Value = booth.IsRented;
                cmd.Parameters.Add($"@Status{i}", SqlDbType.Int).Value = (int)booth.Status;
                cmd.Parameters.Add($"@Customer_ID{i}", SqlDbType.UniqueIdentifier)
                   .Value = (object?)booth.CustomerID ?? DBNull.Value;
                cmd.Parameters.Add($"@Booth_ID{i}", SqlDbType.UniqueIdentifier).Value = booth.BoothID;
            }

            cmd.CommandText = sql.ToString();
        }
    }
}
