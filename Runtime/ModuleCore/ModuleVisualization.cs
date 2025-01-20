using UnityEngine;
using UnityEngine.UI;
using DivineSkies.Modules.Core;

namespace DivineSkies.Modules
{
    public class ModuleVisualization : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        protected virtual string ReturnScene => "GameWorld";

        public virtual void Initialize()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected virtual void OnCloseClicked()
        {
            ModuleController.LoadScene(ReturnScene);
        }
    }
}