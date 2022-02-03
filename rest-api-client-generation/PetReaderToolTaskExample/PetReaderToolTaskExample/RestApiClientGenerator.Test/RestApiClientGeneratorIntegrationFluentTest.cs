using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestApiClientGenerator.Test
{
    [TestClass]
    public class RestApiClientGeneratorIntegrationFluentTest : MSBuildTestBase
    {
        //https://github.com/jeffkl/MSBuildProjectCreator
        [TestMethod]
        public void executeSuccessClientGeneration()
        {
            //Arrange
            ProjectCreator creator = ProjectCreator.Create("test.proj")
             .Sdk("Microsoft.NET.Sdk")
             .UsingTaskAssemblyFile("RestApiClientGenerator.RestApiClientGenerator", "RestApiClientGenerator.dll")
             .Property("PetClientInputOpenApiSpec", "Resources\\petshop-openapi-spec.json")
             .Property("PetClientClientClassName", "PetRestApiClientSuccessFluent")
             .Property("PetClientClientNamespaceName", "PetRestApiClientSuccessFluent")
             .Property("PetClientFolderClientClass", ".")
             .Property("NSwagCommandFullPath", "C:\\Nwag\\Win")
             .Target(name: "generatePetClient", beforeTargets: "CoreCompile")
             .Task(
                 name: "RestApiClientGenerator",
                 parameters: new Dictionary<string, string>
                 {
                    { "InputOpenApiSpec", "$(PetClientInputOpenApiSpec)" },
                    { "ClientClassName", "$(PetClientClientClassName)" },
                    { "ClientNamespaceName", "$(PetClientClientNamespaceName)" },
                    { "FolderClientClass", "$(PetClientFolderClientClass)" },
                    { "NSwagCommandFullPath", "$(NSwagCommandFullPath)" },
                 })
             .Save(".\\PetRestApiClientSuccessFluent.msbuild");

            //Act
            creator.TryBuild(target: "generatePetClient", out bool success, out BuildOutput log);

            //Assert
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists("PetRestApiClientSuccessFluent.cs"));

            //creanup
            File.Delete("PetRestApiClientSuccessFluent.cs");
        }

        [TestMethod]
        public void executeFailClientGeneration()
        {
            //Arrange
            ProjectCreator creator = ProjectCreator.Create("test.proj")
             .Sdk("Microsoft.NET.Sdk")
             .UsingTaskAssemblyFile("RestApiClientGenerator.RestApiClientGenerator", "RestApiClientGenerator.dll")
             .Property("PetClientInputOpenApiSpec", "https://petstore.swagger.io/v2/swagger.json")
             .Property("PetClientClientClassName", "PetRestApiClientFailFluent")
             .Property("PetClientClientNamespaceName", "PetRestApiClientFailFluent")
             .Property("PetClientFolderClientClass", ".")
             .Property("NSwagCommandFullPath", "C:\\Nwag\\Win")
             .Target(name: "generatePetClient", beforeTargets: "CoreCompile")
             .Task(
                 name: "RestApiClientGenerator",
                 parameters: new Dictionary<string, string>
                 {
                    { "InputOpenApiSpec", "$(PetClientInputOpenApiSpec)" },
                    { "ClientClassName", "$(PetClientClientClassName)" },
                    { "ClientNamespaceName", "$(PetClientClientNamespaceName)" },
                    { "FolderClientClass", "$(PetClientFolderClientClass)" },
                    { "NSwagCommandFullPath", "$(NSwagCommandFullPath)" },
                 })
             .Save(".\\PetRestApiClientFailFluent.msbuild");

            //Act
            creator.TryBuild(target: "generatePetClient", out bool success, out BuildOutput log);

            //Assert
            Assert.IsFalse(success);
            Assert.IsFalse(File.Exists("PetRestApiClientFailFluent.cs"));
            Assert.IsTrue(log.Errors.Any(err => err.Contains("URL is not allowed")));
        }
    }
}
