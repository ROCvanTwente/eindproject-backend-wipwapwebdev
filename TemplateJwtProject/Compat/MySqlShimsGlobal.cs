using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Global no-namespace shims to cover unqualified references in migration-generated code.
public static class MySqlModelBuilderExtensions
{
    public static void AutoIncrementColumns(ModelBuilder modelBuilder)
    {
        // no-op
    }
}

public static class MySqlPropertyBuilderExtensions
{
    public static PropertyBuilder<T> UseMySqlIdentityColumn<T>(PropertyBuilder<T> builder)
    {
        return builder;
    }
}
