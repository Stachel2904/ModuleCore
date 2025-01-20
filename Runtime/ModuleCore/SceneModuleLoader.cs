using UnityEngine;

namespace DivineSkies.Modules.Core
{
    public class SceneModuleLoader : MonoBehaviour
    {
        public ModuleBase[] SceneModules => gameObject.GetComponents<ModuleBase>();
    }
}