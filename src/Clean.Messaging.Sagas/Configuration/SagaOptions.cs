using System.Text.Json;

namespace Clean.Messaging.Sagas.Configuration;

public sealed class SagaOptions
{
    public const string Section = "Messaging:Sagas";

    public JsonSerializerOptions SerializerOptions { get; set; } =
        new(JsonSerializerDefaults.Web);

    public TimeSpan Retention { get; set; } =
        TimeSpan.FromDays(30);

    public TimeSpan CleanupInterval { get; set; } =
        TimeSpan.FromHours(1);

    public int CleanupBatchSize { get; set; } = 500;
}