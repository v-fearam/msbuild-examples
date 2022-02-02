# MSBuild examples

This repo is created to include msbuild examples

1. The first example is the creation of a custom task for code generation. The idea is to consume a txt file and generate code from it during build process. It is simple in order to show the mechanism, then you will be able to create more complex piece of code. Part of this effort includes how to ship and consume the custom task as nuget package.

   [Please see the Custom Task-Code Generation Readme](./custom-task-code-generation/)

1. Generate a Rest Client API during the build process. The example uses [NSawg](https://docs.microsoft.com/aspnet/core/tutorials/getting-started-with-nswag) as client generator (It is also a code generation example). It is a very common scenario. We are going to create two examples

   1. Use the pre defined [MSBuild Exec Task](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.tasks.exec) to do that.
   1. Use the Custom task derived from [MSBuild Tool Task](https://docs.microsoft.com/dotnet/api/microsoft.build.utilities.tooltask) to do that.

   [Please see the Rest-Api client Generation Readme](./rest-api-client-generation/)

1. :construction: :thinking: Define how to test a custom task, unit and integration test

1. :thinking: Investigate some action not related to code generation, for example send an email and used if some situation is detected.
