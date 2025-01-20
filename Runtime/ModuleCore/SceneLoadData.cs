using System;

namespace DivineSkies.Modules.Core
{
    public abstract class SceneLoadData
    {
        public abstract Type ModuleType { get; }
    }
}

namespace DivineSkies.Modules
{
    public abstract class SceneLoadData<TModule> : Core.SceneLoadData where TModule : Core.ModuleBase
    {
        public override Type ModuleType => typeof(TModule);
    }
}