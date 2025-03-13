# Unity MCP Server

## Overview
Unity MCP Server is an implementation of the Model Context Protocol for Unity Editor, allowing AI assistants to interact with your Unity projects. The server provides a standardized API that enables AI agents to query information about the Unity project and execute operations within the Unity Editor.

## Features
The MCP server provides the following tools:

- **get_logs**: Retrieves logs from the Unity Editor Console with filtering options
- **list_gameobjects**: Lists all GameObjects in open scenes with hierarchy information
- **list_scenes**: Lists all open scenes with information about the active scene
- **execute_menu_item**: Executes functions tagged with the MenuItem attribute

## Requirements
- Unity 2022.3 or later
- .NET Framework 4.7.1 or later

## Getting Started
1. The server starts automatically when the Unity Editor opens (this behavior can be configured)
2. Access the server settings through the Unity Editor menu: Tools > UnityMCP > Server Window
3. Use the Server Window to start/stop the server and configure settings

## Configuration
The following settings can be configured:
- Port number (default: 50325)
- Auto-start behavior
- Logging preferences
- Tool permissions

You can configure these settings through the Server Window (Tools > UnityMCP > Server Window) or programmatically:

```csharp
// Programmatic configuration
var settings = MCPSettings.Instance;
settings.Port = 8080;
settings.AutoStartServer = true;
settings.RequireConfirmationForExecution = true;
settings.LogCommandExecution = true;
settings.SaveSettings();
```

## Security
The MCP server includes security features to protect your Unity project:
- Optional confirmation dialog for executing menu items
- Logs of all commands executed through the server
- Ability to restrict which tools can be executed

## Usage Examples

### Creating a Custom MCP Tool

You can create your own MCP tools by adding methods with the `MCPTool` attribute:

```csharp
// Simple tool with no parameters
[MCPTool("my_custom_tool", "Description of what my tool does")]
public static MyResultType MyCustomTool()
{
    // Tool implementation
    return new MyResultType();
}

// Tool with simple parameter schema
[MCPTool("tool_with_parameters", "A tool that takes parameters",
    SchemaHelper.String("paramName", "Parameter description", true),
    SchemaHelper.Number("optionalParam", "An optional parameter"))]
public static ResultType ToolWithParameters(ParameterType parameters)
{
    // Implementation using parameters
    return new ResultType();
}

// Advanced tool with complex schema (if SchemaHelper isn't sufficient)
[MCPTool("advanced_tool", "An advanced tool with complex schema", @"{
    ""type"": ""object"",
    ""properties"": {
        ""complexParam"": {
            ""type"": ""object"",
            ""properties"": {
                ""nestedProp"": { ""type"": ""string"" }
            }
        }
    }
}")]
public static AdvancedResult AdvancedTool(AdvancedParams parameters)
{
    // Implementation
    return new AdvancedResult();
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request or open an Issue with your request.
**Commit your changes** following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) format

## License

MIT License

Copyright (c) 2023-2025 CoderGamester

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
