using System.Diagnostics;

namespace PAM;

public static class TracingExtensions
{
    public static readonly ActivitySource Source = new("PAM");
}