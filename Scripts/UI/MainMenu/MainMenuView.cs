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
        [field: Title("Runtime Data"), SerializeField]
        public MainMenuViewState CurrentState { get; private set; }

        [Title("Settings"), SerializeField]
        private SerializedDictionary<MainMenuAction, MainMenuTransition> transitions;

        [Title("Object References"), SerializeField]
        private SerializedDictionary<MainMenuButton, MainMenuCardObject> buttonCards;
        [SerializeField]
        private SerializedDictionary<MainMenuCardPosition, Transform> cardPositions;
        [SerializeField]
        private SteamRoomCodeInputField steamRoomCodeInputField;
        [FoldoutGroup("Controllers"), SerializeField]
        private CardAnimationController animationController;
        [FoldoutGroup("Controllers"), SerializeField]
        private MainMenuInputController inputController;

        public bool AllowInputs => !animationController.IsAnimating;
        public string RoomCode => steamRoomCodeInputField.Text;

        public event Action<MainMenuViewState> OnStateEnter;
        public event Action<MainMenuViewState> OnStateExit;
        public event Action<MainMenuButton> OnButtonClicked;

        // ------------------------------

        private void Awake()
        {
            foreach(var kvPair in buttonCards)
            {
                (MainMenuButton button, MainMenuCardObject card) = (kvPair.Key, kvPair.Value);
                card.OnCardSelected += x =>
                {
                    OnButtonClickedInternal(button, x);
                };
            }
            OnStateEnter += HandleStateEnter;
            OnStateExit += HandleStateExit;
        }

        private void Update()
        {
            inputController.AllowInputs = AllowInputs;
        }

        // ------------------------------

        public void PerformAction(MainMenuAction action)
        {
            if(transitions.TryGetValue(action, out MainMenuTransition transition))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteTransition(transition));
            }
        }

        [TitleGroup("Debug"), Button, DisableInEditorMode]
        private void OnButtonClickedInternal(MainMenuButton button, MainMenuAction action)
        {
            if(action == MainMenuAction.Back)
            {
                MainMenuAction newAction = CurrentState.BackAction();
                if(newAction != MainMenuAction.Back)
                {
                    OnButtonClickedInternal(button, newAction);
                    return;
                }
            }

            PerformAction(action);
            OnButtonClicked?.Invoke(button);
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
            OnStateExit?.Invoke(CurrentState);
            transition.OnStartTransition?.Invoke();
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

            CurrentState = transition.TargetMenuState;
            OnStateEnter?.Invoke(CurrentState);
            transition.OnEndTransition?.Invoke();

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
