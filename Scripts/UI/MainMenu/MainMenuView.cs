using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [Title("Runtime Data"), SerializeField]
        private MainMenuViewState currentState;

        [Title("Settings"), SerializeField]
        private SerializedDictionary<MainMenuAction, MainMenuTransition> transitions;
        [SerializeField]
        private SteamRoomCodeInputField steamRoomCodeInputField;

        [Title("Object References"), SerializeField]
        private SerializedDictionary<MainMenuButton, MainMenuCardObject> buttonCards;
        [SerializeField]
        private SerializedDictionary<MainMenuCardPosition, Transform> cardPositions;
        [SerializeField]
        private CardAnimationController animationController;

        public event Action<MainMenuViewState> OnStateEnter;
        public event Action<MainMenuViewState> OnStateExit;

        // ------------------------------

        private void Awake()
        {
            foreach(MainMenuCardObject card in buttonCards.Values)
                card.OnCardSelected += OnButtonCardClicked;
            OnStateEnter += HandleStateEnter;
            OnStateExit += HandleStateExit;
        }

        // ------------------------------

        [TitleGroup("Debug"), Button, DisableInEditorMode]
        private void OnButtonCardClicked(MainMenuAction action)
        {
            if(transitions.TryGetValue(action, out MainMenuTransition transition))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteTransition(transition));
            }
        }

        private void HandleStateEnter(MainMenuViewState newState)
        {
            if(newState == MainMenuViewState.PlayOnline)
                steamRoomCodeInputField.Show();
        }

        private void HandleStateExit(MainMenuViewState oldState)
        {
            if(oldState == MainMenuViewState.PlayOnline)
                steamRoomCodeInputField.Hide();
        }

        // ------------------------------

        private IEnumerator ExecuteTransition(MainMenuTransition transition)
        {
            OnStateExit?.Invoke(currentState);
            foreach(MainMenuTransitionCardStartPosition startData in transition.StartPositions)
            {
                MainMenuCardObject card = buttonCards[startData.TargetButton];
                Transform target = cardPositions[startData.TargetPosition];
                card.transform.SetLocalPositionAndRotation(target.position, target.rotation * (startData.FaceUp ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f)));
                card.gameObject.SetActive(startData.IsActive);
            }

            if(transition.MoveActions.Count > 0)
                yield return StartCoroutine(ExecuteMoveActionSequence(transition.MoveActions));
            if(transition.Delay > 0f)
                yield return new WaitForSeconds(transition.Delay);
            if(transition.MoveActionsSecondary.Count > 0)
                yield return StartCoroutine(ExecuteMoveActionSequence(transition.MoveActionsSecondary));

            currentState = transition.EndState;
            OnStateEnter?.Invoke(currentState);

            IEnumerator ExecuteMoveActionSequence(List<MainMenuTransitionMoveCardAction> actions)
            {
                foreach(MainMenuTransitionMoveCardAction action in actions)
                {
                    MainMenuCardObject card = buttonCards[action.TargetButton];
                    Transform target = action.TargetPosition == MainMenuCardPosition.Static
                        ? card.transform
                        : cardPositions[action.TargetPosition];

                    action.Execute(card, target, animationController);
                    if(action.PostDelay > 0f)
                        yield return new WaitForSeconds(action.PostDelay);
                }
            }
        }
    }
}
