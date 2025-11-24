using System.Collections;
using Lotec.Interactions;
using Lotec.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lotec.Utils.Tests
{
    /// <summary>
    /// Tests for the interaction system components
    /// </summary>
    public class InteractionSystemTests
    {
        private GameObject testGameObject;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("InteractionTestObject");
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        private SimpleInteractionSystem CreateInteractionSystemWithSensor()
        {
            // Add the sensor component first
            var sensor = testGameObject.AddComponent<InteractionSystemSensor>();

            // Add the interaction system
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("_sensor.*missing"));
            var interactionSystem = testGameObject.AddComponent<SimpleInteractionSystem>();

            // Use reflection to set the private _sensor field
            var sensorField = typeof(SimpleInteractionSystem).GetField("_sensor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (sensorField != null)
            {
                sensorField.SetValue(interactionSystem, sensor);
            }

            return interactionSystem;
        }

        [Test]
        public void SimpleInteractionSystemCanBeAdded()
        {
            // Test that SimpleInteractionSystem can be added as a component
            var interactionSystem = CreateInteractionSystemWithSensor();
            Assert.IsNotNull(interactionSystem, "SimpleInteractionSystem should be addable");
            Assert.IsInstanceOf<MonoBehaviour2>(interactionSystem, "SimpleInteractionSystem should inherit from MonoBehaviour2");
        }

        [Test]
        public void InteractionSystemImplementsInterface()
        {
            // Test that SimpleInteractionSystem implements IInteractionSystem
            var interactionSystem = CreateInteractionSystemWithSensor();
            Assert.IsTrue(interactionSystem is IInteractionSystem, "SimpleInteractionSystem should implement IInteractionSystem");
        }

        [Test]
        public void InteractionSystemHasValidInteractionsList()
        {
            // Test that the ValidInteractions list is properly initialized
            var interactionSystem = CreateInteractionSystemWithSensor();
            Assert.IsNotNull(interactionSystem.ValidInteractions, "ValidInteractions list should not be null");
            Assert.AreEqual(0, interactionSystem.ValidInteractions.Count, "ValidInteractions should start empty");
        }

        // TODO: Setup sensor._aimer.Anchor
        // [UnityTest]
        // public IEnumerator InteractionSystemUpdateWorks() {
        //     // Test that UpdateInteractions method works
        //     var interactionSystem = CreateInteractionSystemWithSensor();

        //     yield return null; // Wait one frame

        //     // Test that we can call UpdateInteractions without errors
        //     Assert.DoesNotThrow(() => interactionSystem.UpdateInteractions(), "UpdateInteractions should not throw");
        // }

        [Test]
        public void InteractionSystemCanShowMessage()
        {
            // Test the ShowMessage functionality
            var interactionSystem = CreateInteractionSystemWithSensor();

            // This should not throw an exception
            Assert.DoesNotThrow(() => interactionSystem.ShowMessage(null, "Test message"),
                "ShowMessage should not throw");
        }

        [Test]
        public void InteractionSystemCanCancelInteraction()
        {
            // Test the CancelInteraction functionality
            var interactionSystem = CreateInteractionSystemWithSensor();

            // This should not throw an exception even with no active interaction
            Assert.DoesNotThrow(() => interactionSystem.CancelInteraction(),
                "CancelInteraction should not throw when no interaction is active");
        }

        [Test]
        public void InteractionSystemCanInteractWithValidIndex()
        {
            // Test the Interact method with various indices
            var interactionSystem = CreateInteractionSystemWithSensor();

            // Should not throw even with empty interactions list
            Assert.DoesNotThrow(() => interactionSystem.Interact(0),
                "Interact should not throw with index 0 on empty list");
            Assert.DoesNotThrow(() => interactionSystem.Interact(-1),
                "Interact should not throw with negative index");
            Assert.DoesNotThrow(() => interactionSystem.Interact(100),
                "Interact should not throw with large index");
        }
    }
}
