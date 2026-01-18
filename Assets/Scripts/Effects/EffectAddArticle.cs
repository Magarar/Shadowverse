using Data;
using GameLogic;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu(fileName = "New EffectAddArticle", menuName = "Effects/EffectAddArticle")]
    public class EffectAddArticle: EffectData
    {
        public ArticleData article;

        public override void DoEffect(Gamelogic logic, AbilityData abilityData, Card caster, Player target)
        {
            base.DoEffect(logic, abilityData, caster, target);
            logic.AddArticle(target, article);
        }
    }
}