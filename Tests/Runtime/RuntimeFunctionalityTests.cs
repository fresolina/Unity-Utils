using System.Collections;
using System.Reflection;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using Lotec.Utils.Extensions;
using Lotec.Utils.Pools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lotec.Utils.Tests {
    /// <summary>
    /// PlayMode tests that validate runtime functionality
    /// These tests run in Play mode and can test actual gameplay behavior
    /// </summary>
    public class RuntimeFunctionalityTests {
        private GameObject testGameObject;

        [SetUp]
        public void SetUp() {
            // Create a test GameObject for each test
            testGameObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown() {
            // Clean up after each test
            if (testGameObject != null) {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void MonoBehaviour2CanBeAdded() {
            // Test that MonoBehaviour2 can be added as a component
            // Expect the LogError from NotNull validation since the field is null
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestMonoBehaviour2.*notNullField.*missing"));

            var component = testGameObject.AddComponent<TestMonoBehaviour2>();
            Assert.IsNotNull(component, "MonoBehaviour2 component should be addable");
            Assert.IsInstanceOf<MonoBehaviour2>(component, "Component should be instance of MonoBehaviour2");
            Assert.IsInstanceOf<MonoBehaviour>(component, "Component should be instance of MonoBehaviour");
        }

        [UnityTest]
        public IEnumerator MonoBehaviour2LifecycleWorks() {
            // Test that MonoBehaviour2 lifecycle methods work properly
            // Expect the LogError from NotNull validation since the field is null
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestMonoBehaviour2.*notNullField.*missing"));

            var component = testGameObject.AddComponent<TestMonoBehaviour2>();

            // Wait one frame for Start to be called
            yield return null;

            Assert.IsTrue(component.WasStartCalled, "Start should be called on MonoBehaviour2");
            Assert.IsTrue(component.WasAwakeCalled, "Awake should be called on MonoBehaviour2");
        }

        [Test]
        public void ComponentExtensionsWork() {
            // Test that extension methods work properly
            var hasTransform = testGameObject.HasComponent<Transform>();
            Assert.IsTrue(hasTransform, "GameObject should have Transform component");

            var hasRigidbody = testGameObject.HasComponent<Rigidbody>();
            Assert.IsFalse(hasRigidbody, "GameObject should not have Rigidbody component initially");

            // Add Rigidbody and test again
            testGameObject.AddComponent<Rigidbody>();
            hasRigidbody = testGameObject.HasComponent<Rigidbody>();
            Assert.IsTrue(hasRigidbody, "GameObject should have Rigidbody component after adding it");
        }

        [Test]
        public void AttributesCanBeApplied() {
            // Test that our custom attributes can be applied to fields
            // Expect the LogError from NotNull validation since the field is null
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestMonoBehaviour2.*notNullField.*missing"));

            var component = testGameObject.AddComponent<TestMonoBehaviour2>();
            var fieldInfo = typeof(TestMonoBehaviour2).GetField("notNullField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(fieldInfo, "Test field should exist");

            var notNullAttribute = fieldInfo.GetCustomAttribute<NotNullAttribute>();
            Assert.IsNotNull(notNullAttribute, "NotNull attribute should be present on test field");
        }

        [Test]
        public void NotNullAttributeValidationWorks() {
            // Create a separate test to verify NotNull validation works correctly
            var testObj = new GameObject("NotNullTest");

            // Expect the first LogError from AddComponent (OnValidate is called automatically)
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestMonoBehaviour2.*notNullField.*missing"));

            var component = testObj.AddComponent<TestMonoBehaviour2>();

            // Expect the second LogError from manual OnValidate call
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestMonoBehaviour2.*notNullField.*missing"));

            // Manually trigger validation to test the NotNull functionality
            component.GetType().GetMethod("OnValidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(component, null);

            // Now assign a valid GameObject and verify no error occurs
            var validObject = new GameObject("ValidObject");
            var fieldInfo = typeof(TestMonoBehaviour2).GetField("notNullField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(component, validObject);

            // This validation should not produce any errors
            component.GetType().GetMethod("OnValidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(component, null);

            // Clean up
            Object.DestroyImmediate(validObject);
            Object.DestroyImmediate(testObj);
        }

        [UnityTest]
        public IEnumerator PoolManagerWorks() {
            // Test PoolManager functionality if it exists
            var poolManager = testGameObject.AddComponent<PoolManager>();
            Assert.IsNotNull(poolManager, "PoolManager should be addable");

            yield return null; // Wait one frame

            // Test that PoolManager initializes properly
            Assert.IsNotNull(poolManager, "PoolManager should still exist after one frame");
        }

        [Test]
        public void AimerCanBeInstantiated() {
            // Test Aimer functionality - it's a generic class, not a MonoBehaviour
            var aimer = new Aimer<GameObject>();
            Assert.IsNotNull(aimer, "Aimer should be instantiable");

            // Test that Aimer has the expected properties
            Assert.IsNull(aimer.ItemInWorld, "ItemInWorld should be null initially");
            Assert.IsNull(aimer.PreviousItemInWorld, "PreviousItemInWorld should be null initially");
        }
    }

    /// <summary>
    /// Test implementation of MonoBehaviour2 for testing purposes
    /// </summary>
    public class TestMonoBehaviour2 : MonoBehaviour2 {
        [NotNull]
        [SerializeField] private GameObject notNullField;

        [Expandable]
        public ScriptableObject expandableField;

        [Options(nameof(GetStringOptions))]
        public string optionsField;

        public bool WasStartCalled { get; private set; }
        public bool WasAwakeCalled { get; private set; }

        protected void Awake() {
            WasAwakeCalled = true;
        }

        void Start() {
            WasStartCalled = true;
            Debug.Log("TestMonoBehaviour2 Start called");
        }

        void Update() {
            // Test that Update can be called
        }

        // Method for OptionsAttribute
        public string[] GetStringOptions() {
            return new string[] { "Option1", "Option2", "Option3" };
        }
    }
}
