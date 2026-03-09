using System.Text;
using Economy.Serializables;
using MySql.Data.MySqlClient;

namespace Economy.Util;

public class DatabaseService : IDisposable {
    private static readonly MySqlConnection Connection = new("server=172.17.0.1;uid=economy;password=economy123;database=economy");

    public DatabaseService() {
        Connection.Open();
    }

    public void CreateTableOrResetTable() {
        var cmd = new MySqlCommand(@"
            CREATE TABLE IF NOT EXISTS EconomicData (
                Id INTEGER PRIMARY KEY AUTO_INCREMENT,
                CountryName VARCHAR(1024) NOT NULL,
                Year DATETIME NOT NULL,
                GdpGrowth DOUBLE NOT NULL,
                Inflation DOUBLE NOT NULL
            );

            TRUNCATE TABLE EconomicData;
        ", Connection);
        cmd.ExecuteNonQuery();
    }

    public void InsertEconomicData(List<EconomicData> data) {
        var success = 0;
        
        try {
            foreach (var d in data) {
                using var cmd = new MySqlCommand(
                    "INSERT INTO EconomicData (CountryName, Year, GdpGrowth, Inflation) VALUES (@CountryName, @Year, @GdpGrowth, @Inflation)",
                    Connection
                );

                cmd.Parameters.AddWithValue($"@CountryName", d.CountryName);
                cmd.Parameters.AddWithValue($"@Year", d.Year);
                cmd.Parameters.AddWithValue($"@GdpGrowth", d.GdpGrowth);
                cmd.Parameters.AddWithValue($"@Inflation", d.Inflation);

                cmd.ExecuteNonQuery();
                success++;
            }
        } catch (MySqlException ex) {
            Console.WriteLine(ex.Message);
        }
        
        Console.WriteLine($"Az adatbázisba betöltöttem: {success} sort");
    }
    
    public void Dispose() {
        Connection.Dispose();
    }
}