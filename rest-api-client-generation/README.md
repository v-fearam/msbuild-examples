# Rest Api Client Generation

Nowadays, an application which consumes RestApi is a very common scenario. We are going to generate the Rest API client automatically during build process. We will use [NSawg](https://docs.microsoft.com/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-6.0&tabs=visual-studio)

Our app will be a console app, because it is simpler. The kind of app which use the Rest Api client is not important.  
The example will consume the public [Pet Store API](https://petstore.swagger.io), which publish the [OpenAPI spec](https://petstore.swagger.io/v2/swagger.json)

If you are not clear on terms such as tasks, targets, properties, or runtimes, you could first check out the docs that explain these concepts, starting with the [MSBuild Concepts article](https://docs.microsoft.com/visualstudio/msbuild/msbuild-concepts).

## Option 1: Use pre defined MSBuid Exec Task

We will use the ["Exec" MSBuild task](https://docs.microsoft.com/dotnet/api/microsoft.build.tasks.exec?view=msbuild-17-netcore), which simply invokes the specified process with the specified arguments, waits for it to complete, and then returns True if the process completed successfully, and False if an error occurred.

NSwag code generation is possible to be used from MSBuild, by [NSwag.MSBuild](https://github.com/RicoSuter/NSwag/wiki/NSwag.MSBuild)

The complete code version is in this PetReaderExecTaskExample folder, you can download and take a look. Anyway, we are going to go through step by step and explain some concept on the way.

- We are going to create a new console application on Visual Studio named PetReaderExecTaskExample. We use Net5.
- Create another project in the same solution: PetShopRestClient (This is going to contain the generated client as a Library). We use netstandard 2.1. The generated client doesn't compile on netstandard 2.0.
- Go to the PetReaderExecTaskExample solution, and add a project dependence to PetShopRestClient project.
- On PetShopRestClient, include the following nuget packages
  - Nswag.MSBuild, it will allow us access to the code generator from MSBuild
  - Newtonsoft.Json, it will be needed to compile the generated client
  - System.ComponentModel.Annotations, it will be needed to compile the generated client
- On PetShopRestClient, add a folder (named PetShopRestClient) for the code generation and delete the Class1.cs automatically generated.
- Create a text file named petshop-openapi-spec.json (on root). We are going to add the OpenApi spec, please copy the content from [here](https://petstore.swagger.io/v2/swagger.json) inside the file. Why did we commit the spec instead of read it online?, we like repetitive build and depending only from the input, consuming directly the api could transform a build which works today to a build which fails tomorrow from the same source. The picture saved on petshop-openapi-spec.json will allow us to still have a version which builds even if the spec changed.
- Now, the most important part. We are going to modify PetShopRestClient.csproj and add a [MSBuild targets](https://docs.microsoft.com/visualstudio/msbuild/msbuild-targets?view=vs-2022) to generate the client during build process.

  - First, we are going to add some props useful for our client generation

    ```xml
    	<PropertyGroup>
    		<PetOpenApiSpecLocation>petshop-openapi-spec.json</PetOpenApiSpecLocation>
    		<PetClientClassName>PetShopRestClient</PetClientClassName>
    		<PetClientNamespace>PetShopRestClient</PetClientNamespace>
    		<PetClientOutputDirectory>PetShopRestClient</PetClientOutputDirectory>
    	</PropertyGroup>
    ```

  - Please add the following targets:
    ```xml
    <Target Name="generatePetClient" BeforeTargets="CoreCompile" Inputs="$(PetOpenApiSpecLocation)" Outputs="$(PetClientOutputDirectory)\$(PetClientClassName).cs">
    	 <Exec Command="$(NSwagExe) openapi2csclient /input:$(PetOpenApiSpecLocation)  /classname:$(PetClientClassName) /namespace:$(PetClientNamespace) /output:$(PetClientOutputDirectory)\$(PetClientClassName).cs" ConsoleToMSBuild="true">
    			<Output TaskParameter="ConsoleOutput" PropertyName="OutputOfExec" />
      </Exec>
    </Target>
    <Target Name="forceReGenerationOnRebuild" AfterTargets="CoreClean">
    	<Delete Files="$(PetClientOutputDirectory)\$(PetClientClassName).cs"></Delete>
    </Target>
    ```
    You can notice we are using [BeforeTarget and AfterTarget](https://docs.microsoft.com/visualstudio/msbuild/target-build-order?view=vs-2022#beforetargets-and-aftertargets) as way to define build order.  
    The first target called "generatePetClient" will be executed before the core compilation target, so we will create the source before the compiler executes. The input and output parameter are related to [Incremental Build](https://docs.microsoft.com/visualstudio/msbuild/how-to-build-incrementally?view=vs-2022). MSBuild can compare the timestamps of the input files with the timestamps of the output files and determine whether to skip, build, or partially rebuild a target.  
    After installing the NSwag.MSBuild NuGet package in your project, you can use the variable $(NSwagExe) in your .csproj file to run the NSwag command line tool in an MSBuild target. This way the tools can easily be updated via NuGet. Here we are using the _Exec MSBUild Task_ to execute the NSwag program with the required parameters to generate the client Rest Api. [More about Nsawg command and parameters](https://github.com/RicoSuter/NSwag/wiki/NSwag.MSBuild).  
    You can capture output from `<Exec>` addig ConsoleToMsBuild="true" to your `<Exec>` tag and then capturing the output using the ConsoleOutput parameter in an `<Output>` tag. ConsoleOutput returns the output as an Item. Whitespace are trimmed. ConsoleOutput is enabled when ConsoleToMSBuild is true.  
    The second target called "forceReGenerationOnRebuild" deletes the generated class during clean up to force the re generation on rebuild target execution. This target runs after core clean msbuild pre defined target.

- At this point in time we can execute a Visual Studio Solution rebuild and see the client generated on the PetShopRestClient folder.
- We are going to use the generated client. Go to the use it on the client Program.cs and copy the following code

  ```c#
  using System;
  using System.Net.Http;

  namespace PetReaderExecTaskExample
  {
      internal class Program
      {
          private const string baseUrl = "https://petstore.swagger.io/v2";
          static void Main(string[] args)
          {
              HttpClient httpClient = new HttpClient();
              httpClient.BaseAddress = new Uri(baseUrl);
              var petClient = new PetShopRestClient.PetShopRestClient(httpClient);
              var pet = petClient.GetPetByIdAsync(1).Result;
              Console.WriteLine($"Id: {pet.Id} Name: {pet.Name} Status: {pet.Status} CategoryName: {pet.Category.Name}");
          }
      }
  }
  ```

  _Note:_ We are using `new HttpClient()` because is simple to show our example working, but it is not appropriated. The best practice is to use HTTPClientFactory to create an HTTPClient object which addresses the known issues of HTTPClient request like Resource Exhaustion or Stale DNS problems. [Read more about it](https://docs.microsoft.com/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)

- Congrats!! Now, you can execute the program to see how it working

## Option 2: Use the Custom task derived from MSBuid Tool Task

In many cases the option 1 is good enough to execute external tools to do something, like Rest Api Client Code Generation.
We are going to continue with the same example, but the ideas can be used for others examples.  
What if we want to allow Rest Api Client Code Generation if only if we don't use absolute Windows path as input? Or What if we need to calculate in some way where is the executable dynamically?.  
When there is any situation where we need execute some code to do extra work, the [MSBuild Tool Task](https://docs.microsoft.com/dotnet/api/microsoft.build.utilities.tooltask) is the best solution. This is an abstract class derivated from MSBuild Task, we need to define a concrete subclass (_We will need to create a Custom MSBuild Task_). It is prepare for command execution and allows us to introduce code during the process.
