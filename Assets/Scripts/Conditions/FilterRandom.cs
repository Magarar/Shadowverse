using System.Collections.Generic;
using Data;
using GameLogic;
using Unit;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "New FilterRandom", menuName = "Filters/FilterRandom")]
    public class FilterRandom: FilterData
    {
        public int amount = 1; //Number of random targets selected

        public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<CardData> FilterTargets(Game data, AbilityData ability, Card caster, List<CardData> source, List<CardData> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }
    }
}