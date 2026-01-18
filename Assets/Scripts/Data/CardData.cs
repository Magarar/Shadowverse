using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;


namespace Data
{
    /// <summary>
    /// 定义所有卡片数据
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "Data/CardData")]
    public class CardData:ScriptableObject
    {
        public string id;

        [Header("Display")] 
        public string title;
        public Sprite artFull;
        public Sprite artBoard;
        public Sprite artEvolveBoard;

        [Header("Stats")] 
        public CardType type;
        public TeamData team;
        public RarityData rarity;
        public int mana;
        public int hp;
        public int attack;
        
        [Header("Traits")]
        public TraitData[] traitDatas;
        public TraitStat[] traitStats;
        
        
        [Header("Abilities")]
        public AbilityData[] abilityDatas;
        
        [Header("Variant")]
        public VariantData variant;
        
        [Header("Card Text")]
        [TextArea(3,5)]
        public string text;
        
        [Header("Description")]
        [TextArea(5, 10)]
        public string desc;
        
        [Header("FX")]
        public GameObject cardSpawnFX;
        public GameObject cardDestroyFX;
        public GameObject cardAttackFX;
        public GameObject cardDamageFX;
        public GameObject cardIdleFX;
        public AudioClip spawnAudio;
        public AudioClip destroyAudio;
        public AudioClip attackAudio;
        public AudioClip damageAudio;
        public AudioClip evolutionAudio;
        public AudioClip exEvolutionAudio;
        
        [Header("Availability")]
        public bool deckbuilding = true;
        public int cost = 100;
        public PackData[] packs;
        
        public static List<CardData> cardList = new List<CardData>(); //Faster access in loops
        public static Dictionary<string, CardData> cardDict = new Dictionary<string, CardData>();
        
        #region fuction

        public static void Load(string folder = "")
        {
            // if (cardList.Count == 0)
            // {
            //     cardList.AddRange(Resources.LoadAll<CardData>(folder));
            //     foreach (CardData card in cardList)
            //     {
            //         cardDict.Add(card.id, card);
            //     }
            // }

            if (cardList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "骑士";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    CardData card = asset as CardData;
                    cardList.Add(card);
                    cardDict.Add(card.id, card);
                }
                Debug.Log($"Loaded {cardList.Count} cards");
            }
        }

        public Sprite GetBoardArt(VariantData variant) => artBoard;
        
        public Sprite GetEvolveBoardArt(VariantData variant) => artEvolveBoard;
        public Sprite GetFullArt(VariantData variant) => artFull;
        public string GetTitle() => title;
        public string GetText() => text;
        public string GetDesc() => desc;

        public string GetTypeID()
        {
            switch (type)
            {
                case CardType.None:
                    return "none";
                    break;
                case CardType.Hero:
                    return "hero";
                    break;
                case CardType.Character:
                    return "creature";
                    break;
                case CardType.Spell:
                    return "spell";
                    break;
                case CardType.Artifact:
                    return "artifact";
                    break;
                case CardType.Secret:
                    return "secret";
                    break;
                case CardType.Equipment:
                    return "equipment";
                    break;
                default:
                    return "";
            }
        }
        
        public string GetAbilitiesDesc()
        {
            string txt = "";
            foreach (AbilityData ability in abilityDatas)
            {
                if (!string.IsNullOrWhiteSpace(ability.desc))
                {
                    txt += "<b>";
                    if (!string.IsNullOrWhiteSpace(ability.GetTitle()))
                    {
                        txt += $"{ability.GetTitle()}:</b>";
                    }
                    else
                    {
                        txt += "</b>";
                    }

                    txt += ability.GetDesc(this) + "\n";
                }
            }
            return txt;
        }
        
        public bool IsCharacter() => type == CardType.Character;
        public bool IsSecret() => type == CardType.Secret;
        public bool IsBoardCard() => type == CardType.Character || type == CardType.Artifact;
        public bool IsRequireTarget() => type == CardType.Equipment||IsRequireTargetSpell();
        public bool IsEquipment() => type == CardType.Equipment;
        public bool IsDynamicManaCost() => mana >99;

        public bool IsRequireTargetSpell()
        {
            return type == CardType.Spell && HasAbility(AbilityTrigger.OnPlay, AbilityTarget.PlayTarget);
        }

        public bool HasTrait(string traitID)
        {
            return traitDatas.Any(traitData => traitData.id == traitID);
        }

        public bool HasTrait(TraitData traitData)
        {
            if(traitData != null)
                return HasTrait(traitData.id);
            return false;
        }

        public bool HasStat(string trait)
        {
            if(traitStats== null)
                return false;
            return traitStats.Any(traitStat => traitStat.trait.id == trait);
        }

        public bool HasStat(TraitData traitData)
        {
            if(traitData != null)
                return HasStat(traitData.id);
            return false;
        }

        public int GetStat(string traitID)
        {
            if(traitStats== null)
                return 0;
            return (from traitStat in traitStats where traitStat.trait.id == traitID select traitStat.value).FirstOrDefault();
        }
        
        public int GetStat(TraitData trait)
        {
            if(trait != null)
                return GetStat(trait.id);
            return 0;
        }

        public bool HasAbility(AbilityData abilityData)
        {
            return abilityDatas.Any(ability => ability==abilityData);
        }
        
        public bool HasAbility(AbilityTrigger trigger)
        {
            return abilityDatas.Any(ability => ability && ability.trigger == trigger);
        }

        public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
        {
            return abilityDatas.Any(ability => ability && ability.trigger == trigger && ability.target == target);
        }

        public AbilityData GetAbility(AbilityTrigger trigger)
        {
            return abilityDatas.FirstOrDefault(ability => ability && ability.trigger == trigger);
        }

        public bool HasPack(PackData pack)
        {
            return packs.Any(apack => apack == pack);
        }

        public bool HasVariant(VariantData variant)
        {
            return this.variant != null && this.variant.id == variant.id;
        }

        public VariantData GetVariant()
        {
            return variant==null ? VariantData.GetDefault() : variant;
        }

        public static CardData Get(string id)
        {
            if(id==null)
                return null;
            bool success = cardDict.TryGetValue(id, out CardData cardData);
            return success ? cardData : null;
        }

        public static List<CardData> GetAllDeckBuilding()
        {
            return GetAll().Where(cardData => cardData.deckbuilding).ToList();
        }

        public static List<CardData> GetAll()
        {
            return cardList;
        }

        public static List<CardData> GetAll(PackData pack)
        {
            return GetAll().Where(cardData => cardData.HasPack(pack)).ToList();
        }
        
        #endregion
    }

    public enum CardType
    {
        None = 0,
        Hero = 5,
        Character = 10,
        Spell = 20,
        Artifact = 30,
        Secret = 40,
        Equipment = 50,
    }

    
}