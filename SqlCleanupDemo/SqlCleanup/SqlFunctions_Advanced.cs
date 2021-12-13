using System.Data;
using System.Data.SqlClient;

namespace SqlCleanup;

public class SqlFunctions_Advanced {
    private DataAccess Access { get; }
    public SqlFunctions_Advanced(string databaseName, string serverName) =>
        Access = BuildOrdersDataAccess(databaseName, serverName);

    private static DataAccess BuildOrdersDataAccess(string databaseName, string serverName) =>
        new DataAccessBuilder()
        .Register(
            ListCustomersSqlCommand,
            ReadCustomer)
        .Register(
            ListProductsSqlCommand,
            ReadProduct)
        .Build(databaseName, serverName);

    public IEnumerable<T> List<T>() =>
        Access.List<T>();

    private static SqlCommand ListCustomersSqlCommand() =>
        new SqlCommand("SELECT c.Id, c.FirstName, c.LastName FROM [dbo].[Customer] c");

    private static SqlCommand ListProductsSqlCommand() =>
        new SqlCommand("SELECT p.Id, p.[Name], p.Price FROM [dbo].[Products] p");

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
}
