# Custom Task-Code Generation

If you are not clear on terms such as tasks, targets, properties, or runtimes, you could first check out the docs that explain these concepts, starting with the [MSBuild Concepts article](https://docs.microsoft.com/visualstudio/msbuild/msbuild-concepts).  

The basic idea of the current example is defined as:

```
Input text => Generation => Output C# (Some code generation)
```

We are going to create a msbuild custom task named AppSettingStronglyTyped. General information about how to [write msbuild custom task](https://docs.microsoft.com/visualstudio/msbuild/task-writing). The task is going to read a set of text files, and each file with lines with the following format:
```
propertyName:type:defaultValue
```
Then our code will generate a c# class with all the constants. :innocent: This is not useful at all, it is simple, the idea is help us to learn the mechanism.  
A problem should stop the build and give us enough information.

## Step 1, create the AppSettingStronglyTyped project
Create a Class Library Net Standard. The Framework should be .Net Standard 2.0.  

:warning: Before we go too far, you must first understand the different between “full” MSBuild (the one that powers Visual Studio) and “portable” MSBuild, or the one bundled in the .NET Core Command Line.

* Full MSBuild: This version of MSBuild usually lives inside Visual Studio. Runs on .NET Framework. Visual Studio uses this when you execute “Build” on your solution or project.
* Dotnet MSBuild: This version of MSBuild is bundled in the .NET Core Command Line. Runs on .NET Core. Visual Studio does not directly invoke this version of MSBuild. Currently only supports projects that build using Microsoft.NET.Sdk.

if you want to share code between .NET Framework and any other .NET implementation, such as .NET Core, your library should target [.NET Standard 2.0](https://docs.microsoft.com/dotnet/standard/net-standard), and we want to run inside Visual Studio which runs on .NET Framework. .NET Framework doesn't support .NET Standard 2.1.
 