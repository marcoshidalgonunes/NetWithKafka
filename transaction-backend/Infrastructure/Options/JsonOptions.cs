using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transactions.Backend.Infrastructure.Options;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Serialization = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
