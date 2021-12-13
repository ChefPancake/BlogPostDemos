using System.Data.SqlClient;

namespace SqlCleanup;

internal static class CommandExtensions {
    public static SqlCommand BindConnection(this SqlCommand cmd, SqlConnection conn) {
        cmd.Connection = conn;
        return cmd;
    }
}
