using UnityEngine;
using System.Collections;

namespace DivineSkies.Modules.Core
{
    public abstract class ModuleBase : MonoBehaviour
    {
        public virtual int InitPriority => 1;
        public virtual void Register() { }
        public virtual IEnumerator InitializeAsync()
        {
            Initialize();
            yield return null;
        }
        public virtual void Initialize()
        {
            if(this is ISceneModule module)
            {
                module.Visualization?.Initialize();
                this.PrintLog("Initialized " + this);
            }
        }
        public virtual void BeforeUnregister() { }
        public virtual void OnSceneFullyLoaded() { }
    }
}

namespace DivineSkies.Modules
{
    public abstract class ModuleBase<T> : Core.ModuleBase where T: Core.ModuleBase
    {
        public static T Main => ModuleController.Get<T>();
    }
}
