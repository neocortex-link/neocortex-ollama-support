using UnityEditor;
using UnityEngine;

namespace Neocortex
{
    [CustomEditor(typeof(OllamaModelDropdown))]
    public class OllamaModelDropdownEditor : UnityEditor.Editor
    {
        private const string COMPONENT_NAME_IN_HIERARCY = "Model Dropdown";
        
        [MenuItem("GameObject/Neocortex/Ollama Support/Model Dropdown", false, -int.MaxValue)]
        private static void AddElement()
        {
            EditorUtilities.CreateInCanvas<OllamaModelDropdown>(COMPONENT_NAME_IN_HIERARCY, (canvas, target) =>
            {
                var targetTransform = target.transform as RectTransform;
                var canvasTransform = canvas.transform as RectTransform;

                if (targetTransform == null || canvasTransform == null) return;

                targetTransform.anchoredPosition = new Vector2(0, 0);
            });
        }
    }
}