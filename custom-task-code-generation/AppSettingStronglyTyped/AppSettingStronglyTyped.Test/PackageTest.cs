using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppSettingStronglyTyped.Test
{
    [TestClass]
    public class PackageTest : MSBuildTestBase
    {
        [TestMethod]
        public void test()
        {
            using (PackageFeed.Create(".")
            .Package("AppSettingStronglyTyped", "1.0.0", out Package package)
                    .Library("netstandard2.0"))
            {

                ProjectCreator projectCreator = ProjectCreator.Create("test.csproj")
                    .Sdk("Microsoft.NET.Sdk")
                    .Property("TargetFramework", "net6.0")
                    .ItemPackageReference(package)
                 .Save(".\\PetRestApiClientSuccessFluent.msbuild");

                //Act
                projectCreator.TryBuild(out bool success, out BuildOutput log);
            }
        }
    }
}
