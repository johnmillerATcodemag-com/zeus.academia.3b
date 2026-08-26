using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

internal sealed class ManageUniversitiesSqlServerTestDatabase : IAsyncDisposable
{
  private readonly string _connectionString;

  private ManageUniversitiesSqlServerTestDatabase(string connectionString)
  {
    _connectionString = connectionString;
  }

  public static async Task<ManageUniversitiesSqlServerTestDatabase> CreateAsync()
  {
    var connectionString = ResolveConnectionString();
    var builder = new SqlConnectionStringBuilder(connectionString)
    {
      InitialCatalog = $"ZeusTests_ManageUniversities_{Guid.NewGuid():N}"
    };

    var database = new ManageUniversitiesSqlServerTestDatabase(builder.ConnectionString);

    try
    {
      await using var context = database.CreateContext();
      await context.Database.MigrateAsync();
      return database;
    }
    catch
    {
      await database.DeleteBestEffortAsync();
      throw;
    }
  }

  public ManageUniversitiesDbContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseSqlServer(_connectionString)
      .Options;

    return new ManageUniversitiesDbContext(options);
  }

  public async ValueTask DisposeAsync()
  {
    await DeleteBestEffortAsync();
    GC.SuppressFinalize(this);
  }

  private async Task DeleteBestEffortAsync()
  {
    try
    {
      await using var context = CreateContext();
      await context.Database.EnsureDeletedAsync();
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine($"SQL Server test database cleanup failed: {exception.Message}");
    }
  }

  private static string ResolveConnectionString()
  {
    var connectionString = Environment.GetEnvironmentVariable("ZEUS_SQLSERVER_CONNECTION");

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
      return connectionString;
    }

    if (OperatingSystem.IsWindows())
    {
      return "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";
    }

    throw new InvalidOperationException(
      "ZEUS_SQLSERVER_CONNECTION is required on non-Windows hosts because SQL Server LocalDB is unavailable.");
  }
}
