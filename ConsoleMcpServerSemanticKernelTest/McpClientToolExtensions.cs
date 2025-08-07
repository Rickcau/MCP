using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleMcpServerSemanticKernelTest
{
    // Extension method to convert MCP tools to Kernel Functions
    public static class McpClientToolExtensions
    {
        public static KernelFunction AsKernelFunction(this McpClientTool tool)
        {
            return KernelFunctionFactory.CreateFromMethod(
                (string input) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(input))
                        {
                            var inputData = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
                            return tool.CallAsync(inputData as IReadOnlyDictionary<string, object?>).Result;
                        }
                        else
                        {
                            return tool.CallAsync().Result;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error calling MCP tool: {ex.Message}");
                        throw;
                    }
                },
                tool.Name,
                tool.Description);
        }
    }
}
