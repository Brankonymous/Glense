using Glense.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Identity.Client;

namespace InitDatabase
{
    // Initialize the database with the SQL script
    // 1. Drop the database if it exists.
    // 2. Create the database.
    // 3. Create the tables and relationships between them.
    // TODO: Insert test data from the SQL script.
    public static class DatabaseInitializer
    {
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

            // Assert that variables are not null or empty
            // Check if the windows or linux parameters are set
            if (string.IsNullOrEmpty(databaseName) || (string.IsNullOrEmpty(windowsServer) && (string.IsNullOrEmpty(linuxServer) || string.IsNullOrEmpty(linuxUser) || string.IsNullOrEmpty(linuxPassword))))
            {
                throw new InvalidOperationException("Database connection parameters are not set in the environment variables.");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows has it's own Authentication for SQL Server.
                connectionString = $"Server={windowsServer};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            }
            else
            {
                // MS SQL Server on Linux and MacOS is accessed through a docker container. This should be a default way of connecting to the SQL Server instance.
                connectionString = $"Server={linuxServer};Database={databaseName};User Id={linuxUser};Password={linuxPassword};Trusted_Connection=True;TrustServerCertificate=True;";
            }

            return Task.FromResult(connectionString);
        }

        // Initialize the database and create tables with the SQL script.
        public static async Task InitializeDatabaseAsync(IServiceProvider services, string connectionString)
        {
            try
            {
                using var scope = services.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Warning: No connection string found. Database initialization skipped.");
                    return;
                }

                // Read and execute the SQL script
                var sqlScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "glense.sql");
                if (File.Exists(sqlScriptPath))
                {
                    var sqlScript = await File.ReadAllTextAsync(sqlScriptPath);

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

                    Console.WriteLine("Database initialized successfully with SQL script.");
                }
                else
                {
                    Console.WriteLine($"Warning: SQL script not found at {sqlScriptPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't throw - allow the application to continue even if DB init fails
            }
        }

        // Helper method to properly split SQL script by GO statements
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
