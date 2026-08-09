using CommonLogger;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;

namespace Real_Time_MCP.FunctionApp.MCPFunctions
{
    public class MCPPromptFunction
    {
        private IAILogger _logger;

        public MCPPromptFunction(IAILogger logger)
        {
            _logger = logger;
        }

        [Function("generate")]
        public string Generate(
        [McpPromptTrigger(
            "generate",
            Title = "generate Text",
            Description = "Generates a prompt that summarizes the provided text.")]
        PromptInvocationContext context,
        [McpPromptArgument("text", "string")] string? text)
        {
            _logger.LogInformation("MCP Prompt Function triggered.");
            return $"Please provide a concise summary of the following text:\n\n{text ?? "(no text provided)"}";
        }
    }
}
