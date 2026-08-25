using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.Extensions.ProvisionExtension;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.Extensions.ProvisionExtension;

public sealed class ProvisionExtensionDbContextModelTests
{
   [Fact]
   public void Extensions_UseSharedKernelEntity_WithNumberPrimaryKey_AndFilteredAssignmentIndex()
   {
      using var context = CreateContext();
      var entityType = context.Model.FindEntityType(typeof(Extension));

      Assert.NotNull(entityType);

      var primaryKey = entityType!.FindPrimaryKey();
      Assert.NotNull(primaryKey);
      Assert.Single(primaryKey.Properties);
      Assert.Equal(nameof(Extension.Number), primaryKey.Properties[0].Name);

      var assignmentIndex = entityType.GetIndexes()
         .SingleOrDefault(index => index.Properties.Count == 1 && index.Properties[0].Name == nameof(Extension.AssignedEmpNr));

      Assert.NotNull(assignmentIndex);
      var filter = assignmentIndex!.GetFilter();
      Assert.True(assignmentIndex.IsUnique);
      Assert.False(string.IsNullOrWhiteSpace(filter));
      Assert.Contains("AssignedEmpNr", filter, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("IS NOT NULL", filter, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void GenerateCreateScript_ContainsExtensionsTable_AndFilteredAssignmentIndex()
   {
      using var context = CreateContext();
      var script = context.Database.GenerateCreateScript();

      Assert.Contains("[Extensions]", script, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("[Number]", script, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("IX_Extensions_AssignedEmpNr", script, StringComparison.OrdinalIgnoreCase);
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
