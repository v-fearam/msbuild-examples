using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AppSettingStronglyTyped.Test
{
    [TestClass]
    public class AppSettingStronglyTypedIntegrationTest
    {
        public const string MSBUILD = "C:\\Program Files\\dotnet\\dotnet.exe";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Process buildProcess;
        private List<string> output;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize()]
        public void Startup()
        {
            output = new List<string>();
            buildProcess = new Process();
            buildProcess.StartInfo.FileName = MSBUILD;
            buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            buildProcess.StartInfo.CreateNoWindow = true;
            buildProcess.StartInfo.RedirectStandardOutput = true;
        }

        [TestCleanup()]
        public void Cleanup()
        {
            buildProcess.Close();
        }

        [TestMethod]
        public void ValidSettingTextFile_SettingClassGenerated()
        {
            //Arrage
            buildProcess.StartInfo.Arguments = "build .\\Resources\\testscript-success.msbuild /t:generateSettingClass";

            //Act
            ExecuteCommandAndCollectResults();

            //Assert
            Assert.AreEqual(0, buildProcess.ExitCode);
            Assert.IsTrue(File.Exists(".\\Resources\\MySettingSuccess.generated.cs"));
            Assert.IsTrue(File.ReadLines(".\\Resources\\MySettingSuccess.generated.cs").SequenceEqual(File.ReadLines(".\\Resources\\testscript-success-class.txt")));

            //creanup
            File.Delete(".\\Resources\\MySettingSuccess.generated.cs");
        }

        [TestMethod]
        public void NotValidSettingTextFile_SettingClassNotGenerated()
        {
            //Arrage
            buildProcess.StartInfo.Arguments = "build .\\Resources\\testscript-fail.msbuild /t:generateSettingClass";

            //Act
            ExecuteCommandAndCollectResults();

            //Assert
            Assert.AreEqual(1, buildProcess.ExitCode);
            Assert.IsFalse(File.Exists(".\\Resources\\MySettingFail.generated.cs"));
            Assert.IsTrue(output.Any(line => line.Contains("Type not supported -> car")));
        }

        private void ExecuteCommandAndCollectResults()
        {
            buildProcess.Start();
            while (!buildProcess.StandardOutput.EndOfStream)
            {
                output.Add(buildProcess.StandardOutput.ReadLine() ?? string.Empty);
            }
            buildProcess.WaitForExit();
        }
    }
}
