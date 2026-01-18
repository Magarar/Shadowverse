using System;
using System.Collections.Generic;
using Data;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameLogic
{
    //Represent the current state of a card during the game (data only)
    [Serializable]
    public class Card
    {
        public string cardId;
        public string uid;
        public int playerID;
        public string variantId;

        public Slot slot;
        public bool exhausted;
        public bool canAttackPlayer = false;
        public int damaged;

        public int mana;
        public int attack;
        public int hp = 0;

        public int manaOngoing = 0;
        public int attackOngoing = 0;
        public int hpOngoing = 0;

        public bool hasEvolved = false;
        
        public string equippedUid;
        
        public List<CardStatus> status = new List<CardStatus>();
        public List<CardStatus> ongoingStatus = new List<CardStatus>();
        
        public List<CardTrait> traits = new List<CardTrait>();
        public List<CardTrait> ongoingTraits = new List<CardTrait>();
        
        public List<string> abilities = new List<string>();
        public List<string> ongoingAbilities = new List<string>();

        [NonSerialized] private int hash = 0;
        [NonSerialized] private CardData data= null;
        [NonSerialized] private VariantData vdata= null;
        [NonSerialized] private List<AbilityData> abilitiesData = null;

        public Card(string cardId, string uid, int playerID)
        {
            this.cardId = cardId;
            this.uid = uid;
            this.playerID = playerID;
        }

        public virtual void Refresh()
        {
            exhausted = false;
            canAttackPlayer = true;
        }

        public virtual void ClearOngoing()
        {
            ongoingStatus.Clear();
            ongoingTraits.Clear();
            ClearOngoingAbility();
            attackOngoing = 0;
            hpOngoing = 0;
            manaOngoing = 0;
        }

        public virtual void Clear()
        {
            ClearOngoing();
            Refresh();
            damaged = 0;
            status.Clear();
            SetCard(CardData,VariantData);
            equippedUid = null;
        }

        public virtual int GetAttack()
        {
            return Mathf.Max(attack+attackOngoing,0);
        }

        public virtual int GetHp()
        {
            return Mathf.Max(hp+hpOngoing-damaged,0);
        }

        public virtual int GetHp(int offset)
        {
            return Mathf.Max(hp + hpOngoing - damaged+offset,0);
        }

        public virtual int GetMana()
        {
            return Mathf.Max(mana+manaOngoing,0);
        }

        public void SetCard(CardData icard, VariantData cvariant)
        {
            data = icard;
            cardId = icard.id;
            variantId = cvariant.id;
            attack = icard.attack;
            hp = icard.hp;
            mana = icard.mana;
            SetTraits(icard);
            SetAbilities(icard);
        }

        public void SetTraits(CardData icard)
        {
            traits.Clear();
            foreach (TraitData trait in icard.traitDatas)
                SetTrait(trait.id,0);
            if (icard.traitStats != null)
            {
                foreach (TraitStat stat in icard.traitStats)
                    SetTrait(stat.trait.id, stat.value);
            }
        }
        
        public void SetAbilities(CardData icard)
        {
            abilities.Clear();
            ongoingAbilities.Clear();
            abilitiesData?.Clear();
            foreach (AbilityData ability in icard.abilityDatas)
                AddAbility(ability);
        }

        //------ Custom Traits/Stats ---------
        public void SetTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if(trait!=null)
                trait.value = value;
            else
            {
                trait = new CardTrait(id, value);
                traits.Add(trait);
            }
        }

        public void AddTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if (trait != null)
                trait.value += value;
            else
            {
                SetTrait(id, value);
            }
        }
        
        public void AddOngoingTrait(string id, int value)
        {
            CardTrait trait = GetOngoingTrait(id);
            if (trait != null)
            {
                trait.value += value;
            }
            else
            {
                trait = new CardTrait(id, value);
                ongoingTraits.Add(trait);
            }
        }

        public void RemoveTrait(string id)
        {
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                if (traits[i].id == id)
                    traits.RemoveAt(i);
            }
        }
        
        private CardTrait GetTrait(string id)
        {
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        private CardTrait GetOngoingTrait(string id)
        {
            foreach (CardTrait trait in ongoingTraits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }
        
        public int GetTraitValue(TraitData trait)
        {
            if (trait != null)
                return GetTraitValue(trait.id);
            return 0;
        }

        public int GetTraitValue(string id)
        {
            int val = 0;
            CardTrait trait1 = GetTrait(id);
            CardTrait trait2 = GetOngoingTrait(id);
            if (trait1 != null)
                val += trait1.value;
            if (trait2 != null)
                val += trait2.value;
            return val;
        }
        
        public bool HasTrait(TraitData trait)
        {
            if (trait != null)
                return HasTrait(trait.id);
            return false;
        }

        private bool HasTrait(string id)
        {
            return GetTrait(id) != null || GetOngoingTrait(id) != null;
        }

        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> allTraits = new List<CardTrait>();
            allTraits.AddRange(traits);
            allTraits.AddRange(ongoingTraits);
            return allTraits;
        }
        
        //Alternate names since traits/stats are stored in same var
        public void SetStat(string id, int value)=> SetTrait(id,value);
        public void AddStat(string id, int value) => AddTrait(id, value);
        public void AddOngoingStat(string id, int value) => AddOngoingTrait(id, value);
        public void RemoveStat(string id) => RemoveTrait(id);
        public int GetStatValue(TraitData trait) => GetTraitValue(trait);
        public int GetStatValue(string id) => GetTraitValue(id);
        public bool HasStat(TraitData trait) => HasTrait(trait);
        public bool HasStat(string id) => HasTrait(id);
        public List<CardTrait> GetAllStats() => GetAllTraits();
        
        //------  Status Effects ---------
        public void AddStatus(StatusData status, int value, int duration)
        {
            if (status != null)
                AddStatus(status.effect, value, duration);
        }
        
        public void AddOngoingStatus(StatusData status, int value)
        {
            if (status != null)
                AddOngoingStatus(status.effect, value);
        }

        public void AddStatus(StatusType type, int value, int duration)
        {
            if (type != StatusType.None)
            {
                CardStatus status = GetStatus(type);
                if (status == null)
                {
                    status = new CardStatus(type, value, duration);
                    this.status.Add(status);
                }
                else
                {
                    status.value += value;
                    status.duration = Mathf.Max(status.duration, duration);
                    status.permanent = status.permanent || duration == 0;
                }
            }
        }

        public void AddOngoingStatus(StatusType type, int value)
        {
            if (type != StatusType.None)
            {
                CardStatus status = GetOngoingStatus(type);
                if (status == null)
                {
                    status = new CardStatus(type, value, 0);
                    ongoingStatus.Add(status);
                }
                else
                {
                    status.value += value;
                }
            }
        }
        
        public void RemoveStatus(StatusType type)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].type == type)
                    status.RemoveAt(i);
            }
        }
        
        public List<CardStatus> GetAllStatus()
        {
            List<CardStatus> allStatus = new List<CardStatus>();
            allStatus.AddRange(status);
            allStatus.AddRange(ongoingStatus);
            return allStatus;
        }
        
        public bool HasStatus(StatusType type)
        {
            return GetStatus(type) != null || GetOngoingStatus(type) != null;
        }
        
        public CardStatus GetStatus(StatusType type)
        {
            foreach (CardStatus status in status)
            {
                if (status.type == type)
                    return status;
            }
            return null;
        }

        public CardStatus GetOngoingStatus(StatusType type)
        {
            foreach (CardStatus status in ongoingStatus)
            {
                if (status.type == type)
                    return status;
            }
            return null;
        }
        
        public virtual int GetStatusValue(StatusType type)
        {
            CardStatus status1 = GetStatus(type);
            CardStatus status2 = GetOngoingStatus(type);
            int v1 = status1?.value ?? 0;
            int v2 = status2?.value ?? 0;
            return v1 + v2;
        }
        
        public virtual void ReduceStatusDurations()
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (!status[i].permanent)
                {
                    status[i].duration -= 1;
                    if (status[i].duration <= 0)
                        status.RemoveAt(i);
                }
            }
        }
        
        //----- Abilities ------------
        
        public void AddAbility(AbilityData ability)
        {
            abilities.Add(ability.id);
            if (abilitiesData != null)
                abilitiesData.Add(ability);
        }
        
        public void RemoveAbility(AbilityData ability)
        {
            abilities.Remove(ability.id);
            if (abilitiesData != null)
                abilitiesData.Remove(ability);
        }
        
        public void AddOngoingAbility(AbilityData ability)
        {
            if (!ongoingAbilities.Contains(ability.id) && !abilities.Contains(ability.id))
            {
                ongoingAbilities.Add(ability.id);
                if (abilitiesData != null)
                    abilitiesData.Add(ability);
            }
        }
        
        public void ClearOngoingAbility()
        {
            if (abilitiesData != null)
            {
                for (int i = abilitiesData.Count - 1; i >= 0; i--)
                {
                    AbilityData ability = abilitiesData[i];
                    if (ongoingAbilities.Contains(ability.id))
                        abilitiesData.RemoveAt(i);
                }
            }
            ongoingAbilities.Clear();
        }
        
        public AbilityData GetAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.trigger == trigger)
                    return iability;
            }
            return null;
        }
        
        public bool HasAbility(AbilityData ability)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.id == ability.id)
                    return true;
            }
            return false;
        }
        
        public bool HasAbility(AbilityTrigger trigger)
        {
            AbilityData iability = GetAbility(trigger);
            if (iability != null)
                return true;
            return false;
        }
        
        public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.trigger == trigger && iability.target == target)
                    return true;
            }
            return false;
        }
        
        public bool HasActiveAbility(Game data, AbilityTrigger trigger)
        {
            AbilityData iability = GetAbility(trigger);
            if (iability != null && CanDoAbilities() && iability.AreTriggerConditionsMet(data, this))
                return true;
            return false;
        }

        public bool AreAbilityConditionsMet(AbilityTrigger abilityTrigger, Game data, Card caster, Card trigger)
        {
            foreach (AbilityData ability in GetAbilities())
            {
                if (ability && ability.trigger == abilityTrigger && ability.AreTriggerConditionsMet(data, caster, trigger))
                    return true;
            }
            return false;
        }
        
        public List<AbilityData> GetAbilities()
        {
            //Load abilities data, important to do this here since this array will be null after being sent through networking (cant serialize it)
            if (abilitiesData == null)
            {
                abilitiesData = new List<AbilityData>(abilities.Count + ongoingAbilities.Count);
                for (int i = 0; i < abilities.Count; i++)
                    abilitiesData.Add(AbilityData.Get(abilities[i]));
                for (int i = 0; i < ongoingAbilities.Count; i++)
                    abilitiesData.Add(AbilityData.Get(ongoingAbilities[i]));
            }

            //Return
            return abilitiesData;
        }
        
        //---- Action Check ---------
        public virtual bool CanAttack(bool skipCost = false)
        {
            if(HasStatus(StatusType.Paralysed))
                return false;
            if(!skipCost&&exhausted)
                return false;
            return true;
        }

        public virtual bool CanMove(bool skipCost = false)
        {
            return false;
        }

        public virtual bool CanEvolve()
        {
            return !hasEvolved;
        }

        public virtual bool CanDoActivatedAbilities()
        {
            if(HasStatus(StatusType.Paralysed))
                return false;
            if(HasStatus(StatusType.Silenced))
                return false;
            return true;
        }
        
        public bool CanDoAbilities()
        {
            if (HasStatus(StatusType.Silenced))
                return false;
            return true;
        }
        
        public virtual bool CanDoAnyAction()
        {
            return CanAttack() || CanMove() || CanDoActivatedAbilities();
        }

        public CardData CardData
        {
            get
            {
                if(data==null||data.id!=cardId)
                    data = CardData.Get(cardId);
                return data;
            }
        }

        public VariantData VariantData
        {
            get
            {
                if(vdata==null||vdata.id!=variantId)
                    vdata = VariantData.Get(variantId);
                return vdata;
            }
        }
        
        public CardData Data => CardData; 
        
        public int Hash
        {
            get {
                if (hash == 0)
                    hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return hash;
            }
        }

        public static Card Create(CardData icard, VariantData ivariant, Player iplayer)
        {
            return Create(icard, ivariant, iplayer,GameTool.GenerateRandomID(11, 15));
        }

        private static Card Create(CardData icard, VariantData ivariant, Player iplayer, string uid)
        {
            Card card = new Card(icard.id, uid, iplayer.id);
            card.SetCard(icard, ivariant);
            iplayer.cardsAll[card.uid] = card;
            return card;
        }
        
        public static Card CloneNew(Card source)
        {
            Card card = new Card(source.cardId, source.uid, source.playerID);
            Clone(source, card);
            return card;
        }
        
        //Clone all card variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Card source, Card dest)
        {
            dest.cardId = source.cardId;
            dest.uid = source.uid;
            dest.playerID = source.playerID;
            
            dest.variantId = source.variantId;
            dest.slot = source.slot;
            dest.exhausted = source.exhausted;
            dest.damaged = source.damaged;
            
            dest.canAttackPlayer = source.canAttackPlayer;
            dest.hasEvolved = source.hasEvolved;
            
            dest.mana = source.mana;
            dest.attack = source.attack;
            dest.hp = source.hp;
            
            dest.manaOngoing = source.manaOngoing;
            dest.attackOngoing = source.attackOngoing;
            dest.hpOngoing = source.hpOngoing;
            
            dest.equippedUid = source.equippedUid;
            
            CardTrait.CloneList(source.traits, dest.traits);
            CardTrait.CloneList(source.ongoingTraits, dest.ongoingTraits);
            CardStatus.CloneList(source.status, dest.status);
            CardStatus.CloneList(source.ongoingStatus, dest.ongoingStatus);
            GameTool.CloneList(source.abilities, dest.abilities); 
            GameTool.CloneList(source.ongoingAbilities, dest.ongoingAbilities); 
            GameTool.CloneListRefNull<AbilityData>(source.abilitiesData, ref dest.abilitiesData); //No need to deep copy since AbilityData doesn't change dynamically, its just a reference

        }

        public static void CloneNone(Card source, ref Card dests)
        {
            if (source == null)
            {
                dests = null;
                return;
            }
            if (dests == null)
            {
                dests = CloneNew(source);
                return;
            }
            //Both arent null, just clone
            Clone(source, dests);
        }
        
        //Clone dictionary completely
        public static void CloneDict(Dictionary<string, Card> source, Dictionary<string, Card> dests)
        {
            foreach (KeyValuePair<string, Card> pair in source)
            {
                bool valid = dests.TryGetValue(pair.Key, out Card dest);
                if (valid)
                    Clone(pair.Value, dest);
                else
                    dests[pair.Key] = CloneNew(pair.Value);
            }
        }
        
        //Clone list by keeping references from ref_dict
        public static void CloneListRef(Dictionary<string, Card> refDict, List<Card> source, List<Card> dests)
        {
            for (int i = 0; i < source.Count; i++)
            {
                Card scard = source[i];
                bool valid = refDict.TryGetValue(scard.uid, out Card dest);
                if (valid)
                {
                    if (i < dests.Count)
                        dests[i] = dest;
                    else
                        dests.Add(dest);
                }
            }

            if(dests.Count > source.Count)
                dests.RemoveRange(source.Count, dests.Count - source.Count);
        }
    }
    
    

    [Serializable]
    public class CardStatus
    {
        public StatusType type;
        public int value;
        public int duration = 1;
        public bool permanent = true;
        
        [NonSerialized]
        private StatusData data;

        public CardStatus(StatusType type, int value, int duration)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
            permanent = duration == 0;
        }

        public StatusData StatusData
        {
            get
            {
                if (data == null || data.effect != type)
                    data = StatusData.Get(type);
                return data;
            }
        }
        
        public StatusData Data => StatusData;

        public static CardStatus CloneNew(CardStatus copy)
        {
            CardStatus status = new CardStatus(copy.type, copy.value, copy.duration);
            status.permanent = copy.permanent;
            return status;
        }

        public static void Clone(CardStatus sources, CardStatus dest)
        {
            dest.type = sources.type;
            dest.value = sources.value;
            dest.duration = sources.duration;
            dest.permanent = sources.permanent;
        }

        public static void CloneList(List<CardStatus> sources, List<CardStatus> dest)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if (i < dest.Count)
                {
                    Clone(sources[i],dest[i]);
                }
                else
                {
                    dest.Add(CloneNew(sources[i]));
                }
            }

            if (dest.Count > sources.Count)
            {
                dest.RemoveRange(sources.Count, dest.Count - sources.Count);
            }
        }
    }

    [Serializable]
    public class CardTrait
    {
        public string id;
        public int value;
        
        [NonSerialized]
        private TraitData data = null;

        public CardTrait(string id, int value)
        {
            this.id = id;
            this.value = value;
        }
        
        public CardTrait(TraitData trait, int value)
        {
            this.id = trait.id;
            this.value = value;
        }
        
        public TraitData TraitData
        {
            get
            {
                if (data == null || data.id != id)
                    data = TraitData.Get(id);
                return data;
            }
        }
        
        public TraitData Data => TraitData;
        
        public static CardTrait CloneNew(CardTrait copy)
        {
            CardTrait status = new CardTrait(copy.id, copy.value);
            return status;
        }

        public static void Clone(CardTrait sources, CardTrait dest)
        {
            dest.id = sources.id;
            dest.value = sources.value;
        }
        
        public static void CloneList(List<CardTrait> source, List<CardTrait> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                    Clone(source[i], dest[i]);
                else
                    dest.Add(CloneNew(source[i]));
            }

            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
        
        
    }
}