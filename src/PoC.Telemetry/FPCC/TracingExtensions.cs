using System.Diagnostics;

namespace FPCC;

public static class TracingExtensions
{
    public static readonly ActivitySource Source = new("FPCC");
}