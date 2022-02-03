# How to test a MSBuild Custom Task

A really important item when we create a MSBuild Custom Task, which is going to be distributed, is to ensure the correctness.  
The way to be confident about that is testing it.  
It is out to scope talking about benefit of doing test and basic test tooling. Here some [basic about unit test](https://docs.microsoft.com/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code?view=vs-2022).  
We are going to use examples which already have been developed. The following projects includes unit and integration MSBuild Custom Tasks testing

1. [Custom Task-Code Generation](./custom-task-code-generation/)
1. [The Rest-Api client Generation - Option 2 - MSBuild Tool Task](./rest-api-client-generation/)

## Unit Test

A MSBuild Custom Task is a class which inherit from MSBuild Task (directly or indirectly, because MSBuild Tool Task is a MSBuild Task). The method which generates the action is `Execute()`.  
We have some input values (parameters), and output parameters which we will be able to assert.  
In our case some input parameter are path to files, so we generated test input files on a folder called _Resources_. Our MSBuild task also generates files, so we are going to assert the generated files.

:white_check_mark: A build engine is needed, a class which implements [IBuildEngine](https://docs.microsoft.com/dotnet/api/microsoft.build.framework.ibuildengine?view=msbuild-17-netcore). In our example we created a mock using [Moq](https://github.com/Moq/moq4/wiki/Quickstart), but you can use other mock tool. I was interesting on collecting the errors, but you can collect another information and then assert it.  
The Engine Mock is needed on all the tests, so it was included as _TestInitialize_ (it is going to be executed before each test, and each test will have its own build engine). [Complete example](.\custom-task-code-generation\AppSettingStronglyTyped\AppSettingStronglyTyped.Test\AppSettingStronglyTypedTest.cs)

```c#
       private Mock<IBuildEngine> buildEngine;
       private List<BuildErrorEventArgs> errors;

        [TestInitialize()]
        public void Startup()
        {
            buildEngine = new Mock<IBuildEngine>();
            errors = new List<BuildErrorEventArgs>();
            buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e));
        }

```

Now we need to create our Task and set the parameters as part of the test arrangement.

```csharp
   //arrange
   var item = new Mock<ITaskItem>();
   item.Setup(x => x.GetMetadata("Identity")).Returns($".\\Resources\\complete-prop.setting");

   var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "MyCompletePropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };

   appSettingStronglyTyped.BuildEngine = buildEngine.Object;
```

First, we create the ITaskItem parameter mock (using [Moq](https://github.com/Moq/moq4/wiki/Quickstart)), and point to the file to be parsed. Then, we create our _AppSettingStronglyTyped_ Custom Task with its parameters. Finally, we set the build engine to our MSBuild Custom Task.

At this point we need to do the action

```csharp
    //act
    var success = appSettingStronglyTyped.Execute();
```

Last but not least, we need to assert the expected outcome from our test

```csharp
   //assert
   Assert.IsTrue(success); // The execution was success
   Assert.AreEqual(errors.Count, 0); //Not error were found
   Assert.AreEqual($"MyCompletePropSetting.generated.cs", appSettingStronglyTyped.ClassNameFile); // The Task expected output
   Assert.AreEqual(true, File.Exists(appSettingStronglyTyped.ClassNameFile)); // The file was generated
   Assert.IsTrue(File.ReadLines(appSettingStronglyTyped.ClassNameFile).SequenceEqual(File.ReadLines(".\\Resources\\complete-prop-class.txt"))); // Assenting the file content
```

Following this pattern you should expand all the possibilities.
:warning: When there are file generation we need to use different file name for each test to avoid collision. Remember delete the generated files as test cleanup.

## Integration Test
