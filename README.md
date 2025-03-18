# MCP Unity [![](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white 'LinkedIn')](https://www.linkedin.com/in/miguel-tomas/)

[![](https://badge.mcpx.dev?type=server 'MCP Server')](https://modelcontextprotocol.io/introduction)
[![smithery badge](https://smithery.ai/badge/@CoderGamester/mcp-unity)](https://smithery.ai/server/@CoderGamester/mcp-unity)
[![](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white 'Unity')](https://unity.com/releases/editor/archive)
[![](https://img.shields.io/badge/Node.js-339933?style=flat&logo=nodedotjs&logoColor=white 'Node.js')](https://nodejs.org/en/download/)
[![](https://img.shields.io/badge/TypeScript-3178C6?style=flat&logo=typescript&logoColor=white 'TypeScript')](https://www.typescriptlang.org/)
[![](https://img.shields.io/badge/WebSocket-4353FF?style=flat&logo=socket.io&logoColor=white 'WebSocket')](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API)

[![](https://img.shields.io/github/last-commit/CoderGamester/mcp-unity 'Last Commit')](https://github.com/CoderGamester/mcp-unity/commits/main)
[![](https://img.shields.io/github/stars/CoderGamester/mcp-unity 'Stars')](https://github.com/CoderGamester/mcp-unity/stargazers)
[![](https://img.shields.io/github/forks/CoderGamester/mcp-unity 'Forks')](https://github.com/CoderGamester/mcp-unity/network/members)
[![](https://img.shields.io/badge/License-MIT-red.svg 'MIT License')](https://opensource.org/licenses/MIT)

```                                                                        
                              ,/(/.   *(/,                                  
                          */(((((/.   *((((((*.                             
                     .*((((((((((/.   *((((((((((/.                         
                 ./((((((((((((((/    *((((((((((((((/,                     
             ,/(((((((((((((/*.           */(((((((((((((/*.                
            ,%%#((/((((((*                    ,/(((((/(#&@@(                
            ,%%##%%##((((((/*.             ,/((((/(#&@@@@@@(                
            ,%%######%%##((/(((/*.    .*/(((//(%@@@@@@@@@@@(                
            ,%%####%#(%%#%%##((/((((((((//#&@@@@@@&@@@@@@@@(                
            ,%%####%(    /#%#%%%##(//(#@@@@@@@%,   #@@@@@@@(                
            ,%%####%(        *#%###%@@@@@@(        #@@@@@@@(                
            ,%%####%(           #%#%@@@@,          #@@@@@@@(                
            ,%%##%%%(           #%#%@@@@,          #@@@@@@@(                
            ,%%%#*              #%#%@@@@,             *%@@@(                
            .,      ,/##*.      #%#%@@@@,     ./&@#*      *`                
                ,/#%#####%%#/,  #%#%@@@@, ,/&@@@@@@@@@&\.                    
                 `*#########%%%%###%@@@@@@@@@@@@@@@@@@&*´                   
                    `*%%###########%@@@@@@@@@@@@@@&*´                        
                        `*%%%######%@@@@@@@@@@&*´                            
                            `*#%%##%@@@@@&*´                                 
                               `*%#%@&*´                                     
                                                       
     ███╗   ███╗ ██████╗██████╗         ██╗   ██╗███╗   ██╗██╗████████╗██╗   ██╗
     ████╗ ████║██╔════╝██╔══██╗        ██║   ██║████╗  ██║██║╚══██╔══╝╚██╗ ██╔╝
     ██╔████╔██║██║     ██████╔╝        ██║   ██║██╔██╗ ██║██║   ██║    ╚████╔╝ 
     ██║╚██╔╝██║██║     ██╔═══╝         ██║   ██║██║╚██╗██║██║   ██║     ╚██╔╝  
     ██║ ╚═╝ ██║╚██████╗██║             ╚██████╔╝██║ ╚████║██║   ██║      ██║   
     ╚═╝     ╚═╝ ╚═════╝╚═╝              ╚═════╝ ╚═╝  ╚═══╝╚═╝   ╚═╝      ╚═╝   
```

MCP Unity is an implementation of the Model Context Protocol for Unity Editor, allowing AI assistants to interact with your Unity projects. This package provides a bridge between Unity and a Node.js server that implements the MCP protocol, enabling AI agents like Claude, Windsurf, and Cursor to execute operations within the Unity Editor.

## Features
MCP Unity currently provides the following tools:

- **execute_menu_item**: Executes Unity menu items (functions tagged with the MenuItem attribute)

More tools will be added in future updates.

## Requirements
- Unity 2022.3 or later
- Node.js 18 or later (for running the server)
- npm 9 or later (for building the server)

## Installation

### Installing the Unity MCP Server package via Unity Package Manager
1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/CoderGamester/mcp-unity.git`
5. Click "Add"

![package manager](https://github.com/user-attachments/assets/a72bfca4-ae52-48e7-a876-e99c701b0497)


### Installing Node.js 
To run MCP Unity server, you'll need to have Node.js 18 or later installed on your computer:

#### Windows
1. Visit the [Node.js download page](https://nodejs.org/en/download/)
2. Download the Windows Installer (.msi) for the LTS version (recommended)
3. Run the installer and follow the installation wizard
4. Verify the installation by opening PowerShell and running:
   ```bash
   node --version
   npm --version
   ```

#### macOS
1. Visit the [Node.js download page](https://nodejs.org/en/download/)
2. Download the macOS Installer (.pkg) for the LTS version (recommended)
3. Run the installer and follow the installation wizard
4. Alternatively, if you have Homebrew installed, you can run:
   ```bash
   brew install node@18
   ```
5. Verify the installation by opening Terminal and running:
   ```bash
   node --version
   npm --version
   ```

### Installing via Smithery

To install MCP Unity via [Smithery](https://smithery.ai/server/@CoderGamester/mcp-unity):

```
Currently not available
```

## Configure MCP Server

Replace `ABSOLUTE/PATH/TO` with the absolute path to your MCP Unity installation.

The right configuration can be accessed in the Unity Editor MCP Server window (Tools > MCP Unity > Server Window)

![MCP configuration](https://github.com/user-attachments/assets/ea9bb912-94a7-4409-81c1-3af39158dac0)


### Configure your AI client

To configure Cursor IDE:
- Add the following configuration to your Cursor MCP Configure settings:

```
Name: MCP Unity
Type: commmand
Command: env UNITY_PORT=8090 node ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js
```

To configure Claude Desktop:
- Open the MCP configuration file (claude_desktop_config.json) in Claude Desktop Developer in (File > Settings > Developer > Edit Config)

To configure Windsurf IDE:
- Open the MCP configuration file (mcp_config.json) in Windsurf IDE in (Windsurf Settings > Advanced Settings > General > Add Sever)

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE/PATH/TO/mcp-unity/Server/build/index.js"
      ],
      "env": {
        "UNITY_PORT": "8090"
      }
    }
  }
}
```

## Running the Server
You need to run the MCP Unity server for this to work, in the following ways:

### Start Node.js Server

1. Navigate to this `mcp-unity` package directory in your device with the terminal.
   ```bash
   cd ABSOLUTE/PATH/TO/mcp-unity
   ```
2. Run the server using Node.js:
   ```bash
   node Server/build/index.js
   ```

### Start Unity Editor MCP Server
1. Open the Unity Editor
2. Navigate to Tools > MCP Unity > Server Window
3. Click "Start Server" to start the WebSocket server
   
![connect](https://github.com/user-attachments/assets/2e266a8b-8ba3-4902-b585-b220b11ab9a2)

## Configure the WebSocket Port
By default, the WebSocket server runs on port 8080. You can change this port in two ways:

### Option 1: Using the Unity Editor
1. Open the Unity Editor
2. Navigate to Tools > MCP Unity > Server Window
3. Change the "WebSocket Port" value to your desired port number
4. Unity will setup the system environment variable UNITY_PORT to the new port number
5. Restart the Node.js server
6. Click again on "Start Server" to reconnect the Unity Editor web socket to the Node.js MCP Server

### Option 2: Change the system environment variable UNITY_PORT in the terminal
1. Set the UNITY_PORT environment variable in the terminal
   - Powershell
   ```powershell
   $env:UNITY_PORT = "8090"
   ```
   - Command Prompt/Terminal
   ```cmd
   set UNITY_PORT=8090
   ```
2. Restart the Node.js server
3. Click again on "Start Server" to reconnect the Unity Editor web socket to the Node.js MCP Server

## Building and Debugging the Server
The MCP Unity server is built using Node.js and TypeScript. It requires to compile the TypeScript code to JavaScript in the `build` directory.
To build the server, open a terminal and:

1. Navigate to the Server directory:
   ```bash
   cd ABSOLUTE/PATH/TO/mcp-unity/Server
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Build the server:
   ```bash
   npm run build
   ```

### Debugging
   
1. Debug the server with [@modelcontextprotocol/inspector](https://github.com/modelcontextprotocol/inspector):
   ```bash
   npx @modelcontextprotocol/inspector node build/index.js
   ```

2. Enable logging on your terminal or into a log.txt file:
   - Powershell
   ```powershell
   $env:LOGGING = "true"
   $env:LOGGING_FILE = "true"
   ```
   - Command Prompt/Terminal
   ```cmd
   set LOGGING=true
   set LOGGING_FILE=true
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

This project is under MIT license
