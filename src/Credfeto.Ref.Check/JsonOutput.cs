using System.Text.Json;

namespace Credfeto.Ref.Check;

public static class JsonOutput
{
    public static string Serialise(AnalysisResult result)
    {
        return JsonSerializer.Serialize(result, RefCheckJsonContext.Default.AnalysisResult);
    }
}
