using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator.UI
{
    [CreateAssetMenu(menuName = "SVE Simulator/Main Menu Transition", fileName = "MainMenuTrans_FROM_TO", order = 1000)]
    public class MainMenuTransition : ScriptableObject
    {
        [field: SerializeField, TableList]
        public List<MainMenuTransitionCardStartPosition> StartPositions { get; private set; } = new();
        [field: SerializeField, TableList, Space(10)]
        public List<MainMenuTransitionMoveCardAction> MoveActions { get; private set; } = new();
        [field: SerializeField, Space(10)]
        public float Delay { get; private set; }
        [field: SerializeField, TableList]
        public List<MainMenuTransitionMoveCardAction> MoveActionsSecondary { get; private set; } = new();
        [field: SerializeField, Space(10)]
        public MainMenuViewState EndState { get; private set; }
    }

    // ------------------------------

    [Serializable]
    public class MainMenuTransitionCardStartPosition
    {
        [field: SerializeField]
        public MainMenuButton TargetButton { get; private set; }
        [field: SerializeField]
        public MainMenuCardPosition TargetPosition { get; private set; }
        [field: SerializeField]
        public bool FaceUp { get; private set; } = true;
        [field: SerializeField]
        public bool IsActive { get; private set; } = true;
    }

    [Serializable]
    public class MainMenuTransitionMoveCardAction
    {
        [field: SerializeField]
        public MainMenuButton TargetButton { get; private set; }
        [field: SerializeField]
        public MainMenuCardPosition TargetPosition { get; private set; }
        [field: SerializeField]
        public float PostDelay { get; private set; }

        [SerializeField]
        private CardMovementType moveType;
        [SerializeField]
        private bool toFaceUp = true;
        [SerializeField]
        private bool isActive = true;

        public void Execute(MainMenuCardObject card, Transform target, CardAnimationController animController, Action onComplete = null)
        {
            if(!card)
                return;

            if(isActive)
                card.gameObject.SetActive(true);
            animController.MoveCardToPosition(moveType, card, target.position, target.rotation * (toFaceUp ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f)), onComplete: () =>
            {
                if(!isActive && card)
                    card.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}
