using CCGKit;

namespace SVESimulator
{
    public class AddNamePassive : SvePassiveEffect
    {
        [StringField("Name", width = 200), Order(1)]
        public string name;

        public override void ApplyPassive(RuntimeCard card, PlayerController player) { }
        public override void RemovePassive(RuntimeCard card, PlayerController player) { }
    }
}
