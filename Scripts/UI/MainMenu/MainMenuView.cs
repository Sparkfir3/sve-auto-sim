using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [Title("Transitions"), SerializeReference]
        private SerializedDictionary<MainMenuButton, MainMenuTransition> transitions;

        [Title("Object References"), SerializeField]
        private SerializedDictionary<MainMenuButton, MainMenuCardObject> buttonCards;
        [SerializeField]
        private SerializedDictionary<MainMenuCardPosition, Transform> cardPositions;
        [SerializeField]
        private CardAnimationController animationController;

        // ------------------------------

        private IEnumerator ExecuteTransition(MainMenuTransition transition)
        {
            foreach(MainMenuTransitionAction action in transition.Actions)
            {
                switch(action)
                {
                    case MainMenuTransitionMoveCard moveAction:
                        MainMenuCardObject card = buttonCards[moveAction.TargetButton];
                        Transform target = moveAction.TargetPosition == MainMenuCardPosition.Static
                            ? card.transform
                            : cardPositions[moveAction.TargetPosition];
                        yield return moveAction.Execute(card, target, animationController);
                        break;

                    case MainMenuTransitionDelay delayAction:
                        yield return delayAction.Execute();
                        break;
                }
            }
        }

        // ------------------------------

        [Button]
        private void TestButton(MainMenuButton buttonType)
        {
            if(transitions.TryGetValue(buttonType, out MainMenuTransition transition))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteTransition(transition));
            }
        }
    }
}
