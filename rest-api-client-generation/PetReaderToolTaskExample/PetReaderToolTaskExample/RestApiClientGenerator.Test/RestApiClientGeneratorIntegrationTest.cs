using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RestApiClientGenerator.Test
{
    [TestClass]
    public class RestApiClientGeneratorIntegrationTest
    {
        public const string MSBUILD = "C:\\Program Files\\dotnet\\dotnet.exe";

        [TestMethod]
        public void executeSuccessBuildFromFile()
        {
            //Arrage
            var output = new List<string>();
            Process buildProcess = new Process();
            buildProcess.StartInfo.FileName = MSBUILD;
            buildProcess.StartInfo.Arguments = "build .\\Resources\\testscript-success.msbuild /t:generatePetClient";
            buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.RedirectStandardOutput = true;

            //act
            buildProcess.Start();
            while (!buildProcess.StandardOutput.EndOfStream)
            {
                output.Add(buildProcess.StandardOutput.ReadLine() ?? string.Empty);
            }           
            buildProcess.WaitForExit();

            //Assert
            Assert.AreEqual(0, buildProcess.ExitCode);
            buildProcess.Close();
            Assert.IsTrue(File.Exists($"PetRestApiClientSuccess.cs"));
        }

        [TestMethod]
        public void executeFailValidationBuildFromFile()
        {
            //Arrage
            var output = new List<string>();
            Process buildProcess = new Process();
            buildProcess.StartInfo.FileName = MSBUILD;
            buildProcess.StartInfo.Arguments = "build .\\Resources\\testscript-fail.msbuild /t:generatePetClient";
            buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.RedirectStandardOutput = true;

            //Act
            buildProcess.Start();
            while (!buildProcess.StandardOutput.EndOfStream)
            {
                output.Add(buildProcess.StandardOutput.ReadLine() ?? string.Empty);
            }
            buildProcess.WaitForExit();

            //Assert
            Assert.AreEqual(1, buildProcess.ExitCode);
            buildProcess.Close();
            Assert.IsFalse(File.Exists($"PetRestApiClientFail.cs"));
            Assert.IsTrue(output.Any(line => line.Contains("URL is not allowed")));
        }
    }
}
