using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace SVESimulator.UI
{
    public class MainMenuCardObject : CardObject
    {
        [field: TitleGroup("Main Menu Data", order: -1f), SerializeField]
        public MainMenuAction Action { get; set; }
    }
}
