using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator.UI
{
    [CreateAssetMenu(menuName = "SVE Simulator/Main Menu Transition", fileName = "MainMenuTrans_FROM_TO", order = 1000)]
    public class MainMenuTransition : ScriptableObject
    {
        [field: SerializeReference] // TODO - SerializeReference does not work with list
        public List<MainMenuTransitionAction> Actions { get; private set; } = new();
    }

    // ------------------------------

    public abstract class MainMenuTransitionAction { }

    [Serializable]
    public class MainMenuTransitionMoveCard : MainMenuTransitionAction
    {
        [field: SerializeField]
        public MainMenuButton TargetButton { get; private set; }
        [field: SerializeField]
        public MainMenuCardPosition TargetPosition { get; private set; }

        [SerializeField]
        private CardMovementType moveType;
        [SerializeField]
        private bool isFaceUp;
        [SerializeField]
        private bool isActive;

        public IEnumerator Execute(MainMenuCardObject card, Transform target, CardAnimationController animController)
        {
            if(!card)
                yield break;

            bool animating = true;
            if(isActive)
                card.gameObject.SetActive(true);
            animController.MoveCardToPosition(moveType, card, target.position, target.rotation, onComplete: () => { animating = false; });
            yield return new WaitUntil(() => !animating);

            if(!isActive)
                card.gameObject.SetActive(false);
        }
    }

    [Serializable]
    public class MainMenuTransitionDelay : MainMenuTransitionAction
    {
        [SerializeField]
        private float delay;

        public IEnumerator Execute()
        {
            yield return new WaitForSeconds(delay);
        }
    }
}
