using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SVESimulator.UI
{
    public enum ButtonDisplayPosition { Center, Top }

    [Serializable]
    public class ButtonPositionData
    {
        [BoxGroup("Text")]
        public float textPosition;
        [BoxGroup("Text")]
        public Vector2 textAnchor = new Vector2(0.5f, 0.5f);
        [BoxGroup("Text")]
        public Vector2 textPivot = new Vector2(0.5f, 0.5f);

        [BoxGroup("Button")]
        public float buttonPosition;
        [BoxGroup("Button")]
        public Vector2 buttonAnchor = new Vector2(0.5f, 0.5f);
        [BoxGroup("Button")]
        public Vector2 buttonPivot = new Vector2(0.5f, 0.5f);
    }
}
