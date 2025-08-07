# Model Context Protocol (MCP) Sample Applications

This repository contains sample applications demonstrating the use of Model Context Protocol (MCP) with various integration patterns.

## Projects

### ConsoleMcpServerTest

A simple console application demonstrating basic MCP client functionality to connect to an MCP server and invoke tools.

### ConsoleMcpServerSemanticKernelTest

This project demonstrates the integration between Model Context Protocol (MCP) and Microsoft Semantic Kernel, allowing natural language to be used to interact with MCP tools.

## Getting Started with ConsoleMcpServerTest

### Prerequisites

- .NET 9.0 SDK or later
- Access to an MCP server endpoint

### Configuration

The application uses `appsettings.json` for configuration. This file is not included in the repository for security reasons. 

To set up your configuration:

1. Copy the `appsettings.example.json` file and rename it to `appsettings.json`
2. Update the settings with your actual values:

```json
{
  "PetStoreSettings": {
    "McpEndpoint": "https://your-mcp-endpoint-here/mcp",
    "SubscriptionKey": "your-subscription-key-here"
  }
}
```

- **PetStoreSettings**:
  - `McpEndpoint`: The URL of your MCP server endpoint
  - `SubscriptionKey`: Your API subscription key for the MCP service (if required)

### Running the Application

Navigate to the project directory and run:

```bash
dotnet run
```

The application will:
1. Connect to the MCP server
2. List available tools from the server

### Prerequisites

- .NET 9.0 SDK or later
- Access to an MCP server endpoint
- Access to Azure OpenAI service

### Configuration

The application uses `appsettings.json` for configuration. This file is not included in the repository for security reasons. 

To set up your configuration:

1. Copy the `appsettings.example.json` file and rename it to `appsettings.json`
2. Update the settings with your actual values:

```json
{
  "PetStoreSettings": {
    "McpEndpoint": "https://your-mcp-endpoint-here/mcp",
    "SubscriptionKey": "your-subscription-key-here"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-azure-openai-endpoint-here/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "your-deployment-name-here"
  }
}
```

- **PetStoreSettings**:
  - `McpEndpoint`: The URL of your MCP server endpoint
  - `SubscriptionKey`: Your API subscription key for the MCP service

- **AzureOpenAI**:
  - `Endpoint`: Your Azure OpenAI service endpoint
  - `ApiKey`: Your Azure OpenAI API key
  - `DeploymentName`: The name of your Azure OpenAI model deployment (e.g., "gpt-4o")

### Running the Application

Navigate to the project directory and run:

```bash
dotnet run
```

The application will:
1. Connect to the MCP server
2. Initialize Semantic Kernel with Azure OpenAI
3. Load MCP tools as Semantic Kernel plugins
4. Execute a natural language request to add a pet to the store

## Security Notes

- The `appsettings.json` file is included in the `.gitignore` to prevent committing sensitive information
- Always keep your API keys and subscription IDs private
- Consider using environment variables or Azure Key Vault for production deployments
