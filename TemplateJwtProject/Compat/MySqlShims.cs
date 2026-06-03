using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TemplateJwtProject.Migrations
{
    // Compatibility shims for leftover MySQL-generated migration code.
    // These no-op implementations allow the project to compile when using the SQL Server provider.

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
}
