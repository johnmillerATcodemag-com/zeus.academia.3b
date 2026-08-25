using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionDbContextModelTests
{
    [Fact]
    public void Extensions_HasPrimaryKeyOnNumber_AndNoDuplicateUniqueIndex()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Zeus.Academia.Features.SharedKernel.Foundation.Domain.Extension));

        Assert.NotNull(entityType);

        var primaryKey = entityType!.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey!.Properties);
        Assert.Equal("Number", primaryKey.Properties[0].Name);

        var duplicatePkUniqueIndex = entityType.GetIndexes().Any(index =>
            index.IsUnique &&
            index.Properties.Count == primaryKey.Properties.Count &&
            index.Properties.Select(p => p.Name).SequenceEqual(primaryKey.Properties.Select(p => p.Name)));

        Assert.False(duplicatePkUniqueIndex);
    }

    [Fact]
    public void Extensions_HasFilteredUniqueIndexOnAssignedEmpNr()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Zeus.Academia.Features.SharedKernel.Foundation.Domain.Extension));

        Assert.NotNull(entityType);
        Assert.Contains(entityType!.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Count == 1 &&
            index.Properties[0].Name == "AssignedEmpNr" &&
            index.GetFilter() is not null && index.GetFilter()!.Contains("AssignedEmpNr", StringComparison.OrdinalIgnoreCase));
    }

    private static ProvisionExtensionDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (OperatingSystem.IsWindows())
            {
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ZeusAcademiaProvisionExtensionDesign;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            }
            else
            {
                throw new InvalidOperationException("ZEUS_SQLSERVER_CONNECTION is required on non-Windows hosts because SQL Server LocalDB is unavailable.");
            }
        }

        var options = new DbContextOptionsBuilder<ProvisionExtensionDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ProvisionExtensionDbContext(options);
    }
}
