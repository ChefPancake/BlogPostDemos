using System.Data;
using System.Data.SqlClient;

namespace SqlCleanup;

public class DataAccess {
    private IReadOnlyDictionary<Type, DataAccessRegistration> Registrations { get; }
    private string ConnectionString { get; }

    internal DataAccess(
            string databaseName,
            string serverName,
            IReadOnlyDictionary<Type, DataAccessRegistration> registrations) {
        ConnectionString =
            new SqlConnectionStringBuilder() {
                InitialCatalog = databaseName,
                DataSource = serverName,
                IntegratedSecurity = true
            }.ConnectionString;
        Registrations = registrations;
    }

    public IEnumerable<T> List<T>() {
        DataAccessRegistration<T> registration = GetRegistration<T>();
        return List<T>(registration.GetSqlCommand(), registration.ReadRecord);
    }

    private DataAccessRegistration<T> GetRegistration<T>() =>
        Registrations.TryGetValue(typeof(T), out DataAccessRegistration registration)
            ? registration.AsGeneric<T>()
            : throw new KeyNotFoundException($"Type {typeof(T).Name} not registered.");

    private SqlConnection OpenConnection() {
        SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    private IEnumerable<IDataRecord> EnumerateResults(SqlDataReader reader) {
        if (reader.HasRows) {
            while (reader.NextResult()) {
                yield return reader;
            }
        }
    }

    private IEnumerable<T> List<T>(
            SqlCommand command,
            Func<IDataRecord, T> transformRecord) {
        using SqlConnection conn = OpenConnection();
        using SqlCommand cmd = command.BindConnection(conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        foreach (var item in EnumerateResults(reader)) {
            yield return transformRecord(item);
        }
    }
}
