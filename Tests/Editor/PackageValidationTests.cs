using System.Linq;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using Lotec.Utils.Pools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Lotec.Utils.Tests {
    /// <summary>
    /// EditMode tests that validate package structure, compilation, and editor functionality
    /// These tests run in the Unity Editor without entering Play mode
    /// </summary>
    public class PackageValidationTests {
        [Test]
        public void PackageAssembliesAreLoaded() {
            // Test that our package assemblies are properly loaded
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var lotecAssemblies = assemblies.Where(a => a.FullName.Contains("Lotec.Utils")).ToArray();

            Assert.Greater(lotecAssemblies.Length, 0, "Lotec.Utils assemblies should be loaded");

            // Log found assemblies for debugging
            Debug.Log($"Found {lotecAssemblies.Length} Lotec.Utils assemblies:");
            foreach (var assembly in lotecAssemblies) {
                Debug.Log($"  - {assembly.GetName().Name}");
            }
        }

        [Test]
        public void MonoBehaviour2TypeExists() {
            // Test that MonoBehaviour2 base class exists and is properly defined
            var type = typeof(MonoBehaviour2);
            Assert.IsNotNull(type, "MonoBehaviour2 type should exist");
            Assert.IsTrue(type.IsSubclassOf(typeof(MonoBehaviour)), "MonoBehaviour2 should inherit from MonoBehaviour");
            Assert.IsTrue(type.IsPublic, "MonoBehaviour2 should be public");
        }

        [Test]
        public void AttributesExist() {
            // Test that all our custom attributes exist and are properly defined
            Assert.IsNotNull(typeof(NotNullAttribute), "NotNullAttribute should exist");
            Assert.IsNotNull(typeof(ExpandableAttribute), "ExpandableAttribute should exist");
            Assert.IsNotNull(typeof(OptionsAttribute), "OptionsAttribute should exist");
            Assert.IsNotNull(typeof(ScriptTooltipAttribute), "ScriptTooltipAttribute should exist");

            // Test that attributes inherit from correct base classes
            Assert.IsTrue(typeof(NotNullAttribute).IsSubclassOf(typeof(System.Attribute)), "NotNullAttribute should inherit from Attribute");
            Assert.IsTrue(typeof(ExpandableAttribute).IsSubclassOf(typeof(System.Attribute)), "ExpandableAttribute should inherit from Attribute");
        }

        [Test]
        public void InteractionSystemTypesExist() {
            // Test that interaction system types exist
            var simpleInteractionSystemType = System.Type.GetType("Lotec.Interactions.SimpleInteractionSystem, Lotec.Utils");
            Assert.IsNotNull(simpleInteractionSystemType, "SimpleInteractionSystem should exist");

            var interactionInterface = System.Type.GetType("Lotec.Interactions.IInteraction, Lotec.Utils");
            Assert.IsNotNull(interactionInterface, "IInteraction interface should exist");

            var interactableType = System.Type.GetType("Lotec.Interactions.Interactable, Lotec.Utils");
            Assert.IsNotNull(interactableType, "Interactable should exist");
        }

        [Test]
        public void UtilityTypesExist() {
            // Test that utility types exist
            Assert.IsNotNull(typeof(PoolManager), "PoolManager should exist");
            Assert.IsNotNull(typeof(EmojiIconManager), "EmojiIconManager should exist");
            Assert.IsNotNull(typeof(Aimer<GameObject>), "Aimer should exist");
        }

        [Test]
        public void EditorTypesExist() {
            // Test that editor-specific types exist (only in EditMode tests)
            var monoBehaviour2EditorType = System.Type.GetType("Lotec.Utils.MonoBehaviour2Editor, Lotec.Utils.Editor");
            Assert.IsNotNull(monoBehaviour2EditorType, "MonoBehaviour2Editor should exist");

            var notNullDrawerType = System.Type.GetType("Lotec.Utils.Attributes.Editors.NotNullDrawer, Lotec.Utils.Editor");
            Assert.IsNotNull(notNullDrawerType, "NotNullDrawer should exist");

            var expandableDrawerType = System.Type.GetType("Lotec.Utils.Attributes.ExpandableDrawer, Lotec.Utils.Editor");
            Assert.IsNotNull(expandableDrawerType, "ExpandableDrawer should exist");
        }
    }
}
