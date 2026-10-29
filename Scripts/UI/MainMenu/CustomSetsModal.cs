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
        private Transform container;
        [SerializeField]
        private Toggle offToggle;
        [SerializeField]
        private TextMeshProUGUI offToggleText;
        [SerializeField]
        private Toggle onToggle;
        [SerializeField]
        private TextMeshProUGUI onToggleText;

        [BoxGroup("Buttons"), SerializeField]
        private Button closeButton;
        [BoxGroup("Buttons"), SerializeField]
        private Button compileButton;
        [BoxGroup("Buttons"), SerializeField]
        private Button downloadImagesButton;
        [BoxGroup("Buttons"), SerializeField]
        private Button clearConsoleButton;
        [BoxGroup("Buttons"), SerializeField]
        private Button openFolderButton;

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
            downloadImagesButton.onClick.AddListener(DownloadImages);
            downloadImagesButton.interactable = false;
            clearConsoleButton.onClick.AddListener(ClearConsole);
            debugConsole.Clear();

            // Other
            closeButton.onClick.AddListener(Close);
            openFolderButton.onClick.AddListener(OpenDataFolder);
        }

        public void Open()
        {
            if(!container)
                container = transform.parent;
            container.gameObject.SetActive(true);
        }

        public void Close()
        {
            if(!container)
                container = transform.parent;
            container.gameObject.SetActive(false);
        }

        // ------------------------------

        private void CompileCustomSets()
        {
            controller.CompileCustomSets();
            if(!SveScriptDebugger.HasErrors())
                downloadImagesButton.interactable = true;
            else if(!CustomSetController.UseCustomSets)
                offToggle.isOn = true;
            debugConsole.AddEntries(SveScriptDebugger.GetLogs());
        }

        private void DownloadImages()
        {
            SveScriptDebugger.Log("Downloading images...");
            debugConsole.AddEntry(LogType.Log, "Downloading images...");
            controller.DownloadCustomSetImages(() =>
            {
                SveScriptDebugger.Log("Finished downloading images");
                debugConsole.AddEntry(LogType.Log, "Finished downloading images");
            });
        }

        private void ClearConsole()
        {
            debugConsole.Clear();
        }

        private void OpenDataFolder()
        {
            Application.OpenURL($"file://{Application.persistentDataPath}");
        }
    }
}
