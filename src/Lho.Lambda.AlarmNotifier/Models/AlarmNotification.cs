namespace Lho.Lambda.AlarmNotifier.Models;

public class AlarmNotification
{
    public string? AlarmName { get; init; }

    public string? AlarmDescription { get; init; }


    public string? NewStateValue { get; init; }

    public string? NewStateReason { get; init; }

    public string? StateChangeTime { get; init; }

    public string? Region { get; init; }

    public string? OldStateValue { get; init; }
}
