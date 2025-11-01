using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

namespace SVESimulator
{
    public class PlayPointMeter : MonoBehaviour
    {
        #region Variables

        private const int MAX_POINTS = 10;

        [TitleGroup("Runtime Data"), SerializeField, ReadOnly, Range(0, 10)]
        private int currentMaxPoints;
        [SerializeField, ReadOnly, Range(0, 10)]
        private int currentPoints;
        
        [TitleGroup("Object References"), SerializeField, DisableInPlayMode]
        private List<PlayPointSphere> spheres;
        [SerializeField]
        private TextMeshProUGUI maxPointsTextBox;
        [SerializeField]
        private TextMeshProUGUI currentPointsTextBox;

        #endregion
        
        // ----------------------------------------------

        #region Set Values
        
        [TitleGroup("Buttons"), Button, DisableInEditorMode]
        public void SetMaxPoints(in int value, in bool setCurrentPoints = false)
        {
            currentMaxPoints = Mathf.Clamp(value, 0, MAX_POINTS);
            if(setCurrentPoints)
                SetCurrentPoints(value);
            else
                UpdateVisuals();
        }

        public void SetCurrentPoints(in int value)
        {
            currentPoints = Mathf.Clamp(value, 0, currentMaxPoints);
            UpdateVisuals();
        }

        #endregion

        // ----------------------------------------------

        #region Visual Controls

        private void UpdateVisuals()
        {
            UpdateTextBoxes();
            UpdateSpheres();
        }

        private void UpdateTextBoxes()
        {
            maxPointsTextBox.text = currentMaxPoints.ToString();
            currentPointsTextBox.text = currentPoints.ToString();
        }

        private void UpdateSpheres()
        {
            for(int i = 0; i < spheres.Count; i++)
            {
                spheres[i].SetStatus(i < currentPoints ? PlayPointSphere.Status.Full
                    : i < currentMaxPoints ? PlayPointSphere.Status.Empty : PlayPointSphere.Status.Disabled);
            }
        }

        #endregion

        // ----------------------------------------------

        #region Debug Buttons

        [TitleGroup("Buttons"), Button, DisableInEditorMode]
        private void IncrementMaxPoints()
        {
            SetMaxPoints(currentMaxPoints + 1);
        }

        [TitleGroup("Buttons"), Button, DisableInEditorMode]
        private void DecrementCurrentPoints()
        {
            SetCurrentPoints(currentPoints - 1);
        }

        #endregion
    }
}
