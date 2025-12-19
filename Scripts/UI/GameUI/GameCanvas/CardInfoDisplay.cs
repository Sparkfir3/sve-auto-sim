using System.Linq;
using CCGKit;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sparkfire.Utility;

namespace SVESimulator.UI
{
    public class CardInfoDisplay : MonoBehaviour
    {
        [TitleGroup("Runtime Data"), SerializeField, ReadOnly, LabelText("Current Card ID")]
        private string currentCardId;

        [TitleGroup("Object References"), SerializeField]
        private RawImage cardImage;
        [SerializeField]
        private TextMeshProUGUI textBoxName;
        [SerializeField, LabelText("Text Box ID")]
        private TextMeshProUGUI textBoxID;
        [SerializeField]
        private TextMeshProUGUI textBoxMainText;
        [SerializeField]
        private TextMeshProUGUI textBoxTrait;

        private int leaderTypeId;

        // ------------------------------

        #region Unity Functions & Controls

        private void Awake()
        {
            currentCardId = "";
            leaderTypeId = GameManager.Instance?.config?.cardTypes?.FirstOrDefault(x => x.name.Equals(SVEProperties.CardTypes.Leader))?.id ?? 5;
        }

        public void Display(CardObject card)
        {
            if(card && card.IsVisible)
                Display(card.LibraryCard);
            else
                Hide();
        }

        public void Display(Card libraryCard)
        {
            if(libraryCard == null)
            {
                Hide();
                return;
            }
            gameObject.SetActive(true);
            string id = libraryCard.GetStringProperty(SVEProperties.CardStats.ID);
            if(currentCardId.Equals(id))
                return;
            LibraryCardCache.CacheCard(libraryCard);
            currentCardId = id;

            SetImage(CardTextureManager.GetCardTexture(id));
            SetNameAndIDText(LibraryCardCache.GetName(libraryCard.id), LibraryCardCache.GetDisplayId(libraryCard.id));

            if(libraryCard.cardTypeId == leaderTypeId)
            {
                SetMainText("");
                SetTraitText("");
                return;
            }
            SetMainText(LibraryCardCache.GetCardText(libraryCard.id));
            SetTraitText(LibraryCardCache.GetCardTrait(libraryCard.id));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

        // ------------------------------

        #region Set Info

        private void SetImage(Texture texture)
        {
            cardImage.texture = texture;
        }

        private void SetNameAndIDText(string cardName, string id)
        {
            textBoxName.text = cardName;
            textBoxID.text = id;
        }

        private void SetMainText(string mainText)
        {
            textBoxMainText.gameObject.SetActive(!mainText.IsNullOrWhiteSpace());
            textBoxMainText.text = mainText;
        }

        private void SetTraitText(string traits)
        {
            textBoxTrait.gameObject.SetActive(!traits.IsNullOrWhiteSpace());
            textBoxTrait.text = traits;
        }

        #endregion
    }
}
