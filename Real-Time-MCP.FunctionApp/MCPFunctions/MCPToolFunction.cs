using CommonLogger;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;

namespace Real_Time_MCP.FunctionApp.MCPFunctions
{
    public class MCPToolFunction
    {
        private IAILogger _logger;

        public MCPToolFunction(IAILogger logger)
        {
            _logger = logger;
        }

        [Function("SummarizeTool")]
        public string SummarizeTool(
        [McpToolTrigger("summarize_text", "Summarizes the provided input text.")]
        ToolInvocationContext context,
        [McpToolProperty("text", "string", true)] string? text)
        {
            _logger.LogInformation("MCP Tool Function triggered.");
            return $"[Summary from Azure Function MCP Tool]: {text ?? "(no text provided)"}";
        }
    }
}
