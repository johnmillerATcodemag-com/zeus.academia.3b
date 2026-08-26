using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared.Configurations;

public sealed class UniversityRecordConfiguration : IEntityTypeConfiguration<UniversityRecord>
{
  public void Configure(EntityTypeBuilder<UniversityRecord> builder)
  {
    var allowedSqlValues = string.Join(", ", UniversityCodeCatalog.SupportedUniversities.Select(x => $"'{x.Code}'"));

    builder.ToTable("Universities", tableBuilder =>
      tableBuilder.HasCheckConstraint("CK_Universities_Code_Allowed", $"[Code] IN ({allowedSqlValues})"));

    builder.HasKey(x => x.Code);

    builder.Property(x => x.Code)
      .HasMaxLength(SharedKernelFieldLengths.UniversityCode)
      .IsRequired();

    builder.Property(x => x.Name)
      .HasMaxLength(SharedKernelFieldLengths.UniversityName)
      .IsRequired();

    builder.Property(x => x.IsActive)
      .IsRequired();

    builder.HasIndex(x => x.IsActive)
      .HasDatabaseName("IX_Universities_IsActive");
  }
}
