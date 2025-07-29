using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace InitDatabase
{
    // Initialize the database with the SQL script
    // 1. Drop the database if it exists.
    // 2. Create the database.
    // 3. Create the tables and relationships between them.
    // 4. Insert test data from the SQL script.
    public static class DatabaseInitializer
    {
        // Directory containing the SQL scripts.
        static string SQL_SCRIPTS_DIR = Path.Combine(Directory.GetCurrentDirectory(), "utils", "sql");
        
        // SQL scripts for the database initialization.
        static string SQL_INITIALIZE_SCRIPT_PATH = Path.Combine(SQL_SCRIPTS_DIR, "glense.sql");
        // SQL scripts for the database filling.
        static string SQL_FILL_SCRIPT_PATH = Path.Combine(SQL_SCRIPTS_DIR, "ingest_data.sql");
        
        // Get the connection string based on the environment, which is used to connect to the SQL Server instance.
        public static Task<string> getConnectionString()
        {
            string connectionString;

            // Read environment variables for database connection
            string? databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
            string? windowsServer = Environment.GetEnvironmentVariable("WINDOWS_SERVER");
            string? linuxServer = Environment.GetEnvironmentVariable("LINUX_SERVER");
            string? linuxUser = Environment.GetEnvironmentVariable("LINUX_USER");
            string? linuxPassword = Environment.GetEnvironmentVariable("LINUX_PASSWORD");

            // Assert that variables for either Linux/macOS or Windows are not null or empty.
            if (string.IsNullOrEmpty(databaseName) || (string.IsNullOrEmpty(windowsServer) && (string.IsNullOrEmpty(linuxServer) || string.IsNullOrEmpty(linuxUser) || string.IsNullOrEmpty(linuxPassword))))
            {
                throw new InvalidOperationException("Database connection parameters are not set in the environment variables.");
            }

            // On the first run, the database does not exist. We need to connect to the master database to create our Glense database.
            if (!string.IsNullOrEmpty(windowsServer) && !DatabaseExists(windowsServer, databaseName) && 
                !string.IsNullOrEmpty(linuxServer) && !string.IsNullOrEmpty(linuxUser) && !string.IsNullOrEmpty(linuxPassword) && !DatabaseExists(linuxServer, databaseName, linuxUser, linuxPassword)) 
            {
                databaseName = "master";
            }

            // TODO: Add login support through user and password in Windows.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows has it's own Authentication for SQL Server.
                connectionString = $"Server={windowsServer};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            }
            else
            {   
                // MS SQL Server on Linux and MacOS is accessed through a docker container. This should be a default way of connecting to the SQL Server instance.
                connectionString = $"Server={linuxServer};Database={databaseName};User Id={linuxUser};Password={linuxPassword};TrustServerCertificate=True;";
            }

            return Task.FromResult(connectionString);
        }

        // Check if the master database exists with trusted connection mode.
        static bool DatabaseExists(string server, string dbName)
        {
            try
            {
                string connStr = $"Server={server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
                string query = $"SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@dbName", dbName);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Check if the master database exists with user and password.
        static bool DatabaseExists(string server, string dbName, string username, string password)
        {
            try
            {
                string connStr = $"Server={server};Database=master;User Id={username};Password={password};TrustServerCertificate=True;";
                string query = $"SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@dbName", dbName);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Initialize the database and create tables with the SQL script.
        public static async Task InitializeDatabaseAsync(IServiceProvider services, string connectionString)
        {
            // Execute the SQL script to initialize the database.
            await ExecuteSqlScriptAsync(services, connectionString, SQL_INITIALIZE_SCRIPT_PATH, "Database initialized successfully with SQL script.");

            // Fill the database with test data.
            await ExecuteSqlScriptAsync(services, connectionString, SQL_FILL_SCRIPT_PATH, "Database filled successfully with SQL script.");
        }

        // Common method to execute SQL scripts.
        private static async Task ExecuteSqlScriptAsync(IServiceProvider services, string connectionString, string scriptPath, string successMessage)
        {
            try
            {
                using var scope = services.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No connection string found when executing SQL script: " + scriptPath);
                }

                // Read and execute the SQL script
                if (File.Exists(scriptPath))
                {
                    var sqlScript = await File.ReadAllTextAsync(scriptPath);

                    using var connection = new SqlConnection(connectionString);
                    await connection.OpenAsync();

                    // Split the script into batches using GO statements
                    var batches = SplitSqlScript(sqlScript);

                    foreach (var batch in batches)
                    {
                        var trimmedBatch = batch.Trim();
                        if (!string.IsNullOrEmpty(trimmedBatch))
                        {
                            try
                            {
                                using var sqlCommand = new SqlCommand(trimmedBatch, connection);
                                sqlCommand.CommandTimeout = 60; // 1 minute timeout per batch
                                await sqlCommand.ExecuteNonQueryAsync();
                            }
                            catch (Exception batchEx)
                            {
                                Console.WriteLine($"Error executing batch: {batchEx.Message}");
                                Console.WriteLine($"Batch content: {trimmedBatch}");
                                // Continue with next batch instead of failing completely
                            }
                        }
                    }

                    Console.WriteLine(successMessage);
                }
                else
                {
                    throw new Exception("SQL script not found: " + scriptPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing SQL script: " + ex.Message + "\n Stack trace: " + ex.StackTrace);
            }
        }

        // Helper method to properly split SQL script by GO statements.
        private static List<string> SplitSqlScript(string script)
        {
            var batches = new List<string>();
            var lines = script.Split('\n');
            var currentBatch = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Check if this line is a GO statement
                if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    // Add the current batch if it has content
                    var batch = currentBatch.ToString().Trim();
                    if (!string.IsNullOrEmpty(batch))
                    {
                        batches.Add(batch);
                    }
                    currentBatch.Clear();
                }
                else
                {
                    // Add the line to current batch (preserve original line ending)
                    currentBatch.AppendLine(line);
                }
            }

            // Add the last batch if it has content
            var lastBatch = currentBatch.ToString().Trim();
            if (!string.IsNullOrEmpty(lastBatch))
            {
                batches.Add(lastBatch);
            }

            return batches;
        }
    }
}
