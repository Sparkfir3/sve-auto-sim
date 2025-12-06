using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;
using UnityEngine;

namespace SVESimulator
{
    public class SummonTokenEffect : SveEffect
    {
        [StringField("Token Name", width = 200), Order(1)]
        public string tokenName;

        [EnumField("Zone", width = 200), Order(2)]
        public SVEProperties.TokenCreationOption createTokenOption;

        [StringField("Amount", width = 100), Order(3)]
        public string amount = "1";

        // ------------------------------

        public override void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null)
        {
            int tokenCount = SVEFormulaParser.ParseValue(amount, player);
            List<CardObject> tokenList = new();

            switch(createTokenOption)
            {
                case SVEProperties.TokenCreationOption.ChooseForEachFieldOrEx: // Select zone
                    CardObject sourceCard = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                    string cardName = sourceCard ? LibraryCardCache.GetCard(sourceCard.RuntimeCard.cardId).name : null;
                    EffectTargetingUI.TargetZone.SetDescriptionText(text);
                    EffectTargetingUI.TargetZone.SetCountRemainingText(tokenCount);
                    SelectZoneToResolve(player, cardName, new List<string> { SVEProperties.Zones.Field, SVEProperties.Zones.ExArea },
                        minActionCount: 0, maxActionCount: tokenCount, resolve: zone =>
                        {
                            SVEProperties.TokenCreationOption option;
                            switch(zone.Runtime.name)
                            {
                                case SVEProperties.Zones.Field:
                                    option = SVEProperties.TokenCreationOption.Field;
                                    break;
                                case SVEProperties.Zones.ExArea:
                                    option = SVEProperties.TokenCreationOption.ExArea;
                                    break;
                                default:
                                    Debug.LogError($"Invalid zone selected when creating token: {zone.Runtime.name}");
                                    return;
                            }
                            tokenList.Add(player.LocalEvents.CreateToken(tokenName, option));
                        });
                    break;

                case SVEProperties.TokenCreationOption.FieldOverflowToEx: // Create and overflow
                    int countOnField = Mathf.Min(tokenCount, player.ZoneController.fieldZone.OpenSlotCount());
                    int countOnExArea = tokenCount - countOnField;
                    for(int i = 0; i < countOnField; i++)
                        tokenList.Add(player.LocalEvents.CreateToken(tokenName, SVEProperties.TokenCreationOption.Field));
                    for(int i = 0; i < countOnExArea; i++)
                        tokenList.Add(player.LocalEvents.CreateToken(tokenName, SVEProperties.TokenCreationOption.ExArea));
                    break;

                default: // Regular token creation
                    for(int i = 0; i < tokenCount; i++)
                        tokenList.Add(player.LocalEvents.CreateToken(tokenName, createTokenOption));
                    break;
            }

            OnTokensCreated(tokenList.Where(x => x).ToList(), player, triggeringCardInstanceId, triggeringCardZone, sourceCardInstanceId, sourceCardZone, onComplete);
        }

        protected virtual void OnTokensCreated(List<CardObject> tokens, PlayerController player, int triggeringCardInstanceId, string triggeringCardZone,
            int sourceCardInstanceId, string sourceCardZone, Action onComplete)
        {
            SVEEffectPool.Instance.StartCoroutine(ResolveCoroutine());
            IEnumerator ResolveCoroutine()
            {
                yield return EngageWardTokens(tokens, player);
                onComplete?.Invoke();
            }
        }

        protected IEnumerator EngageWardTokens(List<CardObject> tokens, PlayerController player)
        {
            foreach(CardObject token in tokens)
            {
                if(!token.RuntimeCard.HasKeyword(SVEProperties.Keywords.Ward) || !player.ZoneController.fieldZone.ContainsCard(token))
                    continue;
                bool waiting = true;
                GameUIManager.MultipleChoice.OpenEngageWardCardOptions(player, token, executeConfirmationTiming: false, onComplete: () =>
                {
                    waiting = false;
                });
                yield return new WaitUntil(() => !waiting);
            }
        }
    }
}
