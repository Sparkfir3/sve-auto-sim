using CCGKit;

namespace SVESimulator
{
    public class AddTraitPassive : SvePassiveEffect
    {
        [StringField("Trait", width = 200), Order(1)]
        public string trait;

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }
        public override void RemovePassive(RuntimeCard card, PlayerController player) { }
    }
}
