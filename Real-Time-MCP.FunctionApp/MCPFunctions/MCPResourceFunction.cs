using CommonLogger;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;

namespace Real_Time_MCP.FunctionApp.MCPFunctions
{
    public class MCPResourceFunction
    {
        private readonly IAILogger _logger;

        public MCPResourceFunction(IAILogger logger)
        {
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────
        // RESOURCE 1: Plain-text resource
        // Trigger  : resources/read
        // URI      : info://app/readme
        // Try it   : curl --location 'http://localhost:7277/runtime/webhooks/mcp' \
        //            --header 'Content-Type: application/json' \
        //            --data '{"jsonrpc":"2.0","id":1,"method":"resources/read",
        //                     "params":{"uri":"info://app/readme"}}'
        // ─────────────────────────────────────────────────────────────────
        [Function("ReadReadmeResource")]
        public string ReadReadme(
            [McpResourceTrigger(
                "info://app/readme",
                "App Readme",
                MimeType = "text/plain",
                Description = "Returns a plain-text description of this MCP Function App.")]
            ResourceInvocationContext context)
        {
            _logger.LogInformation("MCP Resource triggered: info://app/readme");

            return """
                   Real-Time MCP Function App
                   ==========================
                   This Azure Function App exposes MCP primitives:
                     • Tools   → tools/call       (McpToolTrigger)
                     • Prompts → prompts/get      (McpPromptTrigger)
                     • Resources → resources/read (McpResourceTrigger)
                   """;
        }

        // ─────────────────────────────────────────────────────────────────
        // RESOURCE 2: JSON structured data resource
        // Trigger  : resources/read
        // URI      : data://app/config
        // Try it   : curl --location 'http://localhost:7277/runtime/webhooks/mcp' \
        //            --header 'Content-Type: application/json' \
        //            --data '{"jsonrpc":"2.0","id":2,"method":"resources/read",
        //                     "params":{"uri":"data://app/config"}}'
        // ─────────────────────────────────────────────────────────────────
        [Function("ReadConfigResource")]
        public string ReadConfig(
            [McpResourceTrigger(
                "data://app/config",
                "App Config",
                MimeType = "application/json",
                Description = "Returns the current app configuration as JSON.")]
            ResourceInvocationContext context)
        {
            _logger.LogInformation("MCP Resource triggered: data://app/config");

            // Real scenario: read from DB, Key Vault, or app settings
            var config = new
            {
                AppName    = "Real-Time MCP Function App",
                Version    = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development",
                Timestamp  = DateTime.UtcNow.ToString("o"),
                Features   = new[] { "McpTool", "McpPrompt", "McpResource" }
            };

            return System.Text.Json.JsonSerializer.Serialize(config,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }

        // ─────────────────────────────────────────────────────────────────
        // RESOURCE 3: Dynamic record lookup resource
        // Trigger  : resources/read
        // URI      : record://transactions/{id}   (URI passed via context.Arguments)
        // Try it   : curl --location 'http://localhost:7277/runtime/webhooks/mcp' \
        //            --header 'Content-Type: application/json' \
        //            --data '{"jsonrpc":"2.0","id":3,"method":"resources/read",
        //                     "params":{"uri":"record://transactions/TXN-001"}}'
        // ─────────────────────────────────────────────────────────────────
        [Function("ReadTransactionResource")]
        public string ReadTransaction(
            [McpResourceTrigger(
                "record://transactions/{id}",
                "Transaction Record",
                MimeType = "application/json",
                Description = "Returns transaction details for the given ID. URI: record://transactions/{id}")]
            ResourceInvocationContext context,
            string? id)  // {id} from URI template is auto-bound by the Functions runtime
        {
            // context.Uri holds the full requested URI, e.g. "record://transactions/TXN-001"
            _logger.LogInformation($"MCP Resource triggered: {context.Uri}");

            var resolvedId = id ?? "(unknown)";

            // Real scenario: query SQL / CosmosDB by resolvedId
            var record = new
            {
                TransactionId = resolvedId,
                Amount        = 1250.75m,
                Currency      = "USD",
                Status        = "Completed",
                CreatedAt     = DateTime.UtcNow.AddDays(-1).ToString("o"),
                Description   = $"Sample transaction for ID: {resolvedId}"
            };

            return System.Text.Json.JsonSerializer.Serialize(record,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
    }
}

