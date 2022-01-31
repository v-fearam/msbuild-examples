using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Linq;

namespace AppSettingStronglyTyped.Test
{
    [TestClass]
    public class AppSettingStronglyTypedTest
    {
        [TestMethod]
        public void EmptySettingFileList_EmptyClassGenerated()
        {
            //arrange
            var appSettingStronglyTyped = new AppSettingStronglyTyped { SettingClassName = "MySettingEmpty", SettingNamespaceName = "MyNamespace", SettingFiles = new ITaskItem[0] };

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
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

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsFalse(success);
            Assert.AreEqual(null, appSettingStronglyTyped.ClassNameFile);
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

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
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

            //act
            var success = appSettingStronglyTyped.Execute();

            //assert
            Assert.IsTrue(success);
            Assert.AreEqual($"MyCompletePropSetting.generated.cs", appSettingStronglyTyped.ClassNameFile);
            Assert.AreEqual(true, File.Exists(appSettingStronglyTyped.ClassNameFile));
            Assert.IsTrue(File.ReadLines(appSettingStronglyTyped.ClassNameFile).SequenceEqual(File.ReadLines($".\\Resources\\complete-prop-class.txt")));
        }

    }
}