using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Kernel.Tests.EditMode
{
    public class ProjectContextTests
    {
        private const string ProjectContextPath = "Assets/Game/Kernel/Resources/ProjectContext.prefab";

        private bool IsGameAssembly(string assemblyName)
        {
            if (assemblyName.Contains(".Tests") == true)
            {
                return false;
            }

            return assemblyName == "Kernel" || assemblyName.StartsWith("Features.") == true;
        }

        [Test]
        public void EveryGameInstaller_IsRegistered()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPath);
            var projectContext = prefab.GetComponent<ProjectContext>();

            var registered = projectContext.InstallerPrefabs
                .SelectMany(installer => installer.gameObject.GetComponents<MonoInstaller>())
                .Select(installer => installer.GetType())
                .ToList();

            var expected = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => IsGameAssembly(assembly.GetName().Name) == true)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsAbstract == false)
                .Where(type => typeof(MonoInstaller).IsAssignableFrom(type) == true)
                .ToList();

            CollectionAssert.IsNotEmpty(expected);
            CollectionAssert.IsSubsetOf(expected, registered);
        }
    }
}
