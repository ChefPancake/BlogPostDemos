using System.Data.SqlClient;

namespace SqlCleanup;

public class SqlFunctions_Bad {
    public static IEnumerable<Customer> ListCustomers(string databaseName, string serverName) {
        List<Customer> customers = new List<Customer>();
        string cmdText = "SELECT c.Id, c.FirstName, c.LastName FROM [dbo].[Customer] c";
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
            InitialCatalog = databaseName,
            DataSource = serverName,
            IntegratedSecurity = true
        };
        using SqlConnection conn = new SqlConnection(builder.ConnectionString);
        conn.Open();
        using SqlCommand cmd = new SqlCommand(cmdText, conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows) {
            while (reader.Read()) {
                int id = (int)reader[0];
                string firstName = (string)reader[1];
                string lastName = (string)reader[2];
                Customer customer = new Customer(id, firstName, lastName);
                customers.Add(customer);
            }
        }
        return customers;
    }

    public static IEnumerable<Product> ListProducts(string databaseName, string serverName) {
        List<Product> products = new List<Product>();
        string cmdText = "SELECT p.Id, p.[Name], p.Price FROM [dbo].[Products] p";
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
            InitialCatalog = databaseName,
            DataSource = serverName,
            IntegratedSecurity = true
        };
        using SqlConnection conn = new SqlConnection(builder.ConnectionString);
        conn.Open();
        using SqlCommand cmd = new SqlCommand(cmdText, conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows) {
            while (reader.Read()) {
                int id = (int)reader[0];
                string name = (string)reader[1];
                decimal price = (decimal)reader[2];
                Product product = new Product(id, name, price);
                products.Add(product);
            }
        }
        return products;
    }
}
