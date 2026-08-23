using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sparkfire.Utility;
using TMPro;

namespace SVESimulator.UI
{
    public class MainMenuView : MonoBehaviour
    {
        #region Variables

        [field: Title("Runtime Data"), SerializeField, Sirenix.OdinInspector.ReadOnly]
        public MainMenuViewState CurrentState { get; private set; }

        [Title("Settings"), SerializeField]
        private SerializedDictionary<MainMenuAction, MainMenuTransition> transitions;
        
        // ---
        
        [Title("Object References"), SerializeField]
        private MainMenuController mainMenuController;
        [SerializeField]
        private SteamRoomCodeInputField steamRoomCodeInputField;
        [SerializeField]
        private GameObject connectingIndicator;
        [SerializeField]
        private TextMeshProUGUI errorTextBox;
        [SerializeField]
        private SteamUserInfoDisplay userInfoDisplay;
        
        [FoldoutGroup("Cards"), SerializeField]
        private SerializedDictionary<MainMenuButton, MainMenuCardObject> buttonCards;
        [FoldoutGroup("Cards"), SerializeField]
        private SerializedDictionary<MainMenuCardPosition, Transform> cardPositions;
        
        [FoldoutGroup("Controllers"), SerializeField]
        private CardAnimationController animationController;
        [FoldoutGroup("Controllers"), SerializeField]
        private MainMenuInputController inputController;
        
        // ---

        public bool AllowInputs => !animationController.IsAnimating;
        public string RoomCode => steamRoomCodeInputField.Text;

        public event Action<MainMenuViewState> OnStateEnter;
        public event Action<MainMenuViewState> OnStateExit;
        public event Action<MainMenuButton> OnButtonClicked;

        #endregion

        // ------------------------------

        #region Unity Functions

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
            mainMenuController.OnTryConnection += OnTryConnection;
            mainMenuController.OnConnectionFailed += ShowErrorMessage;
            mainMenuController.OnClientConnected += OnClientConnected;
            mainMenuController.OnClientDisconnected += OnClientDisconnected;
            mainMenuController.OnOpponentConnected += OnOpponentConnected;
            mainMenuController.OnOpponentDisconnected += OnOpponentDisconnected;

            connectingIndicator.SetActive(false);
        }

        private void Update()
        {
            inputController.AllowInputs = AllowInputs;
        }

        #endregion

        // ------------------------------

        #region Menu Actions/Transitions

        public void PerformAction(MainMenuAction action)
        {
            if(action == MainMenuAction.Back)
            {
                MainMenuAction newAction = CurrentState.BackAction();
                if(newAction != MainMenuAction.Back)
                {
                    PerformAction(newAction);
                    return;
                }
            }
            if(transitions.TryGetValue(action, out MainMenuTransition transition))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteTransition(transition));
            }
        }

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

        #endregion

        // ------------------------------

        #region UI Events

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
            switch(newState)
            {
                case MainMenuViewState.PlayOnline:
                    steamRoomCodeInputField.Interactable = true;
                    steamRoomCodeInputField.Show();
                    userInfoDisplay.HideAll();
                    break;
                case MainMenuViewState.Connecting:
                case MainMenuViewState.ReadyToStart:
                    steamRoomCodeInputField.Interactable = false;
                    errorTextBox.gameObject.SetActive(false);
                    if(SVEGameNetworkManager.IsSteamManager)
                        userInfoDisplay.ShowPlayer = true;
                    break;
                default:
                    steamRoomCodeInputField.Hide();
                    errorTextBox.gameObject.SetActive(false);
                    userInfoDisplay.HideAll();
                    break;
            }
        }

        private void HandleStateExit(MainMenuViewState oldState) { }

        #endregion

        // ------------------------------

        #region Networking Events
        
        public void ShowErrorMessage(string error)
        {
            errorTextBox.text = error;
            errorTextBox.gameObject.SetActive(true);
        }
        
        public void OnTryConnection(bool isConnecting)
        {
            if(isConnecting)
            {
                steamRoomCodeInputField.Interactable = false;
                connectingIndicator.SetActive(true);
                errorTextBox.gameObject.SetActive(false);
            }
            else
            {
                steamRoomCodeInputField.Interactable = true;
                connectingIndicator.SetActive(false);
            }
        }

        public void OnClientConnected()
        {
            if(!SVEGameNetworkManager.IsSteamManager)
                return;
            userInfoDisplay.ShowPlayer = true;
            if(!NetworkClient.activeHost)
                userInfoDisplay.ShowOpponent = true;
        }

        public void OnClientDisconnected()
        {
            userInfoDisplay.HideAll();
        }

        public void OnOpponentConnected()
        {
            if(SVEGameNetworkManager.IsSteamManager)
                userInfoDisplay.ShowOpponent = true;
        }

        public void OnOpponentDisconnected()
        {
            userInfoDisplay.ShowOpponent = false;
        }

        #endregion
    }
}
