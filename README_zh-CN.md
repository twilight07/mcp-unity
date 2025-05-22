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

## 可选：安装 Node.js 服务器
默认情况下，Node.js 服务器安装在 `Server~/` 目录下。
如遇到问题，可以通过以下方式强制安装：

1. 打开 Unity 编辑器
2. 导航到 Tools > MCP Unity > Server Window
3. 点击“Force Install Server”按钮

> [!TIP]
> Node.js 服务器会安装在 `Server~/` 目录下。


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

## 可选：超时设置

默认情况下，MCP 服务器与 WebSocket 之间的超时时间为 10 秒。
您可以通过以下步骤进行修改：

1. 打开 Unity 编辑器  
2. 导航到 Tools > MCP Unity > Server Window  
3. 修改 **Request Timeout (seconds)** 为所需的秒数  
4. Unity 会设置环境变量 `UNITY_REQUEST_TIMEOUT` 为新的超时时间  
5. 重启 Node.js 服务器  
6. 再次点击 **Start Server** 以重新连接  

> [!TIP]  
> AI 编码 IDE（例如 Claude Desktop、Cursor IDE、Windsurf IDE）与 MCP 服务器之间的超时时间可能因 IDE 而异。

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

## 支持与反馈

有任何问题或需要帮助，请在本仓库打开[Issue](https://github.com/CoderGamester/mcp-unity/issues)，或通过以下方式联系：
- LinkedIn: [![](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white 'LinkedIn')](https://www.linkedin.com/in/miguel-tomas/)
- Discord: gamester7178
- 电子邮件: game.gamester@gmail.com

## 贡献

欢迎贡献！请提交 Pull Request 或者打开 Issue。

**提交信息** 请遵循 [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) 格式。

## 许可证

本项目基于 [MIT License](License.md) 开源。

## 致谢

- Model Context Protocol
- Unity Technologies
- Node.js
- WebSocket-Sharp
