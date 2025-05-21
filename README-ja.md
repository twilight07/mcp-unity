# MCP Unity Editor（ゲームエンジン）

[![](https://badge.mcpx.dev?status=on 'MCP 有効')](https://modelcontextprotocol.io/introduction)
[![](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white 'Unity')](https://unity.com/releases/editor/archive)
[![](https://img.shields.io/badge/Node.js-339933?style=flat&logo=nodedotjs&logoColor=white 'Node.js')](https://nodejs.org/en/download/)
[![](https://img.shields.io/github/stars/CoderGamester/mcp-unity 'スター')](https://github.com/CoderGamester/mcp-unity/stargazers)
[![](https://img.shields.io/github/last-commit/CoderGamester/mcp-unity '最終コミット')](https://github.com/CoderGamester/mcp-unity/commits/main)
[![](https://img.shields.io/badge/License-MIT-red.svg 'MIT ライセンス')](https://opensource.org/licenses/MIT)

| [英語](README.md) | [🇨🇳簡体中文](README_zh-CN.md) | [🇯🇵日本語](README-ja.md) |
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

MCP Unityは、Unityエディター向けのModel Context Protocolの実装であり、AIアシスタントがUnityプロジェクトと対話できるようにします。このパッケージは、UnityとMCPプロトコルを実装するNode.jsサーバー間のブリッジを提供し、Claude、Windsurf、CursorなどのAIエージェントがUnityエディター内で操作を実行できるようにします。

<a href="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity">
  <img width="400" height="200" src="https://glama.ai/mcp/servers/@CoderGamester/mcp-unity/badge" alt="Unity MCPサーバー" />
</a>

## 機能

### IDE統合 - パッケージキャッシュアクセス

MCP Unityは、Unityの`Library/PackedCache`フォルダーをワークスペースに追加することで、VSCode系IDE（Visual Studio Code、Cursor、Windsurf）との自動統合を提供します。この機能により：

- Unityパッケージのコードインテリジェンスが向上
- Unityパッケージのより良いオートコンプリートと型情報が有効化
- AIコーディングアシスタントがプロジェクトの依存関係を理解するのに役立つ

### MCPサーバーツール

- `execute_menu_item`: Unityメニュー項目（MenuItem属性でタグ付けされた関数）を実行
  > **例:** "新しい空のGameObjectを作成するためにメニュー項目'GameObject/Create Empty'を実行"

- `select_gameobject`: パスまたはインスタンスIDでUnity階層内のゲームオブジェクトを選択
  > **例:** "シーン内のMain Cameraオブジェクトを選択"

- `update_gameobject`: GameObject のコアプロパティ（名前、タグ、レイヤー、アクティブ/静的状態）を更新、または存在しない場合は作成します
  > **例:** "Playerオブジェクトのタグを ‘Enemy’ に設定し、非アクティブにする"

- `update_component`: GameObject上のコンポーネントフィールドを更新、またはGameObjectに含まれていない場合は追加
  > **例:** "PlayerオブジェクトにRigidbodyコンポーネントを追加し、その質量を5に設定"

- `add_package`: Unityパッケージマネージャーに新しいパッケージをインストール
  > **例:** "プロジェクトにTextMeshProパッケージを追加"

- `run_tests`: Unityテストランナーを使用してテストを実行
  > **例:** "プロジェクト内のすべてのEditModeテストを実行"

- `send_console_log`: Unityにコンソールログを送信
  > **例:** "Unity Editorにコンソールログを送信"

- `add_asset_to_scene`: AssetDatabaseからアセットをUnityシーンに追加
  > **例:** "プロジェクトからPlayerプレハブを現在のシーンに追加"

### MCPサーバーリソース

- `unity://menu-items`: `execute_menu_item`ツールを容易にするために、Unityエディターで利用可能なすべてのメニュー項目のリストを取得
  > **例:** "GameObject作成に関連する利用可能なすべてのメニュー項目を表示"

- `unity://scenes-hierarchy`: 現在のUnityシーン階層内のすべてのゲームオブジェクトのリストを取得
  > **例:** "現在のシーン階層構造を表示"

- `unity://gameobject/{id}`: シーン階層内のインスタンスIDまたはオブジェクトパスで特定のGameObjectに関する詳細情報を取得
  > **例:** "Player GameObjectに関する詳細情報を取得"

- `unity://logs`: Unityコンソールからのすべてのログのリストを取得
  > **例:** "Unityコンソールからの最近のエラーメッセージを表示"

- `unity://packages`: Unityパッケージマネージャーからインストール済みおよび利用可能なパッケージ情報を取得
  > **例:** "プロジェクトに現在インストールされているすべてのパッケージをリスト"

- `unity://assets`: Unityアセットデータベース内のアセット情報を取得
  > **例:** "プロジェクト内のすべてのテクスチャアセットを検索"

- `unity://tests/{testMode}`: Unityテストランナー内のテスト情報を取得
  > **例:** "プロジェクトで利用可能なすべてのテストをリスト"

## 要件
- Unity 2022.3以降 - [サーバーをインストール](#install-server)するため
- Node.js 18以降 - [サーバーを起動](#start-server)するため
- npm 9以降 - [サーバーをデバッグ](#debug-server)するため

## <a name="install-server"></a>インストール

このMCP Unityサーバーのインストールは複数ステップのプロセスです：

### ステップ1: Unityパッケージマネージャー経由でUnity MCPサーバーパッケージをインストール
1. Unityパッケージマネージャーを開く（Window > Package Manager）
2. 左上隅の"+"ボタンをクリック
3. "Add package from git URL..."を選択
4. 入力: `https://github.com/CoderGamester/mcp-unity.git`
5. "Add"をクリック

![package manager](https://github.com/user-attachments/assets/a72bfca4-ae52-48e7-a876-e99c701b0497)


### ステップ2: Node.jsをインストール
> MCP Unityサーバーを実行するには、コンピューターにNode.js 18以降がインストールされている必要があります：

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Windows</span></summary>

1. [Node.jsダウンロードページ](https://nodejs.org/en/download/)にアクセス
2. LTSバージョンのWindowsインストーラー（.msi）をダウンロード（推奨）
3. インストーラーを実行し、インストールウィザードに従う
4. PowerShellを開いて以下を実行してインストールを確認：
   ```bash
   node --version
   ```
</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">macOS</span></summary>

1. [Node.jsダウンロードページ](https://nodejs.org/en/download/)にアクセス
2. LTSバージョンのmacOSインストーラー（.pkg）をダウンロード（推奨）
3. インストーラーを実行し、インストールウィザードに従う
4. または、Homebrewがインストールされている場合は以下を実行：
   ```bash
   brew install node@18
   ```
5. ターミナルを開いて以下を実行してインストールを確認：
   ```bash
   node --version
   ```
</details>

### ステップ3: AI LLMクライアントを設定

<details open>
<summary><span style="font-size: 1.1em; font-weight: bold;">オプション1: Unityエディターを使用して設定</span></summary>

1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. 以下の画像のようにAI LLMクライアントの"Configure"ボタンをクリック

![image](https://github.com/user-attachments/assets/8d286e83-da60-40fa-bd6c-5de9a77c1820)

4. 表示されるポップアップで設定インストールを確認

![image](https://github.com/user-attachments/assets/b1f05d33-3694-4256-a57b-8556005021ba)

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">オプション2: 手動設定</span></summary>

AIクライアントのMCP設定ファイル（例：Claude Desktopのclaude_desktop_config.json）を開き、以下のテキストをコピー：

> `ABSOLUTE/PATH/TO`をMCP Unityインストールの絶対パスに置き換えるか、UnityエディターMCPサーバーウィンドウ（Tools > MCP Unity > Server Window）からテキストをコピー

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

## <a name="start-server"></a>サーバーの起動

MCP Unityサーバーを起動するには2つの方法があります：

### オプション1: Unityエディター経由で起動
1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. "Start Server"ボタンをクリック

### オプション2: コマンドラインから起動
1. ターミナルまたはコマンドプロンプトを開く
2. MCP Unityサーバーディレクトリに移動
3. 以下のコマンドを実行：
   ```bash
   node Server~/build/index.js
   ```

## オプション: タイムアウト設定

デフォルトでは、MCPサーバーとWebSocket間のタイムアウトは 10 秒です。
お使いのOSに応じて変更できます。

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 1: Windows OS</span></summary>

1. Unityエディターを開きます
2. **Tools > MCP Unity > Server Window** に移動します
3. **Request Timeout (seconds)** の値を希望のタイムアウト秒数に変更します
4. Unityはシステム環境変数UNITY_REQUEST_TIMEOUTに新しいタイムアウト値を設定します
5. Node.jsサーバーを再起動します
6. **Start Server** をもう一度クリックして、UnityエディターのWebソケットをNode.js MCPサーバーに再接続します

</details>

<details>
<summary><span style="font-size: 1.1em; font-weight: bold;">Option 2: Windows以外のOS</span></summary>

Windows 以外の OS の場合は、次の 2 か所で設定する必要があります。

### エディター内プロセスのタイムアウト

1. Unityエディターを開きます
2. **Tools > MCP Unity > Server Window** に移動します
3. **Request Timeout (seconds)** の値を希望のタイムアウト秒数に変更します

### WebSocketのタイムアウト

1. ターミナルで UNITY_REQUEST_TIMEOUT 環境変数を設定します
    - Powershell
   ```powershell
   $env:UNITY_REQUEST_TIMEOUT = "300"
   ```
    - Command Prompt/Terminal
   ```cmd
   set UNITY_REQUEST_TIMEOUT=300
   ```
2. Node.jsサーバーを再起動します
3. **Start Server** をもう一度クリックして、UnityエディターのWebソケットをNode.js MCPサーバーに再接続します

</details>

> [!TIP]  
> AIコーディングIDE（Claude Desktop、Cursor IDE、Windsurf IDE など）とMCPサーバー間のタイムアウト設定は、IDEによって異なります。

## <a name="debug-server"></a>サーバーのデバッグ

MCP Unityサーバーをデバッグするには、以下の方法を使用できます：

### オプション1: Unityエディターを使用してデバッグ
1. Unityエディターを開く
2. Tools > MCP Unity > Server Windowに移動
3. "Debug Server"ボタンをクリック

### オプション2: コマンドラインを使用してデバッグ
1. ターミナルまたはコマンドプロンプトを開く
2. MCP Unityサーバーディレクトリに移動
3. 以下のコマンドを実行：
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

## サポート・フィードバック

ご質問やサポートが必要な場合は、このリポジトリの[issue](https://github.com/CoderGamester/mcp-unity/issues)をご利用ください。

また、以下でも連絡可能です：
- Linkedin: [![](https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white 'LinkedIn')](https://www.linkedin.com/in/miguel-tomas/)
- Discord: gamester7178
- メール: game.gamester@gmail.com

## コントリビューション

コントリビューションは大歓迎です！Pull Requestの送信やIssueの提出をお待ちしています。

**変更は [Conventional Commits](https://www.conventionalcommits.org/ja/v1.0.0/) フォーマットに従ってください。**

## ライセンス

本プロジェクトは [MIT License](License.md) の下で提供されています。

## 謝辞

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)

## 貢献

貢献は大歓迎です！詳細については[貢献ガイド](CONTRIBUTING.md)をお読みください。

## ライセンス

このプロジェクトはMITライセンスの下でライセンスされています - 詳細は[LICENSE](LICENSE)ファイルを参照してください。

## 謝辞

- [Model Context Protocol](https://modelcontextprotocol.io)
- [Unity Technologies](https://unity.com)
- [Node.js](https://nodejs.org)
- [WebSocket-Sharp](https://github.com/sta/websocket-sharp)
