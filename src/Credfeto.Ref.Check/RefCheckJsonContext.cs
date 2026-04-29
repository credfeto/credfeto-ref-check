using System.Text.Json.Serialization;

namespace Credfeto.Ref.Check;

[JsonSerializable(typeof(AnalysisResult))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true)]
internal sealed partial class RefCheckJsonContext : JsonSerializerContext
{
}
