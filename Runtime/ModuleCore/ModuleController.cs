using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DivineSkies.Tools.Extensions;
using DivineSkies.Modules.Core;

namespace DivineSkies.Modules
{
    public class ModuleController : MonoBehaviour
    {
        public static event Action OnSceneChanged;
        public float SceneFadeSpeed { get; set; } = 0.3f;
        public static Transform ConstantUiParent { get; private set; }

        private static IEnumerable<ModuleBase> LoadedModules => _self.constantModules.Concat(_self._sceneModules);

        private static ModuleHolderBase _holder;

        private static ModuleController _self;

        private readonly List<ModuleBase> constantModules = new();
        private readonly List<ModuleBase> _sceneModules = new();
        private readonly List<ModuleBase> _uninitializedModules = new();
        private readonly List<SceneLoadData> sceneLoadData = new();

        private Image _blendMask;

        internal static ModuleController Create(ModuleHolderBase holder)
        {
            if (_self != null)
                return _self;

            _self = new GameObject("Modules").AddComponent<ModuleController>();
            DontDestroyOnLoad(_self.gameObject);

            GameObject overlayRendering = Instantiate(Resources.Load<GameObject>("Prefabs/OverlayRendering"));
            DontDestroyOnLoad(overlayRendering);
            ConstantUiParent = overlayRendering.transform.GetComponentInChildren<Canvas>().transform;
            _self._blendMask = ConstantUiParent.Find("BlendMask").GetComponent<Image>();

            _holder = holder;
            SceneManager.sceneLoaded += _self.AfterSceneLoad;

            return _self;
        }

        internal void InitializeConstantModules(Action callback)
        {
            foreach (var type in _holder.GetConstantModuleTypes())
            {
                AddModule(type, true);
            }

            StartCoroutine(InitializeAllUninitialized(callback));
        }

        public static void Restart()
        {
            SceneManager.sceneLoaded -= _self.AfterSceneLoad;
            Destroy(_self.gameObject);
            Destroy(ConstantUiParent.parent.gameObject);
            Destroy(Camera.main.gameObject);
            _self = null;

            SceneManager.LoadScene("DefaultStart");
        }

        public static bool Has<T>() where T : ModuleBase => LoadedModules.Any(m => m is T);

        public static T Get<T>() where T : ModuleBase
        {
            foreach (ModuleBase module in LoadedModules)
                if (module is T result)
                    return result;

            _self.PrintError(typeof(T) + " is no loaded Module");
            return null;
        }

        public static TLoadData GetLoadData<TLoadData>() where TLoadData : SceneLoadData => _self.sceneLoadData.OfType<TLoadData>().FirstOrDefault();

        #region module managing

        public static void AddSubModule<TModule>() where TModule : ModuleBase
        {
            AddModule(typeof(TModule), false);
        }

        public static void LoadModule<TModule>(Action<TModule> onModuleLoaded) where TModule : ModuleBase
        {
            if (LoadedModules.ToArray().TryFind(m => m is TModule, out ModuleBase module))
            {
                onModuleLoaded?.Invoke(module as TModule);
                return;
            }
            
            if (!_self._uninitializedModules.TryFind(m => m is TModule, out module))
            {
                module = AddModule(typeof(TModule), false);
            }

            if (module != null)
            {
                _self.StartCoroutine(_self.InitializeAllUninitialized(() => onModuleLoaded?.Invoke(module as TModule)));
            }
        }

        private static ModuleBase AddModule(Type moduleType, bool isConstant)
        {
            ModuleBase result = LoadedModules.FirstOrDefault(m => m.GetType() == moduleType);
            if (result != null || _self.gameObject.TryGetComponent(moduleType, out var comp))
            {
                _self.PrintWarning("Module of type " + moduleType.ToString() + " is already added");
                return null;
            }

            result = _self.gameObject.AddComponent(moduleType) as ModuleBase;
            AddModule(result, isConstant);
            return result;
        }

        private static void AddModule(ModuleBase module, bool isConstant)
        {
            if (isConstant)
                _self.constantModules.Add(module);
            else
                _self._sceneModules.Add(module);
            _self._uninitializedModules.Add(module);
            module.Register();
            module.PrintLog("registered");
        }

        private void RemoveSceneModules()
        {
            foreach (ModuleBase module in _sceneModules.ToArray())
            {
                module.BeforeUnregister();
                _self._sceneModules.Remove(module);
                module.PrintLog("unregistered");
                if(module.gameObject == gameObject) //was temporarily added to modulecontroller game object, if not it will be destroyed with scene
                {
                    Destroy(module);
                }
            }
        }

        private IEnumerator InitializeAllUninitialized(Action callback)
        {
            var modules = _uninitializedModules.OrderByDescending(m => m.InitPriority).ToArray();
            _uninitializedModules.Clear();
            foreach (var module in modules)
            {
                var initRoutine = module.InitializeAsync();
                while (initRoutine.MoveNext())
                    yield return initRoutine.Current;
                module.PrintLog("initialized");
            }
            callback?.Invoke();
        }
        #endregion

        #region sceneloading
        public static void LoadScene(Enum scene, params SceneLoadData[] loadData) => LoadScene(scene.ToString(), loadData);
        public static void LoadScene(string scene, params SceneLoadData[] loadData)
        {
            _self._blendMask.gameObject.SetActive(true);
            _self._blendMask.material.SetFloat("_Radius", 1200);
            _self.StartTickRoutine(dt =>
            {
                float scaledDt = dt * (1 / 0.3f);
                float current = _self._blendMask.material.GetFloat("_Radius");
                _self._blendMask.material.SetFloat("_Radius", current - scaledDt * 1200);
                return scaledDt;
            },
            () => _self.LoadSceneInternal(scene, loadData));
        }

        private void LoadSceneInternal(string scene, params SceneLoadData[] loadData)
        {
            RemoveSceneModules();
            sceneLoadData.Clear();
            sceneLoadData.AddRange(loadData);
            SceneManager.LoadSceneAsync(scene.ToString());
        }

        private void AfterSceneLoad(Scene scene, LoadSceneMode mode)
        {
            this.PrintLog("--- Starting scene " + scene.name + " ---");
            var holder = FindObjectOfType<SceneModuleLoader>();
            if(holder == null) //no scenemodules to load
            {
                return;
            }

            foreach (var module in holder.SceneModules)
            {
                AddModule(module, false);
            }

            this.PrintLog("Starting initialize routine");
            StartCoroutine(OnSceneLoadedRoutine());
        }

        private IEnumerator OnSceneLoadedRoutine()
        {
            yield return null;

            var initRoutine = InitializeAllUninitialized(null);
            while (initRoutine.MoveNext())
                yield return initRoutine.Current;

            yield return null;

            foreach (ModuleBase module in LoadedModules)
                module.OnSceneFullyLoaded();

            OnSceneChanged?.Invoke();

            _self.StartTickRoutine(dt =>
            {
                float scaledDt = dt * (1 / SceneFadeSpeed);
                float current = _self._blendMask.material.GetFloat("_Radius");
                _self._blendMask.material.SetFloat("_Radius", current + scaledDt * 1200);
                return scaledDt;
            },
            () =>
            {
                _self._blendMask.gameObject.SetActive(false);
            });
        }
        #endregion
    }
}