using System.Diagnostics;

namespace PAM;

public static class TracingExtensions
{
    public static ActivitySource Source = new("PAM");
}