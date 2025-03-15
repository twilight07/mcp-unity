# MCP Unity

## Overview
MCP Unity is an implementation of the Model Context Protocol for Unity Editor, allowing AI assistants to interact with your Unity projects. This package provides a bridge between Unity and a Node.js server that implements the MCP protocol, enabling AI agents like Claude, Windsurf, and Cursor to execute operations within the Unity Editor.

## Features
MCP Unity currently provides the following tools:

- **execute_menu_item**: Executes Unity menu items (functions tagged with the MenuItem attribute)

More tools will be added in future updates.

## Usage Examples

### Executing a Menu Item
AI assistants can execute Unity menu items using the `execute_menu_item` tool:

```typescript
// Example: Creating a new scene in Unity
await tools.execute_menu_item({
  menuPath: "File/New Scene"
});
```

## Requirements
- Unity 2021.3 or later
- Node.js 18 or later (for running the server)
- npm 9 or later (for building the server)

## Installation

### Installing the Unity MCP Server package via Unity Package Manager
1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/CoderGamester/mcp-unity.git`
5. Click "Add"

### Installing Node.js 
Before installing MCP Unity, you'll need to have Node.js 18 or later installed on your computer to run the server:

#### Windows
1. Visit the [Node.js download page](https://nodejs.org/en/download/)
2. Download the Windows Installer (.msi) for the LTS version (recommended)
3. Run the installer and follow the installation wizard
4. Verify the installation by opening PowerShell and running:
   ```shell
   node --version
   npm --version
   ```

#### macOS
1. Visit the [Node.js download page](https://nodejs.org/en/download/)
2. Download the macOS Installer (.pkg) for the LTS version (recommended)
3. Run the installer and follow the installation wizard
4. Alternatively, if you have Homebrew installed, you can run:
   ```shell
   brew install node@18
   ```
5. Verify the installation by opening Terminal and running:
   ```shell
   node --version
   npm --version
   ```

## Running the Server
You need to run 2 servers for this MCP to work, in the following ways:

### Run Node.js Server
1. Navigate to this `mcp-unity` package directory in your device with the terminal.
   ```powershell
   cd ABSOLUTE/PATH/TO/mcp-unity
   ```
2. Run the server using Node.js:
   ```powershell
   node Server/build/index.js
   ```

### Run Unity Editor MCP Server
1. Open the Unity Editor
2. Navigate to Tools > MCP Unity > Server Window
3. Click "Start Server" to start the WebSocket server

### Configuring the WebSocket Port
By default, the WebSocket server runs on port 8080. You can change this port in two ways:

#### Option 1: Using the Unity Editor
1. Open the Unity Editor
2. Navigate to Tools > MCP Unity > Server Window
3. Change the "WebSocket Port" value to your desired port number
4. The Unity Editor MCP server will automatically reconnect with the new port
5. Restart the Node.js server

#### Option 2: Editing the port.txt file
1. Navigate to the root directory of the MCP Unity package
2. Open or create the `port.txt` file
3. Enter your desired port number (e.g., `8081`)
4. Save the file
5. Restart the Node.js server and Unity Editor MCP Server

## Configuring AI Clients

Replace `ABSOLUTE/PATH/TO` with the actual path to your MCP Unity installation.
The right configuration can be accessed in the Unity Editor MCP Server window. (Tools > MCP Unity > Server Window)

### Claude Desktop
Add the following configuration to your Claude Desktop Developer claude_desktop_config.json:

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js"
      ]
    }
  }
}
```

### Windsurf IDE
Add the following configuration to your Windsurf mcp_config.json settings:

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js"
      ]
    }
  }
}
```

### Cursor
Add the following configuration to your Cursor MCP Configure settings:

- Name: MCP Unity
- Command: node
- Args: ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js

## Building and Debugging the Server
The MCP Unity server is built using Node.js and TypeScript. It requires to compile the TypeScript code to JavaScript in the `build` directory.
To build the server, open a terminal and:

1. Navigate to the Server directory:
   ```shell
   cd ABSOLUTE/PATH/TO/mcp-unity/Server
   ```

2. Install dependencies:
   ```shell
   npm install
   ```

3. Build the server:
   ```shell
   npm run build
   ```
   
4. Debug the server with [@modelcontextprotocol/inspector](https://github.com/modelcontextprotocol/inspector):
   ```shell
   npx @modelcontextprotocol/inspector node build/index.js
   ```

Don't forget to shutdown the server with `Ctrl + C` before closing the terminal or debugging it with the [@modelcontextprotocol/inspector](https://github.com/modelcontextprotocol/inspector).

## Troubleshooting

### Connection Issues
- Ensure the WebSocket server is running (check the Server Window in Unity)
- Check if there are any firewall restrictions blocking the connection
- Make sure the port number is correct (default is 8080)
- Change the port number in the Unity Editor MCP Server window. (Tools > MCP Unity > Server Window)

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

Copyright (c) 2023-2025 Miguel Tomás

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
