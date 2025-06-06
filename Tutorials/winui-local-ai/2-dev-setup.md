# Dev Set up & Project Structure

This portion takes you through the steps needed to begin creating your Windows app using the latest Windows development frameworks and tools: WinUI, Windows App SDK, and Community Toolkit.

## Dependencies

Ensure that your PC supports Windows AI Foundry and that all dependencies are installed.

1. Follow the steps in the [Dependencies](https://learn.microsoft.com/en-us/windows/ai/apis/get-started?tabs=winget%2Cwinui%2Cwinui2#dependencies) section.

If you computer does not support Windows AI Foundry, you will not be able to complete steps 6 & 7.


## First time Windows developer?

Complete the following steps if you are new to Windows development:

<details>
  <summary>New to Windows Development setup:</summary>

## Enable Developer Mode


Windows has a special mode for developers that adjusts security settings to let you run the apps you're working on. You need to enable Developer Mode before you can build, deploy, and test your app using Visual Studio.

To enable Developer Mode:

1. Open Windows Settings and navigate to the **System** > **For developers** page.
1. Toggle the Developer Mode switch to **On** and confirm your choice in the confirmation dialog.

## Install Visual Studio

You use [Visual Studio](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/visualstudio/ide), Microsoft's comprehensive integrated development environment (IDE), to create your WinUI app. This IDE helps you write, debug, and deploy your apps. The project templates in Visual Studio get you started with projects for Windows and many other platforms.

Note: Before installing these tools, ensure your development computer meets the [system requirements for Windows app development](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/windows-app-sdk/system-requirements.md).

1. Go to the Visual Studio page to download and install the latest Visual Studio. You can use the free Visual Studio Community Edition.

Note: The installer guides you through the steps, but if you need detailed instructions, see [Install Visual Studio](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/visualstudio/install/install-visual-studio).

While installing Visual Studio, you need to install the workloads and components required for developing with WinUI and the Windows App SDK.

1. Open the Visual Studio Installer app
1. Select **Modify** to add workloads and components.
1. On the **Workloads** tab of the Visual Studio Installer app, select the following workloads and components:

    - [Visual Studio 2022 version 17.10 and later](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/get-started/start-here.md#tab/vs-2022-17-10)

        - For **C#** app development using the Windows App SDK, select **Windows application development**.

    - [Visual Studio 2022 version 17.1 - 17.9](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/get-started/start-here.md#tab/vs-2022-17-1)

        - For **C#** app development using the Windows App SDK, select **.NET Desktop Development**.

        - In the **Installation details** pane of the installation dialog box, select **Windows App SDK C# Templates** (near the bottom of the list). 
</details>

## Create base WinUI app

To create a new project using the WinUI C# Blank App project template:

1. Open Visual Studio and select **Create a new project** from the launch page. (If Visual Studio is already open to the editor, select **File** > **New**> **Project**)
1. Search for **WinUI** and select the **Blank App, Packaged (WinUI 3 in Desktop)** project template
1. Click **Next**
1. . Name your project: **PoemGenerator**

> **_Important:_** The Project Name and Solution Name must be PoemGenerator. Visual Studio creates namespaces using this information. If it's something different, the code you copy and paste from this tutorial can cause build errors.

5. Select **Create**.

## Update & Install Packages

The Windows App SDK (including WinUI) and tools like the Community Toolkit are distributed as [NuGet packages](https://www.nuget.org/). This distribution method allows updates to be released independently of Windows and Visual Studio. As a result, the Visual Studio template you used to create your project might not reference the latest versions of these NuGet packages. To ensure you have the latest features and fixes, you should update your NuGet packages every time you create a new project in Visual Studio.

> **_Note:_**   NuGet packages are single ZIP files with a .nupkg extension that contains compiled code (Dynamic Link Library (DLLs)), related files, and a descriptive manifest. These packages serve as a mechanism for developers to create, share, and consume useful code in .NET applications.

To update the Windows App SDK NuGet package for your project to get access to Windows AI Foundry​ APIs :

1. In Visual Studio, with your project loaded, in the tool bar on the top, select **Tools** > **NuGet Package Manager** > **Manage NuGet Packages for Solution....**

> **_Important:_**  Projects must not be running in order to access the "Manage Nuget packages" option in the drop down

2. Locate the `Package source` on the top right

![Screenshot of Visual Studio ](assets/package-source.png)

3. Confirm that it is set to `nuget.org`
    - if it is not, follow [these](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources) instructions.
4. Click on the **Installed** tab.
5. Click on **Microsoft.WindowsAppSDK**
6. Select the **Checkbox** to Include prelease
7. On the right panel:
    1. select the **checkbox** of the **Project**
    1. Select from the dropdown  **winappsdk1.8-experimental1**
    1. Click **install**
        1. On the Preview Changes Popup, **Click Apply**.
        1. On the License Acceptance Popup, **Click I Accept**.
8. To install the CommunityToolkit NuGet package for your project:
    1. Still in the Manage NuGet Packages for Solution, Click on the **Browse** tab.
    1. Enter **`CommunityToolkit.Mvvm`** in the search bar
    1. **Click to select** `CommunityToolkit.Mvvm`
    1. On the right panel:
        1. Select the **checkbox** of the **Project**
        1. **Select** version `8.4.0`
        1. Click **Install**.
            1. On the Preview Changes Popup, **Click Apply**.
            1. On the License Acceptance Popup, **Click I Accept**.
9. **Close** the  NuGet Package Manager

The Windows AI Foundry​ APIs uses a specifies  version of the .NET runtime and the minimum Windows version that the app targets. Also, the Community Toolkit.Mvvm and uses features that are ahead of the standard C# language version that your project is configured to use by default. You need to update the project's configuration:

10. If Solution Explorer isn't already open, you can open it by selecting **View** > **Solution Explorer** from the top menu.
11. In the Solution Explorer, you see a tree structure representing your solution and its projects. Look for the project node named **PoemGenerator**. This node is usually a icon of a box with "C#" written inside, the project name next to it.

![Screenshot of Edit Project](assets/edit-project.png)

12. **Right-click** on the project node (**PoemGenerator**) and select **Edit Project File**. This action opens the .csproj file in the editor.
13. **Locate** the first `PropertyGroup` element in the file
14. **Add** `<LangVersion>preview</LangVersion>`
15. **Locate** `<TargetFramework>`
16. **Replace** it with `<TargetFramework>net8.0-windows10.0.22621.0</TargetFramework>`
17. **Save and Close** the file

Adding Preview is a way to unlock experimental language features required by advanced tools like Community Toolkit.Mvvm, enabling developers to use cutting-edge capabilities in their projects.

Now your project is using the latest WinUI features that are available.

## Run Project

You can run this blank project:

1. On the title bar, **Click** on **Debug** > **Start Debugging** OR on your keyboard press **F5** key

1. To stop debugging, **Close** the app window, or press **Shift + F5** or in Visual Studio, on the title bar, **Click** on **Debug** > **Stop Debugging**

## The project's file structure

To see the starting boilerplate code from the WinUI project template, you need to open the Solution Explorer. If Solution Explorer isn't already open, you can access it by selecting **View** > **Solution Explorer** from the top menu.

For this course, you modify only the `MainWindow.xaml` and `MainWindow.xaml.cs` files, leaving the rest of the files unchanged. More files will be added as we progress through the course. The `MainWindow.xaml` file defines what the UI looks like, while the `MainWindow.xaml.cs` file defines how the UI behaves. Within the framework of MVVM (Model-View-ViewModel), these two files are considered part of the View layer.

> **_Note:_**  Not all projects strictly follow the MVVM pattern. Some developers opt for a "code-behind approach," where logic is placed directly in the .xaml.cs file instead of implementing a separate ViewModel. This approach is often referred to as the **"code-behind pattern."**

![Diagram of Project structures organized by MVVM](assets/blank-project-structure.png)

## Organizing Your Project Files

To improve maintainability, you create two new folders for organizing your project files: one for models and another for pages.

Steps to Create Folders:

1. In Solution Explorer, **right-click** on **PoemGenerator** (the icon with box containing C# in it).
1. Select **Add** > **New Folder**.
1. Name the new folder `Models`.
1. Repeat steps 1 & 2 and name the second folder `Pages`.

### Why Name It "Pages" Instead of "Views"?

The folder is named Pages instead of Views because it stores different pages of your app. For example, while MainWindow.xaml is part of the View layer, it remains in the root directory because most developers expect to find it there. Moving it into a folder named "Views" would break this convention and could cause confusion.

In the next section, we’ll explore how MainWindow.xaml and how pages work.

Next [Build UI](./3-build-ui.md)