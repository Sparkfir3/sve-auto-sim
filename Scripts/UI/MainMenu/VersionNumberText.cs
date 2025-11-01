using UnityEngine;
using TMPro;

namespace SVESimulator
{
    public class VersionNumberText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textBox;
        [SerializeField]
        private string prefix;
        [SerializeField]
        private string suffix;
        [SerializeField]
        private string editorSuffix = "-editor";

        // ------------------------------

        private void Awake()
        {
            textBox.text = prefix + Application.version + suffix;
#if UNITY_EDITOR
            textBox.text += editorSuffix;
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!textBox)
                textBox = GetComponent<TextMeshProUGUI>();
        }
#endif
    }
}
