using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using SVESimulator.UI;
using UnityEngine;

namespace SVESimulator
{
    public abstract class SveEffect : Effect
    {
        [StringField("Text", width = 400), Order(0)]
        public string text;

        // ------------------------------

        public abstract void Resolve(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, Action onComplete = null);

        protected void ResolveOnTarget(PlayerController player, int triggeringCardInstanceId, string triggeringCardZone, int sourceCardInstanceId, string sourceCardZone, SVEProperties.SVEEffectTarget targetMode, string rawFilter = "",
            Action<List<CardObject>> onTargetFound = null)
        {
            List<CardObject> targets = new();
            Dictionary<SVEFormulaParser.CardFilterSetting, string> filter;

            switch(targetMode)
            {
                case SVEProperties.SVEEffectTarget.Self:
                    CardObject sourceCard = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                    if(sourceCard)
                        targets.Add(sourceCard);
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllPlayerCards:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.ZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllPlayerCardsEx:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.ZoneController.exAreaZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllPlayerCardsFieldAndEx:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.ZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    targets.AddRange(player.ZoneController.exAreaZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllOpponentCards:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.OppZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllCards:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.ZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    targets.AddRange(player.OppZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.TriggerCard:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    if(player.ZoneController.AllZones.TryGetValue(triggeringCardZone, out CardZone triggerZone) && triggerZone.TryGetCard(triggeringCardInstanceId, out CardObject triggerCard))
                    {
                        if(filter.MatchesCard(triggerCard))
                            targets.Add(triggerCard);
                    }
                    onTargetFound?.Invoke(targets);
                    return;

                // ------------------------------

                // See: SelectTargetCardsToResolve() below
                case SVEProperties.SVEEffectTarget.TargetPlayerCard:
                    SelectTargetCardsToResolve(new List<string>() { SVEProperties.Zones.Field }, null);
                    break;
                case SVEProperties.SVEEffectTarget.TargetPlayerCardEx:
                    SelectTargetCardsToResolve(new List<string>() { SVEProperties.Zones.ExArea }, null);
                    break;

                case SVEProperties.SVEEffectTarget.TargetOpponentCard:
                    SelectTargetCardsToResolve(null, new List<string>() { SVEProperties.Zones.Field });
                    break;
                case SVEProperties.SVEEffectTarget.TargetOpponentCardsDivided:
                    SelectTargetCardsToResolve(null, new List<string>() { SVEProperties.Zones.Field }, EffectTargetCardScreen.SelectMode.MultiSelect);
                    break;
                case SVEProperties.SVEEffectTarget.TargetOpponentCardOrLeader:
                    SelectTargetCardsToResolve(null, new List<string>() { SVEProperties.Zones.Field, SVEProperties.Zones.Leader });
                    break;

                case SVEProperties.SVEEffectTarget.TargetCard:
                    SelectTargetCardsToResolve(new List<string>() { SVEProperties.Zones.Field }, new List<string>() { SVEProperties.Zones.Field });
                    break;

                // ------------------------------

                default:
                    Debug.LogError($"SVEEffectTarget mode {targetMode} is not implemented yet.");
                    onTargetFound?.Invoke(targets);
                    return;
            }

            // ---

            void SelectTargetCardsToResolve(List<string> validLocalZones, List<string> validOppZones, EffectTargetCardScreen.SelectMode mode = EffectTargetCardScreen.SelectMode.Single)
            {
                CardObject sourceCard = CardManager.Instance.GetCardByInstanceId(sourceCardInstanceId);
                string cardName = sourceCard ? LibraryCardCache.GetCard(sourceCard.RuntimeCard.cardId).name : null;
                EffectTargetingUI.TargetCard.SetText(cardName, text);
                EffectTargetingUI.TargetCard.Open(player, sourceCardInstanceId, rawFilter, validLocalZones, validOppZones, mode);
                GameUIManager.NetworkedCalls.CmdShowOpponentTargeting(player.GetOpponentInfo().netId, cardName, text);

                EffectTargetingUI.TargetCard.OnSelectionComplete.AddListener(cards =>
                {
                    onTargetFound?.Invoke(cards);
                    EffectTargetingUI.TargetCard.Close();
                    GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(player.GetOpponentInfo().netId);
                });
            }
        }

        protected void SelectZoneToResolve(PlayerController player, string cardName, List<string> validLocalZones, List<string> validOppZones = null, int minActionCount = 1, int maxActionCount = 1, Action<CardZone> resolve = null)
        {
            int actionCount = 0;
            EffectTargetingUI.TargetZone.Open(player, validLocalZones, validOppZones);
            GameUIManager.NetworkedCalls.CmdShowOpponentTargeting(player.GetOpponentInfo().netId, cardName, text);
            EffectTargetingUI.TargetZone.OnSelectZone.AddListener((zone, isLocal) =>
            {
                PlayerCardZoneController zoneController = isLocal ? player.ZoneController : player.OppZoneController;
                resolve?.Invoke(zoneController.AllZones[zone]);

                actionCount++;
                EffectTargetingUI.TargetZone.SetCloseButtonActive(actionCount >= minActionCount && actionCount < maxActionCount);
                EffectTargetingUI.TargetZone.SetCountRemainingText(maxActionCount - actionCount);
                if(actionCount >= maxActionCount)
                {
                    EffectTargetingUI.TargetZone.Close();
                    GameUIManager.NetworkedCalls.CmdCloseOpponentTargeting(player.GetOpponentInfo().netId);
                }
            });
            EffectTargetingUI.TargetZone.SetCloseButtonActive(actionCount >= minActionCount);
        }
    }
}
