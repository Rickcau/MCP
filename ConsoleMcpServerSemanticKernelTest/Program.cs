using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol.Client;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ConsoleMcpServerSemanticKernelTest;
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
string apimSubscriptionKey = configuration["PetStoreSettings:SubscriptionKey"] ?? throw new InvalidOperationException("Missing PetStoreSettings:SubscriptionKey configuration");
string azureOpenAiEndpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint configuration");
string azureOpenAiApiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing AzureOpenAI:ApiKey configuration");
string azureOpenAiDeploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Missing AzureOpenAI:DeploymentName configuration");

Console.WriteLine("=== Semantic Kernel + MCP PetStore Client ===");
Console.WriteLine();

try
{
    // Step 1: Create MCP Client
    Console.WriteLine("🔌 Connecting to MCP PetStore server...");
    var transportOptions = new SseClientTransportOptions
    {
        Endpoint = new Uri(petStoreMcpEndpoint),
        TransportMode = HttpTransportMode.StreamableHttp,
        ConnectionTimeout = TimeSpan.FromSeconds(30),
        // Add the APIM subscription key header
        AdditionalHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", apimSubscriptionKey }
        }
    };

    var mcpPetStoreClient = await McpClientFactory.CreateAsync(new SseClientTransport(transportOptions));
    Console.WriteLine("✅ Connected to MCP server successfully!");
    Console.WriteLine();

    // Step 2: Create Semantic Kernel with Azure OpenAI
    Console.WriteLine("🧠 Initializing Semantic Kernel with Azure OpenAI...");
    var kernelBuilder = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAiDeploymentName,
            endpoint: azureOpenAiEndpoint,
            apiKey: azureOpenAiApiKey);

    var kernel = kernelBuilder.Build();
    Console.WriteLine("✅ Semantic Kernel initialized!");
    Console.WriteLine();

    // Step 3: Get MCP tools and add them as plugins to Semantic Kernel
    Console.WriteLine("🛠️  Loading MCP tools as Semantic Kernel plugins...");
    var tools = await mcpPetStoreClient.ListToolsAsync();

    Console.WriteLine("Available MCP tools:");
    foreach (var tool in tools)
    {
        Console.WriteLine($"  - {tool.Name}: {tool.Description}");
    }

    // Convert MCP tools to Kernel functions and add to the Kernel
    kernel.Plugins.AddFromFunctions("MCP_PetStore", tools.Select(tool => tool.AsKernelFunction()));

    Console.WriteLine($"✅ Added {tools.Count} MCP tools as Semantic Kernel plugins!");
    Console.WriteLine();

    // Step 4: Use natural language to add a new pet
    Console.WriteLine("🐕 Using natural language to add a new pet...");

    // Static prompt for adding a pet
    string userPrompt = $$$"""
Please add a new pet to the store with the following details:
- Name: Buddycls
- Category: Dog (id: 1)
- Status: available

Use the appropriate tool to add this pet to the store.
The format of the request should be as follows:
    {
    "id": 0,
    "tags": [],
    "status": "",
    "photoUrls": [],
    "name": "",
    "category": {
        "id": 0,
        "name": ""
    }
    }
""";

    Console.WriteLine("User prompt:");
    Console.WriteLine(userPrompt);
    Console.WriteLine();

    // Step 5: Execute the prompt with Semantic Kernel
    Console.WriteLine("🤖 Processing request with AI...");

    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

    // Create execution settings that enable function calling
    var executionSettings = new OpenAIPromptExecutionSettings()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 0.1,
        MaxTokens = 1000
    };

    // Execute the prompt
    var result = await chatCompletionService.GetChatMessageContentAsync(
        userPrompt,
        executionSettings,
        kernel);

    Console.WriteLine("🎉 AI Response:");
    Console.WriteLine(result.Content);
    Console.WriteLine();

    // Step 6: Show the conversation history if there were function calls
    if (result.Metadata?.ContainsKey("Usage") == true)
    {
        Console.WriteLine("📊 Token Usage:");
        Console.WriteLine(result.Metadata["Usage"]);
        Console.WriteLine();
    }

    // Optional: Show all function calls that were made
    Console.WriteLine("🔧 Function Calls Made:");
    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(userPrompt);

    var response = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings,
        kernel);

    Console.WriteLine($"Response: {response.Content}");

    // Clean up
    await mcpPetStoreClient.DisposeAsync();
    Console.WriteLine("✅ MCP connection closed.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"Details: {ex}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();


