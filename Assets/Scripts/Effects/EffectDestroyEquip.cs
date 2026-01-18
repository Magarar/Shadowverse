using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectDestroyEquip", menuName = "Effects/EffectDestroyEquip")]
    public class EffectDestroyEquip: EffectData
    {
        public override void DoEffect(Gamelogic logic, AbilityData ability, Card caster, Card target)
        {
            if (target.CardData.IsEquipment())
            {
                logic.DiscardCard(target);
            }
            else
            {
                Card etarget = logic.GameData.GetCard(target.equippedUid);
                logic.DiscardCard(etarget);
            }
        }
    }
}