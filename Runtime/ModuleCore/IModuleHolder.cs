using System;
using System.Collections.Generic;
using System.Linq;

namespace DivineSkies.Modules.Core
{
    public abstract class ModuleHolderBase
    {
        public abstract Type[] GetConstantModuleTypes();

        public abstract Type[] GetSceneModuleTypes(string scene);
    }

    public abstract class ModuleHolder<TSceneName> : ModuleHolderBase where TSceneName : struct, Enum
    {
        protected abstract Type[] ConstantModules { get; }
        private Type[] _constantModules;

        public sealed override Type[] GetConstantModuleTypes()
        {
            if (_constantModules != null)
            {
                return _constantModules;
            }

            List<Type> constantModules = new List<Type>(ConstantModules);
            constantModules.Add(typeof(Logging.Log));
            _constantModules = constantModules.Distinct().ToArray();

            return _constantModules;
        }

        public sealed override Type[] GetSceneModuleTypes(string scene)
        {
            if(!Enum.TryParse(scene, out TSceneName sceneName))
            {
                this.PrintError("Failed to find Scene named " + scene);
                return Array.Empty<Type>();
            }

            return GetSceneModuleTypes(sceneName);
        }

        protected abstract Type[] GetSceneModuleTypes(TSceneName scene);
    }
}
