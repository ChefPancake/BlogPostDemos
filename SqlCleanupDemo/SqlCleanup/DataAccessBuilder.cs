using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace SqlCleanup;

public class DataAccessBuilder {
    private IDictionary<Type, DataAccessRegistration> Registrations { get; }

    public DataAccessBuilder() {
        Registrations = new Dictionary<Type, DataAccessRegistration>();
    }

    public DataAccessBuilder Register<T>(
            Func<SqlCommand> getCommand,
            Func<IDataRecord, T> readRecord) =>
        Registrations.TryAdd(typeof(T), new DataAccessRegistration<T>(getCommand, readRecord).AsNongeneric())
            ? this
            : throw new RegistrationExistsException(typeof(T));

    public DataAccess Build(string databaseName, string serverName) =>
        new DataAccess(
            databaseName,
            serverName,
            new ReadOnlyDictionary<Type, DataAccessRegistration>(Registrations));
}
