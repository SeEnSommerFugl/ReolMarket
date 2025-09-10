using System.Data;
using Microsoft.Data.SqlClient;
using ReolMarket.MVVM.Model;

namespace ReolMarket.Data.Repository
{
    internal class BoothDbRepository : BaseDbRepository<Booth>
    {
        public BoothDbRepository() : base()
        {
        }
        // Read all columns needed by the model
        protected override string SqlSelectAll => @"
            SELECT Product_ID, ProductName, [Description], Price, StockStatus, PicturePath, Category_ID
            FROM Booth";
        protected override string SqlSelectById => @"
            SELECT Product_ID, ProductName, [Description], Price, StockStatus, PicturePath, Category_ID
            FROM Booth
            WHERE Product_ID = @Product_ID";

        // Return the new identity as the first column so BaseDbRepository can capture it
        protected override string SqlInsert => @"
            INSERT INTO Booth (ProductName, [Description], Price, StockStatus, PicturePath, Category_ID)
            OUTPUT INSERTED.Product_ID
            VALUES (@ProductName, @Description, @Price, @StockStatus, @PicturePath, @Category_ID);";

        protected override string SqlUpdate => @"
            UPDATE Booth
                SET ProductName = @ProductName,
                [Description] = @Description,
                Price = @Price,
                StockStatus = @StockStatus,
                PicturePath = @PicturePath,
                Category_ID = @Category_ID
            WHERE Product_ID = @Product_ID;";

        protected override string SqlDeleteById => @"
            DELETE FROM Booth
            WHERE Product_ID = @Product_ID";
        protected override Booth Map(IDataRecord r) => new Booth
        {
            ProductId = r.GetInt32(r.GetOrdinal("Product_ID")),
            ProductName = r.GetString(r.GetOrdinal("ProductName")),
            Description = r.IsDBNull(r.GetOrdinal("Description"))
                            ? string.Empty
                            : r.GetString(r.GetOrdinal("Description")),
            Price = r.GetDecimal(r.GetOrdinal("Price")),      // 👈 GetDecimal for DECIMAL
            StockStatus = r.GetInt32(r.GetOrdinal("StockStatus")),
            PicturePath = r.IsDBNull(r.GetOrdinal("PicturePath"))
                            ? string.Empty
                            : r.GetString(r.GetOrdinal("PicturePath")),
            CategoryId = r.GetInt32(r.GetOrdinal("Category_ID"))   // 👈 map FK
        };


        protected override void BindId(SqlCommand cmd, int id)
        {
            cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = id;
        }

        protected override void BindInsert(SqlCommand cmd, Booth e)
        {
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 255).Value = e.ProductName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 4000).Value = e.Description;
            cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = e.Price;
            cmd.Parameters["@Price"].Precision = 10;   // matches DECIMAL(10,2)
            cmd.Parameters["@Price"].Scale = 2;
            cmd.Parameters.Add("@StockStatus", SqlDbType.Int).Value = e.StockStatus;
            cmd.Parameters.Add("@PicturePath", SqlDbType.NVarChar, 500).Value = (object?)e.PicturePath ?? DBNull.Value;
            cmd.Parameters.Add("@Category_ID", SqlDbType.Int).Value = e.CategoryId;  // 👈
        }

        protected override void BindUpdate(SqlCommand cmd, Booth e)
        {
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 255).Value = e.ProductName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 4000).Value = e.Description;
            cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = e.Price;
            cmd.Parameters["@Price"].Precision = 10;
            cmd.Parameters["@Price"].Scale = 2;
            cmd.Parameters.Add("@StockStatus", SqlDbType.Int).Value = e.StockStatus;
            cmd.Parameters.Add("@PicturePath", SqlDbType.NVarChar, 500).Value = (object?)e.PicturePath ?? DBNull.Value;
            cmd.Parameters.Add("@Category_ID", SqlDbType.Int).Value = e.CategoryId;
            cmd.Parameters.Add("@Product_ID", SqlDbType.Int).Value = e.ProductId;
        }

        protected override int GetKey(Booth e) => e.ProductId;

        protected override void AssignGeneratedIdIfAny(Booth e, object? id)
        {
            if (id is int i) e.ProductId = i;
            else if (id is long l) e.ProductId = (int)l;
        }
    }
}
