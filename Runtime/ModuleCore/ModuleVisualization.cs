using UnityEngine;
using UnityEngine.UI;

namespace DivineSkies.Modules
{
    public class ModuleVisualization : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        public virtual void Initialize()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected virtual void OnCloseClicked()
        {
            ModuleController.LoadDefaultScene();
        }
    }
}