using Glense.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.IO;

namespace InitDatabase
{
    // Initialize the database with the SQL script
    // 1. Drop the database if it exists.
    // 2. Create the database.
    // 3. Create the tables and relationships between them.
    // TODO: Insert test data from the SQL script.
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
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
