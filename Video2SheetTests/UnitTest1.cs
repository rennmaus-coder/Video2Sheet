using Microsoft.VisualStudio.TestTools.UnitTesting;
using Video2Sheet.Core.Video.Processing.Detection;

namespace Video2SheetTests
{

    [TestClass]
    public class KeyDetectionTests
    {
        private const int WhiteWidth = 40;
        private const int BlackWidth = 20;

        [TestMethod]
        public void GetKeysByTransform_WithWhiteKeyWithinTolerance_ReturnsCorrectIndex()
        {
            WidthDetector.WhiteWidth = WhiteWidth;
            WidthDetector.BlackWidth = BlackWidth;
            // Arrange
            int x = 60; // position of white key
            int width = WhiteWidth; // width of white key
            List<int> expectedKeys = new List<int>() { 1 }; // expected key index

            // Act
            List<int> keys = WidthDetector.GetKeysByTransform(x, width);

            // Assert
            Assert.IsTrue(expectedKeys.Count == keys.Count);
            foreach (int key in expectedKeys)
            {
                Assert.AreEqual(key, keys[expectedKeys.IndexOf(key)]);
            }
        }

        [TestMethod]
        public void GetKeysByTransform_WithBlackKeyWithinTolerance_ReturnsCorrectIndex()
        {
            WidthDetector.WhiteWidth = WhiteWidth;
            WidthDetector.BlackWidth = BlackWidth;
            // Arrange
            int x = 90; // position of black key
            int width = BlackWidth; // width of black key
            List<int> expectedKeys = new List<int>() { 2 }; // expected key index

            // Act
            List<int> keys = WidthDetector.GetKeysByTransform(x, width);

            // Assert
            Assert.IsTrue(expectedKeys.Count == keys.Count);
            foreach (int key in expectedKeys)
            {
                Assert.AreEqual(key, keys[expectedKeys.IndexOf(key)]);
            }
        }

        [TestMethod]
        public void GetKeysByTransform_WithWidthOutsideTolerance_ReturnsEmptyList()
        {
            WidthDetector.WhiteWidth = WhiteWidth;
            WidthDetector.BlackWidth = BlackWidth;
            // Arrange
            int x = 60; // position of white key
            int width = WhiteWidth + 10; // width outside tolerance
            List<int> expectedKeys = new List<int>(); // expected empty list

            // Act
            List<int> keys = WidthDetector.GetKeysByTransform(x, width);

            // Assert
            Assert.IsTrue(keys.Count == 0);
            
        }
    }
}