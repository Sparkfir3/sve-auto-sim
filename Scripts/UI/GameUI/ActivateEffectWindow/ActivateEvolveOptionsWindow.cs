using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator.UI
{
    public class ActivateEvolveOptionsWindow : MonoBehaviour
    {
        public const string KEY_STANDARD_EVOLVE         = "Standard";
        public const string KEY_RACE                    = "Race";
        public const string KEY_EVOLVE_POINT            = "EP";
        public const string KEY_SUPER_EVOLVE            = "Super";
        public const string KEY_SUPER_EVOLVE_WITH_EP    = "SuperEP";

        // -----

        [Title("Runtime Data"), SerializeField]
        private MultipleChoiceButton parentButton;

        [Title("Object References"), SerializeField]
        private MultipleChoiceButton standardEvolve;
        [SerializeField]
        private MultipleChoiceButton race;
        [SerializeField]
        private MultipleChoiceButton evolvePoint;
        [SerializeField]
        private MultipleChoiceButton superEvolve;
        [SerializeField]
        private MultipleChoiceButton superEvolveWithEP;

        public RectTransform RectTransform => GetComponent<RectTransform>();

        // ------------------------------

        public void Initialize()
        {
            standardEvolve.OnClickEffect.AddListener(       () => Invoke(KEY_STANDARD_EVOLVE));
            race.OnClickEffect.AddListener(                 () => Invoke(KEY_RACE));
            evolvePoint.OnClickEffect.AddListener(          () => Invoke(KEY_EVOLVE_POINT));
            superEvolve.OnClickEffect.AddListener(          () => Invoke(KEY_SUPER_EVOLVE));
            superEvolveWithEP.OnClickEffect.AddListener(    () => Invoke(KEY_SUPER_EVOLVE_WITH_EP));
            parentButton = null;
            gameObject.SetActive(false);
        }

        public void Open(MultipleChoiceButton parent, bool isEvolve, bool canUseBase, bool hasEvolvePoint, bool hasSuperEvolve)
        {
            parentButton = parent;
            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, parent.RectTransform.anchoredPosition.y);

            standardEvolve.Interactable = canUseBase;
            race.Interactable = canUseBase;
            superEvolve.Interactable = canUseBase && hasSuperEvolve;

            standardEvolve.gameObject.SetActive(isEvolve);
            race.gameObject.SetActive(!isEvolve);
            evolvePoint.gameObject.SetActive(hasEvolvePoint);
            superEvolve.gameObject.SetActive(hasSuperEvolve && isEvolve);
            superEvolveWithEP.gameObject.SetActive(hasSuperEvolve && hasEvolvePoint && isEvolve);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            standardEvolve.Interactable = true;
            race.Interactable = true;
            superEvolve.Interactable = true;
            parentButton = null;
        }

        // ------------------------------

        private void Invoke(string effectName)
        {
            if(!parentButton)
                return;
            parentButton.InvokeAdditionalEffect(effectName);
        }
    }
}
