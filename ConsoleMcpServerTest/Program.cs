// using Azure.AI.OpenAI;
// using Azure.Identity;
// using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;
using System.IO;

// Load configuration from appsettings.json
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables(); // Allow environment variable overrides

// Check if appsettings.json exists
string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
if (File.Exists(settingsPath))
{
    configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
}
else
{
    // Check for example settings file and warn the user
    string exampleSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.example.json");
    if (File.Exists(exampleSettingsPath))
    {
        Console.WriteLine("⚠️ Warning: appsettings.json not found. Please copy appsettings.example.json to appsettings.json and update with your settings.");
        Console.WriteLine("Attempting to continue with environment variables if available...\n");
    }
    else
    {
        Console.WriteLine("⚠️ Error: No configuration files found. Please create an appsettings.json file based on the example in the README.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        return;
    }
}

IConfiguration configuration = configBuilder.Build();

// Get configuration values
string petStoreMcpEndpoint = configuration["PetStoreSettings:McpEndpoint"] ?? throw new InvalidOperationException("Missing PetStoreSettings:McpEndpoint configuration");
string? subscriptionKey = configuration["PetStoreSettings:SubscriptionKey"];



// Create an IChatClient using Azure OpenAI.
// IChatClient client =
//     new ChatClientBuilder(
//         new AzureOpenAIClient(new Uri("<your-azure-openai-endpoint>"),
//         new DefaultAzureCredential())
//         .GetChatClient("gpt-4o").AsIChatClient())
//     .UseFunctionInvocation()
//     .Build();

// Create the MCP client
// Configure it to start and connect to your MCP server.
// IMcpClient mcpClient = await McpClientFactory.CreateAsync(
//     new StdioClientTransport(new()
//     {
//         Command = "dotnet run",
//         Arguments = ["--project", "<path-to-your-mcp-server-project>"],
//         Name = "Minimal MCP Server",
//     }));

var transportOptions = new SseClientTransportOptions
{
    Endpoint = new Uri(petStoreMcpEndpoint),
    TransportMode = HttpTransportMode.StreamableHttp,
    ConnectionTimeout = TimeSpan.FromSeconds(30)
};

// Add subscription key header if available
if (!string.IsNullOrEmpty(subscriptionKey))
{
    transportOptions.AdditionalHeaders = new Dictionary<string, string>
    {
        { "Ocp-Apim-Subscription-Key", subscriptionKey }
    };
}

var mcpPetStoreClient = await McpClientFactory.CreateAsync(new SseClientTransport(transportOptions));

// List all available tools from the MCP server.
Console.WriteLine("Available tools:");
IList<McpClientTool> tools = await mcpPetStoreClient.ListToolsAsync();
foreach (McpClientTool tool in tools)
{
    Console.WriteLine($"{tool}");
}
Console.WriteLine();

// Conversational loop that can utilize the tools via prompts.
// List<ChatMessage> messages = [];
// while (true)
// {
//     Console.Write("Prompt: ");
//     messages.Add(new(ChatRole.User, Console.ReadLine()));

//     List<ChatResponseUpdate> updates = [];
//     await foreach (ChatResponseUpdate update in client
//         .GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
//     {
//         Console.Write(update);
//         updates.Add(update);
//     }
//     Console.WriteLine();

//     messages.AddMessages(updates);
// }