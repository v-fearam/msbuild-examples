# Custom Task-Code Generation

If you are not clear on terms such as tasks, targets, properties, or runtimes, you could first check out the docs that explain these concepts, starting with the [MSBuild Concepts article](https://docs.microsoft.com/visualstudio/msbuild/msbuild-concepts).

The basic idea of the current example is defined as:

```
Input text => Generation => Output C# (Some code generation)
```

We are going to create a msbuild custom task named AppSettingStronglyTyped. The task is going to read a set of text files, and each file with lines with the following format:

```
propertyName:type:defaultValue
```

Then our code will generate a c# class with all the constants. :innocent: This is not useful at all, it is simple, the idea is help us to learn the mechanism.  
A problem should stop the build and give us enough information.

## Step 1, create the AppSettingStronglyTyped project

Create a Class Library Net Standard. The Framework should be .Net Standard 2.0.

:warning: Before we go too far, you must first understand the different between “full” MSBuild (the one that powers Visual Studio) and “portable” MSBuild, or the one bundled in the .NET Core Command Line.

- Full MSBuild: This version of MSBuild usually lives inside Visual Studio. Runs on .NET Framework. Visual Studio uses this when you execute “Build” on your solution or project.
- Dotnet MSBuild: This version of MSBuild is bundled in the .NET Core Command Line. Runs on .NET Core. Visual Studio does not directly invoke this version of MSBuild. Currently only supports projects that build using Microsoft.NET.Sdk.

if you want to share code between .NET Framework and any other .NET implementation, such as .NET Core, your library should target [.NET Standard 2.0](https://docs.microsoft.com/dotnet/standard/net-standard), and we want to run inside Visual Studio which runs on .NET Framework. .NET Framework doesn't support .NET Standard 2.1.

## Step 2, create the AppSettingStronglyTyped MSBuild Custom Task

We need to create our MSBuild CustomTask. Information about how to [write msbuild custom task](https://docs.microsoft.com/visualstudio/msbuild/task-writing), it is good information to understand the following steps.

We need to include _Microsoft.Build.Utilities.Core_ nuget package, and the create a AppSettingStronglyTyped derived from Microsoft.Build.Utilities.Task.

We are going use three parameters:

```dotnet
        //The name of the class which is going to be generated
        [Required]
        public string SettingClassName { get; set; }

        //The name of the namespace where the class is going to be generated
        [Required]
        public string SettingNamespaceName { get; set; }

        //List of files which we need to read with the defined format: 'propertyName:type:defaultValue' per line
        [Required]
        public ITaskItem[] SettingFiles { get; set; }
```

The task is going to process the _SettingFiles_ and generate a class 'SettingNamespaceName.SettingClassName'. The class will have a set of constants based on the file's content.  
The task output will be:

```dotnet
        //The filename where the class was generated
        [Output]
        public string ClassNameFile { get; set; }
```

We need to override a Execute method. The execute method return true if the task was succeed and false other case. Task implements ITask and provides default implementations of some ITask members and additionally, logging is easier. It is important the log to know what is going on. And even more important if we are going to return not succeed (false). On error, we should use Log.LogError.

```dotnet
        public override bool Execute()
        {
            //Read the input files and return a IDictionary<string, object> with the properties to be created.
            //Any format error it will return not succeed and Log.LogError properly
            var (success, settings) = ReadProjectSettingFiles();
            if (!success)
            {
                return success;
            }
            //Create the class based on the Dictionary
            return CreateSettingClass(settings);
        }
```

Then, the details are really not important for our purpose. You can copy from the source code and improve if you like.

### Step 3, Change the AppSettingStronglyTyped.csproj

We need to do some changes on the project file. Now we have something simple like

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Build.Utilities.Core" Version="17.0.0" />
  </ItemGroup>

</Project>
```

We are going to generate a nuget package, so first we need to add some basic information

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
	<version>1.0.0</version>
	<title>AppSettingStronglyTyped</title>
	<authors>Federico Arambarri</authors>
	<description>Generates a strongly typed setting class base on a txt file</description>
	<tags>MyTags</tags>
	<copyright>Copyright © Company 2022</copyright>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Build.Utilities.Core" Version="17.0.0" />
  </ItemGroup>

</Project>
```

Then, the dependencies of your MSBuild task must be packaged inside the package, they cannot be expressed as normal PackageReferences. We don't expose any regular dependencies to the outside world. It is not need for the current example, because we don't have extra dependencies, but it is worth to be prepared for and be aware of.

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>netstandard2.0</TargetFramework>
		<version>1.0.0</version>
		<title>AppSettingStronglyTyped</title>
		<authors>Federico Arambarri</authors>
		<description>Generates a strongly typed setting class base on a txt file</description>
		<tags>MyTags</tags>
		<copyright>Copyright © Company 2022</copyright>
		<!-- we need the assemblies bundled, so set this so we don't expose any dependencies to the outside world -->
		<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
		<TargetsForTfmSpecificBuildOutput>$(TargetsForTfmSpecificBuildOutput);CopyProjectReferencesToPackage</TargetsForTfmSpecificBuildOutput>
		<DebugType>embedded</DebugType>
		<IsPackable>true</IsPackable>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.Build.Utilities.Core" Version="17.0.0" />
	</ItemGroup>

	<Target Name="CopyProjectReferencesToPackage" DependsOnTargets="ResolveReferences">
		<ItemGroup>
			<!-- the dependencies of your MSBuild task must be packaged inside the package, they cannot be expressed as normal PackageReferences -->

			<!--example: <BuildOutputInPackage Include="$(PkgFParsec)/lib/netstandard2.0/FParsecCS.dll" />-->
		</ItemGroup>
	</Target>

</Project>

```

### Step 4, Include MSBuild props and targets in a package

We recommend first read the bases about [props and target](https://docs.microsoft.com/visualstudio/msbuild/customize-your-build) and then how to [include props and targets on a nuget](https://docs.microsoft.com/nuget/create-packages/creating-a-package#include-msbuild-props-and-targets-in-a-package).

In some cases, you might want to add custom build targets or properties in projects that consume your package, such as running a custom tool or process during build. You do this by placing files in the form <package_id>.targets or <package_id>.props within the \build folder of the project.  
Files in the root \build folder are considered suitable for all target frameworks.  
In this next step we’ll wire up the task implementation in a .props and .targets file, which will be included in our NuGet package and automatically loaded from a referencing project.
First, we should modify the AppSettingStronglyTyped.csproj, adding

```xml
	<ItemGroup>
		<!-- these lines pack the build props/targets files to the `build` folder in the generated package.
         by convention, the .NET SDK will look for build\<Package Id>.props and build\<Package Id>.targets
         for automatic inclusion in the build. -->
		<Content Include="build\AppSettingStronglyTyped.props" PackagePath="build\" />
		<Content Include="build\AppSettingStronglyTyped.targets" PackagePath="build\" />
	</ItemGroup>
```

Then we must create a _build_ folder and inside two text files: _AppSettingStronglyTyped.props_ and _AppSettingStronglyTyped.targets_.
AppSettingStronglyTyped.props is imported very early in Microsoft.Common.props, and properties defined later are unavailable to it. So, avoid referring to properties that are not yet defined (and will evaluate to empty).

Directory.Build.targets is imported from Microsoft.Common.targets after importing .targets files from NuGet packages. So, it can override properties and targets defined in most of the build logic, or set properties for all your projects regardless of what the individual projects set. You can see [import order](https://docs.microsoft.com/visualstudio/msbuild/customize-your-build?view=vs-2022#import-order).  
_AppSettingStronglyTyped.props_ includes the task and define some prop with default values:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<!--defining properties interesting for my task-->
	<PropertyGroup>
		<!--default directory where the .dll was publich inside a nuget package-->
		<taskForldername>lib</taskForldername>
		<taskFramework>netstandard2.0</taskFramework>
		<!--The folder where the custom task will be present. It points to inside the nuget package. It could be override during development time to get directly from the project compile directory -->
		<CustomTasksFolder Condition="'$(CustomTasksFolder)' == ''">$(MSBuildThisFileDirectory)\..\$(taskForldername)\$(taskFramework)</CustomTasksFolder>
		<!--Reference to the assembly which contains the MSBuild Task-->
		<CustomTasksAssembly>$(CustomTasksFolder)\$(MSBuildThisFileName).dll</CustomTasksAssembly>
	</PropertyGroup>

	<!--If a project is going to run a task, MSBuild must know how to locate and run the assembly that contains the task class. Tasks are registered using the UsingTask element (MSBuild). TaskName is the class name and AssemblyFile the dll file path where the class is included-->
	<UsingTask TaskName="$(MSBuildThisFileName).$(MSBuildThisFileName)" AssemblyFile="$(CustomTasksAssembly)"/>

	<!--Task parameters default values, this can be overridden-->
	<PropertyGroup>
		<RootFolder Condition="'$(RootFolder)' == ''">$(MSBuildProjectDirectory)</RootFolder>
		<SettingClass Condition="'$(SettingClass)' == ''">MySetting</SettingClass>
		<SettingNamespace Condition="'$(SettingNamespace)' == ''">Example</SettingNamespace>
		<SettingExtensionFile Condition="'$(SettingExtensionFile)' == ''">mysettings</SettingExtensionFile>
	</PropertyGroup>
</Project>
```

Beyond the [build properties](https://docs.microsoft.com/visualstudio/msbuild/walkthrough-using-msbuild?view=vs-2022#build-properties) defined, actually, important part of this file is the task registration, MSBuild must know how to locate and run the assembly that contains the task class. Tasks are registered using the [UsingTask element (MSBuild)](https://docs.microsoft.com/visualstudio/msbuild/usingtask-element-msbuild?view=vs-2022).Taskname is the name of the task to reference from the assembly. This attribute should always specify full namespaces. AssemblyFile is the file path of the assembly.

The _AppSettingStronglyTyped.props_ will be automatically included when the package is install, then our client has the task available and some default values. However, it is never used. In order to put this code in action we need to define some targets on _AppSettingStronglyTyped.targets_ file which also will be also automatically included when the package is install:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

	<!--Defining all the text files input parameters-->
	<ItemGroup>
		<SettingFiles Include="$(RootFolder)\*.$(SettingExtensionFile)" />
	</ItemGroup>

	<!--It is generated a target which is executed before the compilation-->
	<!--It is built only if 1- Output doesn't exist or 2- Input is newer than output-->
	<Target Name="BeforeCompile" Inputs="@(SettingFiles)" Outputs="$(RootFolder)\$(SettingClass).generated.cs">
		<!--Calling our custom task-->
		<AppSettingStronglyTyped SettingClassName="$(SettingClass)" SettingNamespaceName="$(SettingNamespace)" SettingFiles="@(SettingFiles)">
			<Output TaskParameter="ClassNameFile" PropertyName="SettingClassFileName" />
		</AppSettingStronglyTyped>
		<!--Our generated file is included to be compiled-->
		<ItemGroup>
			<Compile Remove="$(SettingClassFileName)" />
			<Compile Include="$(SettingClassFileName)" />
		</ItemGroup>
	</Target>

	<!--The generated file is deleted after a general clean. It will force the regeneration on rebuild-->
	<Target Name="AfterClean">
		<Delete Files="$(RootFolder)\$(SettingClass).generated.cs" />
	</Target>
</Project>
```

The first step is the creation of an [InputGroup](https://docs.microsoft.com/visualstudio/msbuild/msbuild-items?view=vs-2022) which represents the text files (it could be more than one) to read and it will be some of our task parameter. There are default for the location and the extension which we look for, but you can override the values defining the properties on the client msbuild project file.

Then we define two [MSBuild targets](https://docs.microsoft.com/visualstudio/msbuild/msbuild-targets?view=vs-2022). We [extends the MSBuild process](https://docs.microsoft.com/visualstudio/msbuild/how-to-extend-the-visual-studio-build-process?view=vs-2022) overriding predefined targets:

1. BeforeCompile: The goal is to call our custom task to generate the class and include the class to be compiled. Tasks that are inserted before core compilation is done. Input and Output field are related to [incremental build](https://docs.microsoft.com/visualstudio/msbuild/incremental-builds?view=vs-2022).If all output items are up-to-date, MSBuild skips the target. This incremental build of the target can significantly improve the build speed. An item is considered up-to-date if its output file is the same age or newer than its input file or files.
1. AfterClean: The goal is to delete the generated class file after a general clean happens.Tasks that are inserted after the core clean functionality is invoked. It force the generation on MSBuild rebuild target execution. 

### Step 5, Generates the nuget package

We can use Visual Studio (Right click on the project and select 'pack').
We can also do it by command line. Move to the folder where the AppSettingStronglyTyped.csproj is present, and execute:

```dotnetcli
//-o is to define the output, we are choose the current folder
dotnet pack -o .
```

Congrats!! You must have `\AppSettingStronglyTyped\AppSettingStronglyTyped\AppSettingStronglyTyped.1.0.0.nupkg` generated.

.nupkg files are a zip file. You can open with a zip tool. On build folder the .target and .props files must be present. On lib\netstandard2.0\ folder the .dll file mus be present. On the root must be AppSettingStronglyTyped.nuspec file.

### Step 6, Generate console app to use the new MSBuild task

Create a standard .Net Core console app.
We called MSBuildConsoleExample.

Import the AppSettingStronglyTyped nuget.

rebuild to be sure every thing is ok

create on the root MyValues.mysettings

Greeting:string:Hello World!

rebuild

check the generated class MySetting.generated.cs

re write the namespace, open csproj and add (your namespace)

```
	<PropertyGroup>
		<SettingNamespace>MSBuildConsoleExample</SettingNamespace>
	</PropertyGroup>
```

rebuild

go to the program and change the console log

Console.WriteLine(MySetting.Greeting);

See that you compiled class works.

-------you can try changing the default class name

---------read binary log
