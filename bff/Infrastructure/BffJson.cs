using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transactions.Bff.Infrastructure;

public static class BffJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}