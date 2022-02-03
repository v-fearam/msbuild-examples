using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppSettingStronglyTyped.Test
{
    [TestClass]
    public class AppSettingStronglyTypedIntegrationFluentTest : MSBuildTestBase
    {
        //https://github.com/jeffkl/MSBuildProjectCreator
        [TestMethod]
        public void executeSuccessFluent_SettingGenerated()
        {
            //Arrange
            ProjectCreator creator = ProjectCreator.Create("test.proj")
             .Sdk("Microsoft.NET.Sdk")
             .UsingTaskAssemblyFile("AppSettingStronglyTyped.AppSettingStronglyTyped", "AppSettingStronglyTyped.dll")
             .Property("SettingClass", "SettingSuccessFluent")
             .Property("SettingNamespace", "SettingSuccessFluent")
             .ItemInclude("SettingFiles", ".\\Resources\\complete-prop.setting")
             .Target(name: "generateSettingClass", beforeTargets: "CoreCompile")
             .Task(
                 name: "AppSettingStronglyTyped",
                 parameters: new Dictionary<string, string>
                 {
                    { "SettingClassName", "$(SettingClass)" },
                    { "SettingNamespaceName", "$(SettingNamespace)" },
                    { "SettingFiles", "@(SettingFiles)" }
                 })
             .Save(".\\PetRestApiClientSuccessFluent.msbuild");

            //Act
            creator.TryBuild(target: "generateSettingClass", out bool success, out BuildOutput log);

            //Assert
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists("SettingSuccessFluent.generated.cs"));
            Assert.IsTrue(File.ReadLines(".\\SettingSuccessFluent.generated.cs").SequenceEqual(File.ReadLines(".\\Resources\\success-fluent-success-class.txt")));

            //creanup
            File.Delete("SettingSuccessFluent.generated.cs");
        }

        [TestMethod]
        public void executeSuccessFail_SettingGenerated()
        {
            //Arrange
            ProjectCreator creator = ProjectCreator.Create("test.proj")
             .Sdk("Microsoft.NET.Sdk")
             .UsingTaskAssemblyFile("AppSettingStronglyTyped.AppSettingStronglyTyped", "AppSettingStronglyTyped.dll")
             .Property("SettingClass", "SettingFailFluent")
             .Property("SettingNamespace", "SettingFailFluent")
             .ItemInclude("SettingFiles", ".\\Resources\\notvalidvalue-prop.setting")
             .Target(name: "generateSettingClass", beforeTargets: "CoreCompile")
             .Task(
                 name: "AppSettingStronglyTyped",
                 parameters: new Dictionary<string, string>
                 {
                    { "SettingClassName", "$(SettingClass)" },
                    { "SettingNamespaceName", "$(SettingNamespace)" },
                    { "SettingFiles", "@(SettingFiles)" }
                 })
             .Save(".\\PetRestApiClientFailFluent.msbuild");

            //Act
            creator.TryBuild(target: "generateSettingClass", out bool success, out BuildOutput log);

            //Assert
            Assert.IsFalse(success);
            Assert.IsFalse(File.Exists("SettingFailFluent.generated.cs"));
            Assert.IsTrue(log.Errors.Any(err => err.Contains("It is not possible parse some value based on the type -> bool - awsome")));
        }

    }
}
