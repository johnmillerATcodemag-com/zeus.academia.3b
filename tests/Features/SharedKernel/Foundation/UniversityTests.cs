using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Tests.Features.SharedKernel.Foundation;

public sealed class UniversityTests
{
  [Fact]
  public void Create_NormalizesCodeAndExposesItAsIdentity()
  {
    var university = University.Create(" mit ");

    Assert.Equal("MIT", university.Code);
  }

  [Fact]
  public void Create_WithMissingCode_ThrowsArgumentExceptionForCode()
  {
    var exception = Assert.Throws<ArgumentException>(() => University.Create(" "));

    Assert.Equal("code", exception.ParamName);
  }

  [Fact]
  public void Create_WithCodeLongerThanConfiguredLimit_ThrowsArgumentException()
  {
    var code = new string('X', SharedKernelFieldLengths.UniversityCode + 1);

    var exception = Assert.Throws<ArgumentException>(() => University.Create(code));

    Assert.Equal("code", exception.ParamName);
  }

  [Fact]
  public void AcademicQualification_PersistsUniversityCodeRatherThanDisplayName()
  {
    var degree = Degree.Create("MCS");
    var university = University.Create("UCSD");

    var qualification = AcademicQualification.Create("EMP001", degree, university);

    Assert.Equal("UCSD", qualification.UniversityCode);
  }
}
