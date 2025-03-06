using Npgsql;



public class DatabaseClass
{
   private readonly string dbconnection = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";




    public DatabaseClass()
    {
        StartCheck();



    }        
    
    
    
    
    
    
    
    
    public void StartCheck()
        {

                try { 
                using (var connection = new NpgsqlConnection(dbconnection))
                {
                    connection.Open();
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Database is not Online: {ex.Message}");
                    throw;
                }
    }
}