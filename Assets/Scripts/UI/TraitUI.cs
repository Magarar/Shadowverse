using Data;
using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Shows a trait or custom card stats, add it to the array of a CardUI
    /// </summary>

    public class TraitUI: MonoBehaviour
    {
        public TraitData trait;
        public Image bg;
        public TextMeshProUGUI text;

        void Start()
        {

        }

        public void SetCard(Card card)
        {
            bool hasTrait = card.HasTrait(trait);
            int val = card.GetTraitValue(trait);
            text.text = val.ToString();
            bg.enabled = hasTrait;
            text.enabled = hasTrait;
        }

        public void SetCard(CardData card)
        {
            bool hasTrait = card.HasTrait(trait);
            int val = card.GetStat(trait.id);
            text.text = val.ToString();
            bg.enabled = hasTrait;
            text.enabled = hasTrait;
        }
    }
}