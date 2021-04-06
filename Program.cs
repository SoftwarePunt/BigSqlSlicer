using System;
using System.IO;
using System.Text;

namespace BigSqlSlicer
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.ExitCode = 1;

            // Hello world
            Console.WriteLine("[Big SQL Slicer Tool]");

            // Read params
            var targetPath = args.Length > 0 ? args[0] : null;
            var targetTable = args.Length > 1 ? args[1] : null;
            
            if (String.IsNullOrWhiteSpace(targetPath) || String.IsNullOrWhiteSpace(targetTable))
            {
                Console.WriteLine("Usage: bss [sqlFilePath] [tableName]");
                Environment.Exit(1);
                return;
            }

            var writePath = targetPath + ".SLICED";

            // Print params
            Console.WriteLine($"- Read path: {targetPath}");
            Console.WriteLine($"- Write path: {writePath}");
            Console.WriteLine($"- Target table: {targetTable}");

            // Begin reading
            var didFindTable = false;
            var readingRelevantBlock = false;

            Console.WriteLine("\r\nStarting read...\r\n");

            try
            {
                using (var writer = File.OpenWrite(writePath))
                {
                    using (var reader = File.OpenText(targetPath))
                    {
                        string line = String.Empty;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("DROP TABLE IF EXISTS") || line.Contains("CREATE TABLE") || line.Contains("INSERT INTO"))
                            {
                                if (line.Contains($"`{targetTable}`"))
                                {
                                    if (!readingRelevantBlock)
                                    {
                                        Console.WriteLine("\r\n\r\nFound table!\r\n");
                                        readingRelevantBlock = true;
                                        didFindTable = true;
                                    }
                                }
                                else
                                {
                                    if (readingRelevantBlock)
                                    {
                                        Console.WriteLine("\r\n\r\nEnd of table!\r\n");
                                        readingRelevantBlock = false;
                                    }
                                }
                            }

                            if (readingRelevantBlock)
                            {
                                writer.Write(Encoding.UTF8.GetBytes(line + Environment.NewLine));
                            }

                            Console.Write('.');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Read error!");
                Console.WriteLine(ex);
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }

            Console.WriteLine();

            if (!didFindTable)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Table was not found in SQL file!");
            }

            Environment.ExitCode = 0;
            Console.WriteLine();
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey(true);
        }
    }
}
