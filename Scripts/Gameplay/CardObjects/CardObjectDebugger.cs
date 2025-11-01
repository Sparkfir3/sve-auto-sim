using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CCGKit;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [RequireComponent(typeof(CardObject))]
    public class CardObjectDebugger : MonoBehaviour
    {
        [Serializable]
        private class SerializableRuntimeKeyword
        {
            public int keywordId;
            public int valueId;
            public string keywordName;

            public SerializableRuntimeKeyword(int keyword, int value)
            {
                keywordId = keyword;
                valueId = value;

                keywordName = GameManager.Instance?.config?.keywords[keywordId]?.values[valueId]?.value;
            }
        }

        [SerializeField, Required]
        private CardObject card;

        [Title("Data"), ShowInInspector, ReadOnly, LabelText("Runtime Card Engaged")]
        private bool runtimeEngaged => card && card.Engaged;
        [ShowInInspector, TableList, ReadOnly]
        private List<SerializableRuntimeKeyword> keywords => card && card.RuntimeCard != null
            ? card.RuntimeCard.keywords?.Select(x => new SerializableRuntimeKeyword(x.keywordId, x.valueId))?.ToList()
            : new();

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!card)
                card = GetComponent<CardObject>();
        }
#endif
    }
}
