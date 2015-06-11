using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerError8621
{
    class Program
    {
        private const int MaxNumberOfForeignKeys = 1953;
        private static readonly string ConnectionString = "Data Source=.; Integrated Security=true; Initial Catalog=FooBar;";

        static void Main(string[] args)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // create teachers table.
                    connection.Execute("CREATE TABLE Teachers(Id INT NOT NULL PRIMARY KEY IDENTITY(1, 1), FName nvarchar(20) NOT NULL);", transaction);

                    for (int i = 0; i < MaxNumberOfForeignKeys; i++)
                    {
                        // creates tables like Info1, Info2, Info3 while all of them have reference to dbo.Teachers.Id
                        var relatedTableScript = String.Format(
                            @"CREATE TABLE Info{0}(Id INT NOT NULL PRIMARY KEY IDENTITY(1, 1), TeacherId INT NULL REFERENCES Teachers(Id));", i);
                        connection.Execute(relatedTableScript, transaction);

                        Console.WriteLine("Table Info" + i + " Created.");
                    }

                    connection.Execute("INSERT INTO Teachers(FName)VALUES(@FName);", transaction, new SqlParameter("@FName", "Jalal"));
                    connection.Execute("DELETE FROM Teachers WHERE Id = 1;", transaction);

                    transaction.Commit();
                    Console.WriteLine("OK!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error!");
                    Debug.WriteLine(ex);
                }
            }

            Console.ReadKey();
        }


    }

    public static class SqlConnectionExtensions
    {
        public static int Execute(this SqlConnection connection, string script, SqlTransaction transaction = null,
            params SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = script;
            command.CommandTimeout = 120;

            if (transaction != null)
                command.Transaction = transaction;

            if (parameters != null && parameters.Any())
                command.Parameters.AddRange(parameters);

            return command.ExecuteNonQuery();
        }

    }
}
