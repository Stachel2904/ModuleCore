using System;
using UnityEngine;

namespace DivineSkies.Modules.Core
{
    public abstract class BootstrapBase<TSceneName, TModuleHolder> : MonoBehaviour where TSceneName : struct, Enum where TModuleHolder : ModuleHolder<TSceneName>, new()
    {
        protected abstract TSceneName? StartScene { get; }

        private void Awake()
        {
            ModuleController controller = ModuleController.Create(new TModuleHolder());
            controller.InitializeConstantModules(OnConstantModulesInitialized);
        }

        private void OnConstantModulesInitialized()
        {
            if (StartScene == null)
            {
                OnStarted();
            }
            else
            {
                ModuleController.OnSceneChanged += AfterSceneLoaded;
                ModuleController.LoadScene(StartScene);
            }
        }

        private void AfterSceneLoaded()
        {
            ModuleController.OnSceneChanged -= AfterSceneLoaded;
            OnStarted();
        }

        protected virtual void OnStarted()
        {

        }
    }
}