using System;
using UnityEngine;

namespace DivineSkies.Modules.Core
{
    public abstract class BootstrapBase<TSceneName, TModuleHolder> : MonoBehaviour where TSceneName : struct, Enum where TModuleHolder : ModuleHolder<TSceneName>, new()
    {
        protected virtual TSceneName? StartScene => null;

        private void Awake()
        {
            ModuleController controller = ModuleController.Create(new TModuleHolder());

            if (StartScene.HasValue)
            {
                controller.SetDefaultScene(StartScene.ToString());
            }

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
                ModuleController.LoadDefaultScene();
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