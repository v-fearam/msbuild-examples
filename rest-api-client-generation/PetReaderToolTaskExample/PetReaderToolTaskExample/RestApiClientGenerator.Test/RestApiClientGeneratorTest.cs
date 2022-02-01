using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestApiClientGenerator.Test
{
    [TestClass]
    public class RestApiClientGeneratorTest
    {
        [TestMethod]
        public void SpecAsFile_GeneratesTheClient()
        {
            //Arrange
            var restApiClientGenerator = new RestApiClientGenerator
            {
                InputOpenApiSpec = ".\\Resources\\petshop-openapi-spec.json",
                ClientClassName = "MyClient",
                ClientNamespaceName = "MyNamespace",
                FolderClientClass = ".",
                NSwagCommandFullPath = "C:\\Users\\far\\Downloads\\Win"
            };
            var buildEngine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e)); ;
            restApiClientGenerator.BuildEngine = buildEngine.Object;

            //act
            var result = restApiClientGenerator.Execute();

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(errors.Count, 0);
            Assert.IsTrue(File.Exists($"{restApiClientGenerator.FolderClientClass}\\{restApiClientGenerator.ClientClassName}.cs"));
        }

        [TestMethod]
        public void SpecAsFile_BadFormat_ClientNotGenerated()
        {
            //Arrange
            var restApiClientGenerator = new RestApiClientGenerator
            {
                InputOpenApiSpec = ".\\Resources\\bad-spec.json",
                ClientClassName = "BadSpec",
                ClientNamespaceName = "MyNamespace",
                FolderClientClass = ".",
                NSwagCommandFullPath = "C:\\Users\\far\\Downloads\\Win"
            };
            var buildEngine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e)); ;
            restApiClientGenerator.BuildEngine = buildEngine.Object;

            //act
            var result = restApiClientGenerator.Execute();

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(errors.Count, 1);
            Assert.IsFalse(File.Exists($"{restApiClientGenerator.FolderClientClass}\\{restApiClientGenerator.ClientClassName}.cs"));
            Assert.AreEqual("\"RestApiClientGenerator\" exited with code - 1.", errors.First().Message);   
        }

        [TestMethod]
        public void SpecAsURL_ClientNotGenerated()
        {
            //Arrange
            var restApiClientGenerator = new RestApiClientGenerator
            {
                InputOpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                ClientClassName = "ClientNotGenerated",
                ClientNamespaceName = "MyNamespace",
                FolderClientClass = ".",
                NSwagCommandFullPath = "C:\\Users\\far\\Downloads\\Win"
            };
            var buildEngine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e)); ;
            restApiClientGenerator.BuildEngine = buildEngine.Object;

            //act
            var result = restApiClientGenerator.Execute();

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(File.Exists($"{restApiClientGenerator.FolderClientClass}\\{restApiClientGenerator.ClientClassName}.cs"));
            Assert.AreEqual(errors.Count, 1);
            Assert.AreEqual("URL is not allowed", errors.First().Message);
        }
    }
}