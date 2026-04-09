using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [Title("Settings"), SerializeField]
        private SerializedDictionary<MainMenuAction, MainMenuTransition> transitions;

        [Title("Object References"), SerializeField]
        private SerializedDictionary<MainMenuButton, MainMenuCardObject> buttonCards;
        [SerializeField]
        private SerializedDictionary<MainMenuCardPosition, Transform> cardPositions;
        [SerializeField]
        private CardAnimationController animationController;

        // ------------------------------

        private void Awake()
        {
            foreach(MainMenuCardObject card in buttonCards.Values)
                card.OnCardSelected += OnButtonCardClicked;
        }

        // ------------------------------

        [TitleGroup("Debug"), Button]
        private void OnButtonCardClicked(MainMenuAction action)
        {
            if(transitions.TryGetValue(action, out MainMenuTransition transition))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteTransition(transition));
            }
        }

        private IEnumerator ExecuteTransition(MainMenuTransition transition)
        {
            foreach(MainMenuTransitionCardStartPosition startData in transition.StartPositions)
            {
                MainMenuCardObject card = buttonCards[startData.TargetButton];
                Transform target = cardPositions[startData.TargetPosition];
                card.transform.SetLocalPositionAndRotation(target.position, target.rotation * (startData.FaceUp ? Quaternion.identity : Quaternion.Euler(180f, 0f, 0f)));
                card.gameObject.SetActive(startData.IsActive);
            }

            if(transition.MoveActions.Count > 0)
                yield return StartCoroutine(ExecuteMoveActionSequence(transition.MoveActions));
            if(transition.Delay > 0f)
                yield return new WaitForSeconds(transition.Delay);
            if(transition.MoveActionsSecondary.Count > 0)
                yield return StartCoroutine(ExecuteMoveActionSequence(transition.MoveActionsSecondary));

            IEnumerator ExecuteMoveActionSequence(List<MainMenuTransitionMoveCardAction> actions)
            {
                foreach(MainMenuTransitionMoveCardAction action in actions)
                {
                    MainMenuCardObject card = buttonCards[action.TargetButton];
                    Transform target = action.TargetPosition == MainMenuCardPosition.Static
                        ? card.transform
                        : cardPositions[action.TargetPosition];

                    if(action.WaitForComplete)
                    {
                        bool waiting = true;
                        action.Execute(card, target, animationController, () => { waiting = false; });
                        yield return new WaitUntil(() => !waiting);
                    }
                    else
                        action.Execute(card, target, animationController);
                }
            }
        }
    }
}
