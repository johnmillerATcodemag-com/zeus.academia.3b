using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

public sealed class ExtensionConfiguration : IEntityTypeConfiguration<Extension>
{
  public void Configure(EntityTypeBuilder<Extension> builder)
  {
    builder.ToTable("Extensions");

    builder.HasKey(x => x.Number);

    builder.Property(x => x.Number)
      .ValueGeneratedNever();

    builder.Property(x => x.AssignedEmpNr)
      .HasMaxLength(SharedKernelFieldLengths.EmpNr);

    builder.HasIndex(x => x.AssignedEmpNr)
      .IsUnique()
      .HasFilter("[AssignedEmpNr] IS NOT NULL");
  }
}
