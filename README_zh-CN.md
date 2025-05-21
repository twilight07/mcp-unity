# MCP Unity Editor（游戏引擎）

[![](https://badge.mcpx.dev?status=on 'MCP 启用')](https://modelcontextprotocol.io/introduction)
[![](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white 'Unity')](https://unity.com/releases/editor/archive)
[![](https://img.shields.io/badge/Node.js-339933?style=flat&logo=nodedotjs&logoColor=white 'Node.js')](https://nodejs.org/en/download/)
[![](https://img.shields.io/github/stars/CoderGamester/mcp-unity 'Stars')](https://github.com/CoderGamester/mcp-unity/stargazers)
[![](https://img.shields.io/github/last-commit/CoderGamester/mcp-unity 'Last Commit')](https://github.com/CoderGamester/mcp-unity/commits/main)
[![](https://img.shields.io/badge/License-MIT-red.svg 'MIT 许可证')](https://opensource.org/licenses/MIT)

| [英文](README.md) | [🇨🇳简体中文](README_zh-CN.md) | [🇯🇵日本語](README-ja.md) |
|----------------------|---------------------------------|----------------------|


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

MCP Unity 是 Model Context Protocol 在 Unity 编辑器中的实现，允许 AI 助手与您的 Unity 项目交互。这个包提供了 Unity 和实现 MCP 协议的 Node.js 服务器之间的桥梁，使 Claude、Windsurf 和 Cursor 等 AI 代理能够在 Unity 编辑器中执行操作。

<a href="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity">
  <img width="400" height="200" src="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity/badge" alt="Unity MCP 服务器" />
</a>

## 功能

### IDE 集成 - 包缓存访问

MCP Unity 通过将 Unity `Library/PackedCache` 文件夹添加到您的工作区，提供与 VSCode 类 IDE（Visual Studio Code、Cursor、Windsurf）的自动集成。此功能：

- 提高对 Unity 包的代码智能感知
- 为 Unity 包提供更好的自动完成和类型信息
- 帮助 AI 编码助手理解您项目的依赖关系

### MCP 服务器工具

- `execute_menu_item`: 执行 Unity 菜单项（用 MenuItem 属性标记的函数）
  > **示例提示:** "执行菜单项 'GameObject/Create Empty' 创建一个新的空 GameObject"

- `select_gameobject`: 通过路径或实例 ID 选择 Unity 层次结构中的游戏对象
  > **示例提示:** "选择场景中的 Main Camera 对象"

- `update_gameobject`: 更新 GameObject 的核心属性（名称、标签、层、激活/静态状态），如果不存在则创建
  > **示例提示:** "将 Player 对象的标签设置为 ‘Enemy’ 并使其不可用"

- `update_component`: 更新 GameObject 上的组件字段，如果 GameObject 不包含该组件则添加它
  > **示例提示:** "给 Player 对象添加 Rigidbody 组件并设置其质量为 5"

- `add_package`: 在 Unity 包管理器中安装新包
  > **示例提示:** "给我的项目添加 TextMeshPro 包"

- `run_tests`: 使用 Unity 测试运行器运行测试
  > **示例提示:** "运行我项目中所有的 EditMode 测试"

- `send_console_log`: 发送控制台日志到 Unity
  > **示例提示:** "发送控制台日志到 Unity 编辑器"

- `add_asset_to_scene`: 将 AssetDatabase 中的资源添加到 Unity 场景中
  > **示例提示:** "将我的项目中的 Player 预制体添加到当前场景"

### MCP 服务器资源

- `unity://menu-items`: 获取 Unity 编辑器中所有可用的菜单项列表，以方便 `execute_menu_item` 工具
  > **示例提示:** "显示与 GameObject 创建相关的所有可用菜单项"

- `unity://scenes-hierarchy`: 获取当前 Unity 场景层次结构中所有游戏对象的列表
  > **示例提示:** "显示当前场景层次结构"

- `unity://gameobject/{id}`: 通过实例 ID 或场景层次结构中的对象路径获取特定 GameObject 的详细信息，包括所有 GameObject 组件及其序列化的属性和字段
  > **示例提示:** "获取 Player GameObject 的详细信息"

- `unity://logs`: 获取 Unity 控制台的所有日志列表
  > **示例提示:** "显示 Unity 控制台最近的错误信息"

- `unity://packages`: 从 Unity 包管理器获取已安装和可用包的信息
  > **示例提示:** "列出我 Unity 项目中当前安装的所有包"

- `unity://assets`: 获取 Unity 资产数据库中资产的信息
  > **示例提示:** "查找我项目中的所有纹理资产"

- `unity://tests/{testMode}`: 获取 Unity 测试运行器中测试的信息
  > **示例提示:** "列出我 Unity 项目中所有可用的测试"

## 要求
- Unity 2022.3 或更高版本 - 用于[安装服务器](#install-server)
- Node.js 18 或更高版本 - 用于[启动服务器](#start-server)
- npm 9 或更高版本 - 用于[调试服务器](#debug-server)

## <a name="install-server"></a>安装

安装 MCP Unity 服务器是一个多步骤过程：

### 步骤 1: 通过 Unity 包管理器安装 Unity MCP 服务器包
1. 打开 Unity 包管理器 (Window > Package Manager)
2. 点击左上角的 "+" 按钮
3. 选择 "Add package from git URL..."
4. 输入: `https://github.com/CoderGamester/mcp-unity.git`
5. 点击 "Add"

![package manager](https://github.com/user-attachments/assets/a72bfca4-ae52-48e7-a876-e99c701b0497)


### 步骤 2: 安装 Node.js 
> 要运行 MCP Unity 服务器，您需要在计算机上安装 Node.js 18 或更高版本：

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Windows</span></summary>

1. 访问 [Node.js 下载页面](https://nodejs.org/en/download/)
2. 下载 Windows 安装程序 (.msi) 的 LTS 版本（推荐）
3. 运行安装程序并按照安装向导操作
4. 通过打开 PowerShell 并运行以下命令验证安装：
   ```bash
   node --version
   ```
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">macOS</span></summary>

1. 访问 [Node.js 下载页面](https://nodejs.org/en/download/)
2. 下载 macOS 安装程序 (.pkg) 的 LTS 版本（推荐）
3. 运行安装程序并按照安装向导操作
4. 或者，如果您已安装 Homebrew，可以运行：
   ```bash
   brew install node@18
   ```
5. 通过打开终端并运行以下命令验证安装：
   ```bash
   node --version
   ```
</details>

### 步骤 3: 配置 AI LLM 客户端

<details open>
<summary><span style="font-size: 1.1em; font-weight: bold;">选项 1: 使用 Unity 编辑器配置</span></summary>

1. 打开 Unity 编辑器
2. 导航到 Tools > MCP Unity > Server Window
3. 点击 "Configure" 按钮为您的 AI LLM 客户端配置，如下图所示

![image](https://github.com/user-attachments/assets/8d286e83-da60-40fa-bd6c-5de9a77c1820)

4. 使用给定的弹出窗口确认配置安装

![image](https://github.com/user-attachments/assets/b1f05d33-3694-4256-a57b-8556005021ba)

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">选项 3: 手动配置</span></summary>

打开您的 AI 客户端的 MCP 配置文件（例如 Claude Desktop 中的 claude_desktop_config.json）并复制以下文本：

> 将 `ABSOLUTE/PATH/TO` 替换为您的 MCP Unity 安装的绝对路径，或者直接从 Unity 编辑器 MCP 服务器窗口（Tools > MCP Unity > Server Window）复制文本。

```json
{
  "mcpServers": {
    "mcp-unity": {
      "command": "node",
      "args": [
        "ABSOLUTE/PATH/TO/mcp-unity/Server~/build/index.js"
      ]
    }
  }
}
```

</details>

## <a name="start-server"></a>启动服务器

启动 MCP Unity 服务器有两种方式：

### 选项 1: 通过 Unity 编辑器启动
1. 打开 Unity 编辑器
2. 导航到 Tools > MCP Unity > Server Window
3. 点击 "Start Server" 按钮

### 选项 2: 通过命令行启动
1. 打开终端或命令提示符
2. 导航到 MCP Unity 服务器目录
3. 运行以下命令：
   ```bash
   node Server~/build/index.js
   ```

## 可选：设置超时

默认情况下，MCP 服务器与 WebSocket 之间的超时时间为 10 秒。
您可以根据所使用的操作系统进行更改：

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 1: Windows OS</span></summary>

1. 打开 Unity 编辑器
2. 导航至 Tools > MCP Unity > Server Window
3. 将 "Request Timeout (seconds)" 值更改为所需的超时秒数
4. Unity 会将系统环境变量 UNITY_REQUEST_TIMEOUT 设置为新的超时值
5. 重启 Node.js 服务器
6. 再次点击“启动服务器”，将 Unity 编辑器 Web 套接字重新连接到 Node.js MCP 服务器

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 2: 非Windows操作系统</span></summary>

对于非Windows操作系统，需要配置两个地方：

### 编辑器进程超时

1. 打开 Unity 编辑器
2. 导航至 Tools > MCP Unity > Server Window
3. 将 "Request Timeout (seconds)" 值更改为所需的超时秒数

### WebSocket 超时

1. 在终端中设置 UNITY_REQUEST_TIMEOUT 环境变量
    - Powershell
   ```powershell
   $env:UNITY_REQUEST_TIMEOUT = "300"
   ```
    - Command Prompt/Terminal
   ```cmd
   set UNITY_REQUEST_TIMEOUT=300
   ```
2. 重启 Node.js 服务器
3. 再次点击“启动服务器”，将 Unity 编辑器 Web 套接字重新连接到 Node.js MCP 服务器

</details>

> [!TIP]  
> 您的 AI 编码 IDE（例如，Claude Desktop、Cursor IDE、Windsurf IDE）和 MCP 服务器之间的超时取决于 IDE。

## <a name="debug-server"></a>调试服务器

要调试 MCP Unity 服务器，您可以使用以下方法：

### 选项 1: 使用 Unity 编辑器调试
1. 打开 Unity 编辑器
2. 导航到 Tools > MCP Unity > Server Window
3. 点击 "Debug Server" 按钮

### 选项 2: 使用命令行调试
1. 打开终端或命令提示符
2. 导航到 MCP Unity 服务器目录
3. 运行以下命令：
   ```bash
   npm run debug
   ```

## Frequently Asked Questions

### What is MCP Unity?
MCP Unity is a powerful bridge that connects your Unity Editor environment to AI assistants and external tools using the Model Context Protocol (MCP). 

**MCP (Model Context Protocol)** is an open standard designed to allow AI models (like large language models) to interact with software applications and their data in a structured way. Think of it as a universal language that AI can use to "talk" to different programs.

In essence, MCP Unity:
-   Exposes Unity Editor functionalities (like creating objects, modifying components, running tests, etc.) as "tools" and "resources" that an AI can understand and use.
-   Runs a WebSocket server inside Unity and a Node.js server (acting as a WebSocket client to Unity) that implements the MCP. This allows AI assistants to send commands to Unity and receive information back.
-   Enables you to use natural language prompts with your AI assistant to perform complex tasks within your Unity project, significantly speeding up development workflows.

### Why use MCP Unity?
MCP Unity offers several compelling advantages for developers, artists, and project managers:

-   **Accelerated Development:** Automate repetitive tasks, generate boilerplate code, and manage assets using AI prompts. This frees up your time to focus on creative and complex problem-solving.
-   **Enhanced Productivity:** Interact with Unity Editor features without needing to manually click through menus or write scripts for simple operations. Your AI assistant becomes a direct extension of your capabilities within Unity.
-   **Improved Accessibility:** Allows users who are less familiar with the deep intricacies of the Unity Editor or C# scripting to still make meaningful contributions and modifications to a project through AI guidance.
-   **Seamless Integration:** Designed to work with various AI assistants and IDEs that support MCP, providing a consistent way to leverage AI across your development toolkit.
-   **Extensibility:** The protocol and the toolset can be expanded. You can define new tools and resources to expose more of your project-specific or Unity's functionality to AI.
-   **Collaborative Potential:** Facilitates a new way of collaborating where AI can assist in tasks traditionally done by team members, or help in onboarding new developers by guiding them through project structures and operations.

### How does MCP Unity compare with the upcoming Unity 6 AI features?
Unity 6 is set to introduce new built-in AI tools, including Unity Muse (for generative AI capabilities like texture and animation generation) and Unity Sentis (for running neural networks in Unity runtime). As Unity 6 is not yet fully released, this comparison is based on publicly available information and anticipated functionalities:

-   **Focus:**
    -   **MCP Unity:** Primarily focuses on **Editor automation and interaction**. It allows external AI (like LLM-based coding assistants) to *control and query the Unity Editor itself* to manipulate scenes, assets, and project settings. It's about augmenting the *developer's workflow* within the Editor.
    -   **Unity 6 AI (Muse & Sentis):**
        -   **Muse:** Aims at **in-Editor content creation** (generating textures, sprites, animations, behaviors) and AI-powered assistance for common tasks, directly integrated into the Unity Editor interface.
        -   **Sentis:** Focuses on **runtime AI model inference**. It allows you to deploy and run pre-trained neural networks *within your game or application* for features like NPC behavior, image recognition, etc.

-   **Mechanism:**
    -   **MCP Unity:** Uses an external AI assistant communicating via the Model Context Protocol to control the Editor. It's about *external AI driving the Editor*.
    -   **Unity 6 AI:** These are *native, integrated AI features*. Muse will be part of the Editor's UI/UX, and Sentis is a runtime library.

-   **Use Cases:**
    -   **MCP Unity:** "Create a new 3D object, name it 'Player', add a Rigidbody, and set its mass to 10." "Find all materials in the project that use the 'Standard' shader." "Run all Play Mode tests."
    -   **Unity Muse:** "Generate a sci-fi texture for this material." "Create a walking animation for this character." "Help me write a script for player movement."
    -   **Unity Sentis:** Powering intelligent NPCs in your built game, implementing real-time style transfer, enabling voice commands for players.

-   **Complementary, Not Mutually Exclusive:**
    MCP Unity and Unity's native AI tools can be seen as complementary. You might use MCP Unity with your AI coding assistant to set up a scene or batch-modify assets, and then use Unity Muse to generate a specific texture for one of those assets. Sentis would then be used for AI logic in the final game. MCP Unity provides a flexible, protocol-based way to interact with the Editor, which can be powerful for developers who want to integrate with a broader range of external AI services or build custom automation workflows.

### What MCP hosts and IDEs currently support MCP Unity?
MCP Unity is designed to work with any AI assistant or development environment that can act as an MCP client. The ecosystem is growing, but current known integrations or compatible platforms include:

-   **AI Coding Assistants:**
    -   **Windsurf:** A powerful agentic AI coding assistant that can leverage MCP tools.
    -   **Cursor:** An AI-first code editor that can integrate with MCP servers.
    -   Other LLM-based assistants that can be configured to use the MCP protocol.
-   **IDEs:**
    -   **Visual Studio Code (VSCode) and variants (like Cursor):** Configuration is typically done by specifying the MCP server details in the IDE's settings, allowing the AI features within these IDEs to connect to MCP Unity.
-   **MCP Inspector:**
    -   The `@modelcontextprotocol/inspector` is a tool that can be used to debug and interact with any MCP server, including MCP Unity.

The flexibility of MCP means that as more tools adopt the protocol, they should theoretically be able to interface with MCP Unity. Always check the documentation of your specific AI assistant or IDE for MCP support and configuration instructions.

### How can MCP Unity help with "Generative Engine Optimization" (GEO) for my project?
"Generative Engine Optimization" (GEO) is an emerging concept, similar to SEO for websites, but for discoverability and usability by generative AI models and AI-powered search engines (like Google's SGE). Here's how MCP Unity helps:

-   **Structured Data Exposure:** MCP tools and resources expose Unity project data (scene hierarchy, assets, components) in a structured, machine-readable format. This makes it easier for AI to "understand" your project.
-   **Actionable Endpoints:** By providing clear "actions" (tools) the AI can take, MCP Unity allows generative AI to not just read about your project, but to *interact* with and *modify* it. This is key for AI agents that perform tasks.
-   **Clear Tool Definitions:** The names, descriptions, and parameters of MCP tools (e.g., `update_gameobject`, `add_asset_to_scene`) act like keywords and schemas for AI. Well-defined tools make your project more "AI-friendly."
-   **Facilitating AI-Driven Content & Code Generation:** While MCP Unity itself doesn't generate game content directly, it enables AI assistants to *orchestrate* the use of Unity's features. For example, an AI could use MCP Unity tools to set up a scene and then prompt you (or potentially another AI service in the future) to generate assets for that scene.
-   **Standardized Interaction:** Using an open standard like MCP increases the chances that future AI systems and search tools will be able to interface with your project's capabilities exposed via MCP Unity.

Essentially, by making your Unity Editor environment programmatically accessible and understandable to AI through MCP, you are optimizing it for interaction with generative AI engines and AI-driven development workflows.

### Can I extend MCP Unity with custom tools for my project?
Yes, absolutely! One of the significant benefits of the MCP Unity architecture is its extensibility.
-   **In Unity (C#):** You can create new C# classes that inherit from `McpToolBase` (or a similar base for resources) to expose custom Unity Editor functionality. These tools would then be registered in `McpUnityServer.cs`. For example, you could write a tool to automate a specific asset import pipeline unique to your project.
-   **In Node.js Server (TypeScript):** You would then define the corresponding TypeScript tool handler in the `Server/src/tools/` directory, including its Zod schema for inputs/outputs, and register it in `Server/src/index.ts`. This Node.js part will forward the request to your new C# tool in Unity.

This allows you to tailor the AI's capabilities to the specific needs and workflows of your game or application.

### Troubleshooting Common Issues

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Connection Issues</span></summary>

- Ensure the WebSocket server is running (check the Server Window in Unity)
- Send a console log message from MCP client to force a reconnection between MCP client and Unity server
- Change the port number in the Unity Editor MCP Server window. (Tools > MCP Unity > Server Window)
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Server Not Starting</span></summary>

- Check the Unity Console for error messages
- Ensure Node.js is properly installed and accessible in your PATH
- Verify that all dependencies are installed in the Server directory
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Connection failed when running Play Mode tests</span></summary>

The `run_tests` tool returns the following response:
```
Error:
Connection failed: Unknown error
```

This error occurs because the bridge connection is lost when the domain reloads upon switching to Play Mode.  
The workaround is to turn off **Reload Domain** in **Edit > Project Settings > Editor > "Enter Play Mode Settings"**.
</details>

### Where can I find more information or contribute?
- **Official MCP Unity Repository:** [https://github.com/CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity) (Check for Issues, Discussions, and the Roadmap)
- **Model Context Protocol Documentation:** [https://modelcontextprotocol.io](https://modelcontextprotocol.io)
- **Unity Forums and Communities:** For general Unity questions or discussions on integrating AI.

### Is MCP Unity free to use?
Yes, MCP Unity is an open-source project distributed under the MIT License. You are free to use, modify, and distribute it according to the license terms.

## 支持与反馈

如有任何问题或需要支持，请在本仓库[提交 issue](https://github.com/CoderGamester/mcp-unity/issues)。

你也可以通过以下方式联系：
- Linkedin: [![](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white 'LinkedIn')](https://www.linkedin.com/in/miguel-tomas/)
- Discord: gamester7178
- 邮箱: game.gamester@gmail.com

## 贡献

欢迎贡献！请随时提交 Pull Request 或提出 Issue。

**请遵循 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/v1.0.0/) 格式提交更改。**

## 许可证

本项目采用 [MIT License](License.md) 授权。

## 鸣谢

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)

## 贡献

欢迎贡献！请阅读我们的[贡献指南](CONTRIBUTING.md)以获取更多信息。

## 许可证

此项目根据 MIT 许可证授权 - 详情请参阅 [LICENSE](LICENSE) 文件。

## 致谢

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)
