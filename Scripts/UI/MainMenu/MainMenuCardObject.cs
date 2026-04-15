using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;

namespace SVESimulator.UI
{
    public class MainMenuCardObject : CardObject
    {
        [field: TitleGroup("Main Menu Data", order: -1f), SerializeField]
        public MainMenuAction Action { get; set; }

        public event Action<MainMenuAction> OnCardSelected;

        [Button]
        public void OnClick()
        {
            OnCardSelected?.Invoke(Action);
        }

        public override void OnHoverEnter()
        {

        }

        public override void OnHoverExit()
        {

        }
    }
}
