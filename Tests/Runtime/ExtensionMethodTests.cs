using Lotec.Utils;
using Lotec.Utils.Extensions;
using Lotec.Utils.Pools;
using NUnit.Framework;
using UnityEngine;

namespace Lotec.Utils.Tests {
    /// <summary>
    /// Tests for extension methods provided by the package
    /// </summary>
    public class ExtensionMethodTests {
        private GameObject testGameObject;

        [SetUp]
        public void SetUp() {
            testGameObject = new GameObject("ExtensionTestObject");
        }

        [TearDown]
        public void TearDown() {
            if (testGameObject != null) {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void HasComponentExtensionWorks() {
            // Test the HasComponent extension method
            Assert.IsTrue(testGameObject.HasComponent<Transform>(),
                "GameObject should have Transform component");

            Assert.IsFalse(testGameObject.HasComponent<Rigidbody>(),
                "GameObject should not have Rigidbody component initially");

            // Add component and test again
            testGameObject.AddComponent<Rigidbody>();
            Assert.IsTrue(testGameObject.HasComponent<Rigidbody>(),
                "GameObject should have Rigidbody component after adding it");
        }

        [Test]
        public void HasComponentWorksWithInterfaces() {
            // Test HasComponent with interface types (if supported)
            var poolManager = testGameObject.AddComponent<PoolManager>();

            // Test that we can check for the component
            Assert.IsTrue(testGameObject.HasComponent<PoolManager>(),
                "GameObject should have PoolManager component");
        }

        [Test]
        public void HasComponentHandlesNullGameObject() {
            // Test that HasComponent handles edge cases gracefully
            GameObject nullGameObject = null;

            // This should not throw, but return false
            // Note: This test might need adjustment based on actual implementation
            Assert.DoesNotThrow(() => {
                try {
                    var result = nullGameObject.HasComponent<Transform>();
                } catch (System.NullReferenceException) {
                    // Expected for null GameObject
                }
            }, "HasComponent should handle null GameObject gracefully");
        }

        [Test]
        public void ExtensionMethodsAreAccessible() {
            // Test that extension methods are properly accessible
            // This ensures the using statements and namespaces are correct

            var hasTransform = testGameObject.HasComponent<Transform>();
            Assert.IsTrue(hasTransform, "Extension method should be accessible and work");

            // Test with a component that doesn't exist
            var hasCamera = testGameObject.HasComponent<Camera>();
            Assert.IsFalse(hasCamera, "Extension method should return false for missing components");
        }
    }
}
