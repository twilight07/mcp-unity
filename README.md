# MCP Unity

## Overview
MCP Unity is an implementation of the Model Context Protocol for Unity Editor, allowing AI assistants to interact with your Unity projects. This package provides a bridge between Unity and a Node.js server that implements the MCP protocol, enabling AI agents like Claude, Windsurf, and Cursor to execute operations within the Unity Editor.

## Features
MCP Unity currently provides the following tools:

- **execute_menu_item**: Executes Unity menu items (functions tagged with the MenuItem attribute)

More tools will be added in future updates.

## Requirements
- Unity 2021.3 or later
- Node.js 18 or later (for running the server)
- npm 9 or later (for building the server)

## Installation

### Via Unity Package Manager
1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/CoderGamester/mcp-unity.git`
5. Click "Add"

### Manual Installation
1. Clone the repository: `git clone https://github.com/CoderGamester/mcp-unity.git`
2. Copy the contents to your Unity project's Assets folder

## Running the Server
There are two ways to run the server:

### From Unity Editor
1. Open the Unity Editor
2. Navigate to Tools > MCP Unity > Server Window
3. Click "Start Server" to start the WebSocket server

### Standalone Mode
1. Navigate to the Server directory
2. Run the server using Node.js:
   ```
   node build/index.js
   ```

## Configuring AI Clients

Replace `ABSOLUTE\PATH\TO` with the actual path to your MCP Unity installation.

### Claude Desktop
Add the following configuration to your Claude Desktop settings:

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE\PATH\TO\mcp-unity\Server\build\index.js"
      ]
    }
  }
}
```

### Windsurf
Add the following configuration to your Windsurf MCP Configure settings:

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE\PATH\TO\mcp-unity\Server\build\index.js"
      ]
    }
  }
}
```

### Cursor
Add the following configuration to your Cursor MCP Configure settings:

- Name: MCP Unity
- Command: node
- Args: ABSOLUTE\PATH\TO\mcp-unity\Server\build\index.js

## Building the Server
The MCP Unity server is built using Node.js and TypeScript. To build the server:

1. Navigate to the Server directory:
   ```
   cd Server
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Build the server:
   ```
   npm run build
   ```

This will compile the TypeScript code to JavaScript in the `build` directory.

## Usage Examples

### Executing a Menu Item
AI assistants can execute Unity menu items using the `execute_menu_item` tool:

```typescript
// Example: Creating a new scene in Unity
await tools.execute_menu_item({
  menuPath: "File/New Scene"
});
```

## Troubleshooting

### Connection Issues
- Ensure the WebSocket server is running (check the Server Window in Unity)
- Check if there are any firewall restrictions blocking the connection

### Server Not Starting
- Check the Unity Console for error messages
- Ensure Node.js is properly installed and accessible in your PATH
- Verify that all dependencies are installed in the Server directory

### Menu Items Not Executing
- Ensure the menu item path is correct (case-sensitive)
- Check if the menu item requires confirmation
- Verify that the menu item is available in the current context

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request or open an Issue with your request.

**Commit your changes** following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) format.

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
