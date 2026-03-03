using System;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine;
using Sirenix.OdinInspector;
using Sparkfire.Utility;

namespace SVESimulator
{
    public class CardManager : Singleton<CardManager>
    {
        [Serializable]
        private class PooledCard
        {
            public bool active;
            public CardObject card;
        }

        // ------------------------------

        #region Variables

        [TitleGroup("Runtime Data"), ShowInInspector, TableList, DisableInPlayMode]
        private List<PooledCard> cardPool = new();
        [ShowInInspector, ReadOnly]
        private SerializedDictionary<int, CardObject> cardsByInstanceId = new();

        [TitleGroup("Settings"), SerializeField]
        private Vector3 defaultRotation = new Vector3(90f, 0f, 0f);

        [TitleGroup("Object References"), SerializeField, Required]
        private CardAnimationController cardAnimator;
        [SerializeField]
        private SerializedDictionary<int, KeywordIcon.KeywordIconData> keywordIcons;

        [TitleGroup("Prefabs"), SerializeField, AssetsOnly]
        private CardObject cardPrefab;

        // ---

        public static CardAnimationController Animator => Instance.cardAnimator;

        #endregion

        // ------------------------------

        #region Public Card Controls

        /// <summary>
        /// Get a new available card object from the object pool
        /// </summary>
        /// <param name="runtimeCard">RuntimeCard to assign to the requested card object</param>
        /// <param name="setActive">Whether to set the card active or not</param>
        public CardObject RequestCard(RuntimeCard runtimeCard, bool setActive = true)
        {
            CardObject existingCard = GetCardByInstanceId(runtimeCard.instanceId);
            if(existingCard)
                return existingCard;

            if(!TryGetFirstAvailableCard(out PooledCard newCard))
            {
                newCard = SpawnNewCard();
            }
            newCard.active = true;
            newCard.card.Initialize(runtimeCard);
            if(setActive)
            {
                newCard.card.gameObject.SetActive(true);
            }
            SetCardTexture(newCard.card);
            cardsByInstanceId.Add(runtimeCard.instanceId, newCard.card);
            return newCard.card;
        }

        public bool ReleaseCard(CardObject card)
        {
            PooledCard pooledCard = cardPool.Find(x => x.card == card);
            if(pooledCard == null)
            {
                Debug.LogError($"Attempted to release card {card.gameObject.name} that is not in the pool");
                return false;
            }
            if(!pooledCard.active)
            {
                Debug.LogError($"Attempted to release card {card.gameObject.name} that is already released");
                return false;
            }

            cardsByInstanceId.Remove(card.RuntimeCard.instanceId);
            if(card.CurrentZone)
                card.CurrentZone.RemoveCard(card);
            card.RuntimeCard = null;
            card.gameObject.SetActive(false);
            card.transform.parent = transform;
            card.transform.localScale = Vector3.one;
            pooledCard.active = false;
            return true;
        }

        public void ReleaseAllDisabledCards()
        {
            foreach(PooledCard pooledCard in cardPool)
            {
                if(pooledCard.active && !pooledCard.card.gameObject.activeInHierarchy)
                    ReleaseCard(pooledCard.card);
            }
        }

        public CardObject GetCardByInstanceId(int instanceId)
        {
            return cardsByInstanceId.GetValueOrDefault(instanceId, null);
        }

        public bool TryGetKeywordIconData(int keywordValue, out KeywordIcon.KeywordIconData data)
        {
            return keywordIcons.TryGetValue(keywordValue, out data);
        }

        #endregion

        // ------------------------------

        #region Private Card Management

        protected override void Awake()
        {
            base.Awake();
            cardPool = cardPool.Where(x => x.card).ToList();
            foreach(PooledCard pooledCard in cardPool)
            {
                pooledCard.active = false;
                pooledCard.card.gameObject.SetActive(false);
            }
            cardsByInstanceId.Clear();
        }

        private bool TryGetFirstAvailableCard(out PooledCard card)
        {
            card = cardPool.FirstOrDefault(x => !x.active);
            return card != null;
        }

        private PooledCard SpawnNewCard()
        {
            PooledCard newCard = new()
            {
                active = false,
                card = Instantiate(cardPrefab).GetComponent<CardObject>()
            };
            newCard.card.transform.rotation = Quaternion.Euler(defaultRotation);
            cardPool.Add(newCard);
            return newCard;
        }

        private void SetCardTexture(CardObject card)
        {
            card.SetCardBack(CardTextureManager.GetCardBackTexture());
            if(card.RuntimeCard == null)
            {
                card.SetCardFront(CardTextureManager.GetCardBackTexture());
                return;
            }

            string id = card.LibraryCard.GetEnglishSveID();
            card.SetCardFront(CardTextureManager.GetCardTexture(id));
        }

        #endregion
    }
}
