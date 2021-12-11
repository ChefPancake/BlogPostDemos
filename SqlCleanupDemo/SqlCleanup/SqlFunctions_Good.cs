using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlCleanup {
    public class SqlFunctions_Good {
        public static IEnumerable<Customer> ListCustomers(string databaseName, string serverName) =>
            List(databaseName,
                serverName,
                ListCustomersSqlCommand(),
                ReadCustomer);

        public static IEnumerable<Product> ListProducts(string databaseName, string serverName) =>
            List(databaseName,
                serverName,
                ListProductsSqlCommand(),
                ReadProduct);

        private static SqlConnection OpenConnection(string databaseName, string serverName) {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
                InitialCatalog = databaseName,
                DataSource = serverName,
                IntegratedSecurity = true
            };
            SqlConnection conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            return conn;
        }

        private static SqlCommand ListCustomersSqlCommand() =>
            new SqlCommand("SELECT c.Id, c.FirstName, c.LastName FROM [dbo].[Customer] c");

        private static SqlCommand ListProductsSqlCommand() =>
            new SqlCommand("SELECT p.Id, p.[Name], p.Price FROM [dbo].[Products] p");

        private static IEnumerable<IDataRecord> EnumerateResults(SqlDataReader reader) {
            if (reader.HasRows) {
                while (reader.NextResult()) {
                    yield return reader;
                }
            }
        }

        private static Customer ReadCustomer(IDataRecord record) {
            int id = (int)record[0];
            string firstName = (string)record[1];
            string lastName = (string)record[2];
            return new Customer(id, firstName, lastName);
        }

        private static Product ReadProduct(IDataRecord record) {
            int id = (int)record[0];
            string name = (string)record[1];
            decimal price = (decimal)record[2];
            return new Product(id, name, price);
        }

        private static IEnumerable<T> List<T>(
                string databaseName, 
                string serverName, 
                SqlCommand command, 
                Func<IDataRecord, T> transformRecord) {
            using SqlConnection conn = OpenConnection(databaseName, serverName);
            using SqlCommand cmd = command.BindConnection(conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            foreach (var item in EnumerateResults(reader)) {
                yield return transformRecord(item);
            }
        }
    }
}