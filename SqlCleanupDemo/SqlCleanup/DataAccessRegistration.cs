using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlCleanup {
    internal record DataAccessRegistration(
            Type RecordType,
            Func<SqlCommand> GetSqlCommand,
            Func<IDataRecord, object> ReadRecord) {
        public DataAccessRegistration<T> AsGeneric<T>() =>
            RecordType.GetType() is T
                 ? new DataAccessRegistration<T>(GetSqlCommand, x => (T)ReadRecord(x))
                 : throw new InvalidCastException($"Unable to cast {RecordType.GetType().Name} as {typeof(T).Name}");
    }

    internal record DataAccessRegistration<T>(
            Func<SqlCommand> GetSqlCommand,
            Func<IDataRecord, T> ReadRecord) {
        public Type RecordType => typeof(T);
        public DataAccessRegistration AsNongeneric() =>
            new DataAccessRegistration(
                    RecordType,
                    GetSqlCommand,
                    x => ReadRecord(x));
    }
}