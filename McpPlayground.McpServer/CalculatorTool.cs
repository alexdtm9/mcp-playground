using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpPlayground.McpServer;

[McpServerToolType]
public static class CalculatorTool
{
    [McpServerTool, Description("Adds two numbers")]
    public static string Add(int a, int b) => $"Sum {a + b}";
}