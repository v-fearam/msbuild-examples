using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppSettingStronglyTyped.Test
{
    [TestClass]
    public class AppSettingStronglyTypedTest
    {
        private Mock<IBuildEngine> buildEngine;
        private List<BuildErrorEventArgs> errors;

        [TestInitialize()]
        public void Startup()
        {
            buildEngine = new Mock<IBuildEngine>();
            errors = new List<BuildErrorEventArgs>();
            buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e)); 
        }

        [TestMethod]
        public void EmptySettingFileList_EmptyClassGenerated()
        {
            //arrange
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "MySettingEmpty", SettingNamespaceName = "MyNamespace", SettingFiles = new ITaskItem[0] };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
            Assert.AreEqual(errors.Count, 0);
            Assert.AreEqual("MySettingEmpty.generated.cs", appSettingStronglyTyped.ClassNameFile);
            Assert.IsTrue(File.Exists(appSettingStronglyTyped.ClassNameFile));
            Assert.IsTrue(File.ReadLines(appSettingStronglyTyped.ClassNameFile).SequenceEqual(File.ReadLines(".\\Resources\\empty-class.txt")));
        }

        [TestMethod]
        public void SettingFileBadFormat_NotSuccess()
        {
            //arrange
            var item = new Mock<ITaskItem>();
            item.Setup(x => x.GetMetadata("Identity")).Returns(".\\Resources\\error-prop.setting");
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "ErrorPropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsFalse(success);
            Assert.AreEqual(errors.Count, 1);
            Assert.AreEqual(null, appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual("Incorrect line format. Valid format prop:type:defaultvalue", errors.First().Message);
        }

        [TestMethod]
        public void SettingInvalidType_NotSuccess()
        {
            //arrange
            var item = new Mock<ITaskItem>();
            item.Setup(x => x.GetMetadata("Identity")).Returns(".\\Resources\\notvalidtype-prop.setting");
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "ErrorPropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsFalse(success);
            Assert.AreEqual(errors.Count, 1);
            Assert.AreEqual(null, appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual("Type not supported -> car", errors.First().Message);
        }

        [TestMethod]
        public void SettingInvalidValue_NotSuccess()
        {
            //arrange
            var item = new Mock<ITaskItem>();
            item.Setup(x => x.GetMetadata("Identity")).Returns(".\\Resources\\notvalidvalue-prop.setting");
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "ErrorPropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsFalse(success);
            Assert.AreEqual(errors.Count, 1);
            Assert.AreEqual(null, appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual("It is not possible parse some value based on the type -> bool - awsome", errors.First().Message);
        }

        [DataTestMethod]
        [DataRow("string")]
        [DataRow("int")]
        [DataRow("bool")]
        [DataRow("guid")]
        [DataRow("long")]
        public void SettingFileWithProperty_ClassGeneratedWithOneProperty(string value)
        {
            //arrange
            var item = new Mock<ITaskItem>();
            item.Setup(x => x.GetMetadata("Identity")).Returns($".\\Resources\\{value}-prop.setting");
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = $"My{value}PropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
            Assert.AreEqual(errors.Count, 0);
            Assert.AreEqual($"My{value}PropSetting.generated.cs", appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual(true, File.Exists(appSettingStronglyTyped.ClassNameFile));
            Assert.IsTrue(File.ReadLines(appSettingStronglyTyped.ClassNameFile).SequenceEqual(File.ReadLines($".\\Resources\\{value}-prop-class.txt")));
        }

        [DataTestMethod]
        public void SettingFileWithMultipleProperty_ClassGeneratedWithMultipleProperty()
        {
            //arrange
            var item = new Mock<ITaskItem>();
            item.Setup(x => x.GetMetadata("Identity")).Returns($".\\Resources\\complete-prop.setting");
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = $"MyCompletePropSetting", SettingNamespaceName = "MyNamespace", SettingFiles = new[] { item.Object } };
            appSettingStronglyTyped.BuildEngine = buildEngine.Object;

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
            Assert.AreEqual(errors.Count, 0);
            Assert.AreEqual($"MyCompletePropSetting.generated.cs", appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual(true, File.Exists(appSettingStronglyTyped.ClassNameFile));
            Assert.IsTrue(File.ReadLines(appSettingStronglyTyped.ClassNameFile).SequenceEqual(File.ReadLines(".\\Resources\\complete-prop-class.txt")));
        }

    }
}