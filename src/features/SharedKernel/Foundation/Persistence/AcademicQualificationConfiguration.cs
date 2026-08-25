using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

public sealed class AcademicQualificationConfiguration : IEntityTypeConfiguration<AcademicQualification>
{
  public void Configure(EntityTypeBuilder<AcademicQualification> builder)
  {
    builder.ToTable("AcademicQualifications");

    builder.HasKey(x => new { x.EmpNr, x.DegreeCode });

    builder.Property(x => x.EmpNr)
      .HasMaxLength(SharedKernelFieldLengths.EmpNr)
      .IsRequired();

    builder.Property(x => x.DegreeCode)
      .HasMaxLength(SharedKernelFieldLengths.DegreeCode)
      .IsRequired();

    builder.Property(x => x.UniversityCode)
      .HasMaxLength(SharedKernelFieldLengths.UniversityCode)
      .IsRequired();
  }
}
