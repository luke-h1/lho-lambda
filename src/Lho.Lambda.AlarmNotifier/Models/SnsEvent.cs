namespace Lho.Lambda.AlarmNotifier.Models;

public class SnsEvent
{
    public List<SnsEventRecord>? Records { get; init; }
}

public class SnsEventRecord
{
    public SnsMessage? Sns { get; init; }
}

public class SnsMessage
{
    public string? Message { get; init; }
}
