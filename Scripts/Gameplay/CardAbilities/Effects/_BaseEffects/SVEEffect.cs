using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using Sparkfire.Utility;
using SVESimulator.UI;
using UnityEngine;

namespace SVESimulator
{
    public abstract class SveEffect : Effect
    {
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
                case SVEProperties.SVEEffectTarget.AllOpponentCardsAndLeader:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.OppZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    targets.AddRange(player.OppZoneController.leaderZone.AllCards);
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllOpponentCardsEx:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.OppZoneController.exAreaZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    onTargetFound?.Invoke(targets);
                    return;
                case SVEProperties.SVEEffectTarget.AllOpponentCardsFieldAndEx:
                    filter = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                    targets.AddRange(player.OppZoneController.fieldZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
                    targets.AddRange(player.OppZoneController.exAreaZone.GetAllPrimaryCards().Where(x => filter.MatchesCard(x)).ToList());
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
                    CardObject triggerCard = CardManager.Instance.GetCardByInstanceId(triggeringCardInstanceId);
                    if(filter.MatchesCard(triggerCard))
                        targets.Add(triggerCard);
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
                case SVEProperties.SVEEffectTarget.TargetOpponentCardEx:
                    SelectTargetCardsToResolve(null, new List<string>() { SVEProperties.Zones.ExArea });
                    break;

                case SVEProperties.SVEEffectTarget.TargetCard:
                    SelectTargetCardsToResolve(new List<string>() { SVEProperties.Zones.Field }, new List<string>() { SVEProperties.Zones.Field });
                    break;
                case SVEProperties.SVEEffectTarget.TargetCardEx:
                    SelectTargetCardsToResolve(new List<string>() { SVEProperties.Zones.ExArea }, new List<string>() { SVEProperties.Zones.ExArea });
                    break;

                // ------------------------------

                case SVEProperties.SVEEffectTarget.MultiTargetMode:
                    var targetingData = GetMultiTargetModeFilterSettings(rawFilter, sourceCardInstanceId);
                    EffectTargetingUI.TargetCard.OnSelectionUpdated.AddListener(cards => UpdateTargetsForMultiTargetMode(player, cards, targetingData));
                    int targetAmount = targetingData.Sum(x => x.Item1);
                    rawFilter = $"m({targetAmount},{targetAmount})"; // clear to prevent EffectTargetingUI from trying to parse it, only keep min/max
                    SelectTargetCardsToResolve(null, null);
                    UpdateTargetsForMultiTargetMode(player, null, targetingData);
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
                    player.LocalEvents.OnCardsSelectedForAbility(cards);
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

        // ------------------------------

        #region Multi Target Mode (Targeting with Multiple Target-Filter Pairs)

        // Triplet of type <amount, target, filter>
        private List<(int, SVEProperties.SVEEffectTarget, Dictionary<SVEFormulaParser.CardFilterSetting, string>)> GetMultiTargetModeFilterSettings(string filter, int sourceCardInstanceId)
        {
            List<string> rawTargetData = new();
            for(int pointer = 0; pointer < filter.Length; pointer++)
            {
                string newRawFilter = filter[pointer..].TextInsideParentheses(out _, out int length);
                pointer += length;
                rawTargetData.Add(newRawFilter);
            }

            List<(int, SVEProperties.SVEEffectTarget, Dictionary<SVEFormulaParser.CardFilterSetting, string>)> targetingData = new();
            for(int i = 0; i < rawTargetData.Count; i++)
            {
                // TODO - support min/max amount, don't rely on split by comma
                string[] split = rawTargetData[i].Split(",");
                int amount = int.Parse(split[0]);
                SVEProperties.SVEEffectTarget target = (SVEProperties.SVEEffectTarget)Enum.Parse(typeof(SVEProperties.SVEEffectTarget), split[1]);
                string rawFilter = split.Length > 2 ? split[2] : null;
                var filterDict = SVEFormulaParser.ParseCardFilterFormula(rawFilter, sourceCardInstanceId);
                targetingData.Add((amount, target, filterDict));
            }
            return targetingData;
        }

        // Triplet of type <amount, target, filter>
        private void UpdateTargetsForMultiTargetMode(PlayerController player, List<CardObject> selectedCards, List<(int, SVEProperties.SVEEffectTarget, Dictionary<SVEFormulaParser.CardFilterSetting, string>)> targetingData)
        {
            List<CardObject> newTargetsList = selectedCards != null ? new(selectedCards) : new();
            for(int i = 0; i < targetingData.Count; i++)
            {
                try
                {
                    CardZone zone = targetingData[i].Item2 switch
                    {
                        SVEProperties.SVEEffectTarget.TargetPlayerCard      => player.ZoneController.fieldZone,
                        SVEProperties.SVEEffectTarget.TargetOpponentCard    => player.OppZoneController.fieldZone,
                        _                                                   => null
                    };
                    if(!zone)
                        continue;

                    List<CardObject> validTargets = ((zone is CardPositionedZone posZone) ? posZone.GetAllPrimaryCards() : zone.AllCards)
                        .Where(x => targetingData[i].Item3.MatchesCard(x.RuntimeCard)).ToList();
                    if(selectedCards != null && selectedCards.Count(x => validTargets.Contains(x)) >= targetingData[i].Item1)
                        continue;
                    newTargetsList.AddRange(validTargets);
                }
                catch(Exception e)
                {
                    Debug.LogError($"An error occurred when trying to update targets for MultiTargetMode at data index {i}: {e.ToString()}");
                }
            }
            EffectTargetingUI.TargetCard.OverrideAvailableTargetsList(newTargetsList);
        }

        #endregion
    }
}
