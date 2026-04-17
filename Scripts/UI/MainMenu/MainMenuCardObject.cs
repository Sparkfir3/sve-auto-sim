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
        [TitleGroup("Main Menu Data"), SerializeField]
        private Animator animator;
        [TitleGroup("Main Menu Data"), SerializeField]
        private string animField_Selected = "Selected";

        private bool isHovering;

        public event Action<MainMenuAction> OnCardSelected;

        // ------------------------------

        private void Update()
        {
            if(animator)
                animator.SetBool(animField_Selected, isHovering);
        }

        // ------------------------------

        [Button]
        public void OnClick()
        {
            OnCardSelected?.Invoke(Action);
        }

        public override void OnHoverEnter()
        {
            isHovering = true;
        }

        public override void OnHoverExit()
        {
            isHovering = false;
        }
    }
}
