using UnityEngine;
using Sirenix.OdinInspector;
using SVESimulator.Database;
using SVESimulator.SveScript;
using TMPro;
using UnityEngine.UI;

namespace SVESimulator.UI
{
    public class CustomSetsModal : MonoBehaviour
    {
        [Title("Object References"), SerializeField]
        private CustomSetController controller;
        [SerializeField]
        private Toggle offToggle;
        [SerializeField]
        private TextMeshProUGUI offToggleText;
        [SerializeField]
        private Toggle onToggle;
        [SerializeField]
        private TextMeshProUGUI onToggleText;
        [SerializeField]
        private Button compileButton;
        [SerializeField]
        private DebugConsoleDisplay debugConsole;

        // ------------------------------

        private void Start()
        {
            if(!controller)
                controller = FindObjectOfType<CustomSetController>();

            // Toggles
            if(CustomSetController.UseCustomSets)
                onToggle.isOn = true;
            else
                offToggle.isOn = true;
            offToggleText.color = !CustomSetController.UseCustomSets ? Color.red : Color.black;
            onToggleText.color = CustomSetController.UseCustomSets ? Color.green : Color.black;
            offToggle.onValueChanged.AddListener(isOn =>
            {
                if(isOn)
                    controller.SetCustomSetsEnabled(false);
                offToggleText.color = isOn ? Color.red : Color.black;
            });
            onToggle.onValueChanged.AddListener(isOn =>
            {
                if(isOn)
                    controller.SetCustomSetsEnabled(true);
                onToggleText.color = isOn ? Color.green : Color.black;
            });

            // Compiler
            compileButton.onClick.AddListener(CompileCustomSets);
            debugConsole.Clear();
        }

        private void CompileCustomSets()
        {
            debugConsole.Clear();
            controller.CompileCustomSets();
            debugConsole.AddEntries(SveScriptDebugger.GetLogs());
        }
    }
}
