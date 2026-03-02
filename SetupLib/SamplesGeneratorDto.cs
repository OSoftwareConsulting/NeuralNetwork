using System.Text.Json.Serialization;

namespace SetupLib;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(FileSamplesGeneratorDto), "File")]
[JsonDerivedType(typeof(FunctionSamplesGeneratorDto), "Function")]
public abstract class SamplesGeneratorDto
{
}
