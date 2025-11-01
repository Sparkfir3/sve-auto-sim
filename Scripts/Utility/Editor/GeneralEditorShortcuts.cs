using System;
using System.Reflection;
using UnityEditor;

namespace Sparkfire.Utility
{
    public static class EditorShortcuts
    {
        [MenuItem("Tools/Toggle Inspector Mode &d")]
        private static void ToggleInspectorDebug()
        {
            // "EditorWindow.focusedWindow" can be used instead
            EditorWindow targetInspector = EditorWindow.focusedWindow;
            if(!targetInspector || targetInspector.GetType().Name != "InspectorWindow")
                targetInspector = EditorWindow.mouseOverWindow;
            if(!targetInspector || targetInspector.GetType().Name != "InspectorWindow")
                return;

            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");

            // Get the type of the inspector window to find out the variable/method from
            FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

            // Get the field we want to read, for the type (not our instance)
            InspectorMode mode = (InspectorMode)field.GetValue(targetInspector);

            // Read the value for our target inspector
            mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);

            // Find the method to change the mode for the type
            MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);

            // Call the function on our targetInspector, with the new mode as an object[]
            method?.Invoke(targetInspector, new object[] {mode});

            // Refresh inspector
            targetInspector.Repaint();
        }

        [MenuItem("Tools/Toggle Lock %l")]
        private static void ToggleInspectorLock()
        {
            EditorWindow targetWindow = EditorWindow.focusedWindow;
            if(!targetWindow)
                targetWindow = EditorWindow.mouseOverWindow;
            if(!targetWindow)
                return;

            Type type;
            PropertyInfo propertyInfo = null;
            switch(targetWindow.GetType().Name)
            {
                case "InspectorWindow":
                    type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
                    propertyInfo = type.GetProperty("isLocked");
                    break;
                case "ProjectBrowser":
                    type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.ProjectBrowser");
                    propertyInfo = type.GetProperty("isLocked", BindingFlags.NonPublic | BindingFlags.Instance);
                    break;
            }
            if(propertyInfo == null)
                return;

            bool value = (bool)propertyInfo.GetValue(targetWindow, null);
            propertyInfo.SetValue(targetWindow, !value, null);
            targetWindow.Repaint();
        }
    }
}
