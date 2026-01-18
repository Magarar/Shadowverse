using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Effects;
using GameLogic;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace Data
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Data/Ability")]
    public class AbilityData:ScriptableObject
    {
        public string id;
        
        [Header("Trigger")]
        public AbilityTrigger trigger;
        public ConditionData[] conditionsTrigger; //在卡上检查触发能力的条件（通常是施法者）
        
        [Header("Target")]
        public AbilityTarget target;
        public ConditionData[] conditionsTarget;
        public FilterData[] filtersTarget;
        
        [Header("Effect")]
        public EffectData[] effects;
        public StatusData[] status;   
        public int value;   
        public int duration;     
        
        [FormerlySerializedAs("chainAbility")] [Header("ChainAbility")]
        public AbilityData[] chainAbilities;
        
        [Header("Activated Ability")]
        public int manaCost;
        public bool exhaust;

        [Header("FX")] 
        public GameObject boardFX;
        public GameObject casterFX;
        public GameObject targetFX;
        public GameObject projectileFX;
        public AudioClip castAudio;
        public AudioClip targetAudio;
        public bool chargeTarget;
        
        [Header("Text")]
        public string title;
        [TextArea(5, 7)]
        public string desc;
        
        public static List<AbilityData> abilityList = new List<AbilityData>();
        public static Dictionary<string, AbilityData> abilityDict = new Dictionary<string, AbilityData>();

        #region fuction
        public static void Load(string folder = "")
        {
            
            
            // if (abilityList.Count == 0)
            // {
            //     abilityList.AddRange(Resources.LoadAll<AbilityData>(folder));
            //     foreach (AbilityData ability in abilityList)
            //     {
            //         abilityDict.Add(ability.id, ability);
            //     }
            // }

            if (abilityList.Count == 0)
            {
                var package = YooAssets.GetPackage("DefaultPackage");
                var location = "AddArticle";
                var handle = package.LoadAllAssetsSync(location);
                foreach (var asset in handle.AllAssetObjects)
                {
                    AbilityData ability = asset as AbilityData;
                    abilityList.Add(ability);
                    abilityDict.Add(ability.id, ability);
                }
                Debug.Log("Load AbilityData: " + abilityList.Count);
            }
        }

        public string GetTitle() => title;
        public string GetDesc() => desc;

        public string GetDesc(CardData cardData)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", cardData.title);
            dsc = dsc.Replace("<value>", value.ToString());
            dsc = dsc.Replace("<duration>", duration.ToString());
            return dsc;
        }
        
        //触发能力的一般条件
        public bool AreTriggerConditionsMet(Game data,Card caster)
        {
            return AreTriggerConditionsMet(data, caster, caster); //Triggerer is the caster
        }

        //某些技能是由另一张牌（PlayOther）引起的，否则大多数时候触发者是施法者，请检查触发者的状态
        public bool AreTriggerConditionsMet(Game data, Card caster, Card triggerCard)
        {
            foreach (var condition in conditionsTrigger)
            {
                if (!condition.IsTriggerConditionMet(data, this, caster))
                {
                    return false;
                }
                if (!condition.IsTargetConditionMet(data, this, caster, triggerCard))
                    return false;
            }
            return true;
        }
        
        //某些技能是由对玩家的动作引起的（攻击玩家时为OnFight），请检查该玩家的状态
        public bool AreTriggerConditionsMet(Game data, Card caster, Player triggerPlayer)
        {
            foreach (var condition in conditionsTrigger)
            {
                if (!condition.IsTriggerConditionMet(data, this, caster))
                    return false;
                if (!condition.IsTargetConditionMet(data, this, caster, triggerPlayer))
                    return false;
            }
            return true;
        }
        
        public bool AreTriggerConditionsMet(Game data,Article caster)
        {
            return AreTriggerConditionsMet(data, caster, caster); //Triggerer is the caster
        }
        
        public bool AreTriggerConditionsMet(Game data, Article caster, Article triggerArticle)
        {
            foreach (var condition in conditionsTrigger)
            {
                if (!condition.IsTriggerConditionMet(data, this, caster))
                {
                    return false;
                }
                if (!condition.IsTargetConditionMet(data, this, caster, triggerArticle))
                    return false;
            }
            return true;
        }
        
        public bool AreTriggerConditionsMet(Game data, Article caster, Card triggerCard)
        {
            foreach (var condition in conditionsTrigger)
            {
                if (!condition.IsTriggerConditionMet(data, this, caster))
                {
                    return false;
                }
                if (!condition.IsTargetConditionMet(data, this, caster, triggerCard))
                    return false;
            }
            return true;
        }
        
        public bool AreTriggerConditionsMet(Game data, Article caster, Player triggerPlayer)
        {
            foreach (var condition in conditionsTrigger)
            {
                if (!condition.IsTriggerConditionMet(data, this, caster))
                    return false;
                if (!condition.IsTargetConditionMet(data, this, caster, triggerPlayer))
                    return false;
            }
            return true;
        }
        
        //检查卡目标是否有效
        public bool AreTargetConditionsMet(Game data, Card caster, Card triggerCard)
        {
            foreach (var condition in conditionsTarget)
            {
                if(condition!=null && !condition.IsTargetConditionMet(data, this, caster, triggerCard))
                    return false;
            }
            return true;
        }
        
        //Check if the player target is valid
        public bool AreTargetConditionsMet(Game data, Card caster, Player triggerPlayer)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, triggerPlayer))
                    return false;
            }
            return true;
        }
        
        //Check if the slot target is valid
        public bool AreTargetConditionsMet(Game data, Card caster, Slot targetSlot)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, targetSlot))
                    return false;
            }
            return true;
        }
        
        //检查卡数据目标是否有效
        public bool AreTargetConditionsMet(Game data, Card caster, CardData targetCard)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, targetCard))
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool AreTargetConditionsMet(Game data, Article caster, Card triggerCard)
        {
            foreach (var condition in conditionsTarget)
            {
                if(condition!=null && !condition.IsTargetConditionMet(data, this, caster, triggerCard))
                    return false;
            }
            return true;
        }
        
        public bool AreTargetConditionsMet(Game data, Article caster, Player triggerPlayer)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, triggerPlayer))
                    return false;
            }
            return true;
        }
        
        public bool AreTargetConditionsMet(Game data,Article caster, Slot targetSlot)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, targetSlot))
                    return false;
            }
            return true;
        }
        
        public bool AreTargetConditionsMet(Game data, Article caster, CardData targetCard)
        {
            foreach (var condition in conditionsTarget)
            {
                if (condition != null && !condition.IsTargetConditionMet(data, this, caster, targetCard))
                {
                    return false;
                }
            }
            return true;
        }
        
        //CanTarget类似于AreTargetConditionsMet，但仅适用于板上的目标，并具有额外的板上条件
        public bool CanTarget(Game data, Card caster, Card _target)
        {
            if (_target.HasStatus(StatusType.Stealth))
                return false; //Hidden

            if (_target.HasStatus(StatusType.SpellImmunity))
                return false; //Spell immunity

            bool conditionMatch = AreTargetConditionsMet(data, caster, _target);
            return conditionMatch;
        }
        
        //CanTarget 检查其他限制，通常用于选择目标或播放目标功能
        public bool CanTarget(Game data, Card caster, Player _target)
        {
            bool conditionMatch = AreTargetConditionsMet(data, caster, _target);
            return conditionMatch;
        }
        
        public bool CanTarget(Game data, Card caster, Slot _target)
        {
            return AreTargetConditionsMet(data, caster, _target); //No additional conditions for slots
        }
        
        public bool CanTarget(Game data, Article caster, Card _target)
        {
            if (_target.HasStatus(StatusType.Stealth))
                return false; //Hidden

            if (_target.HasStatus(StatusType.SpellImmunity))
                return false; //Spell immunity

            bool conditionMatch = AreTargetConditionsMet(data, caster, _target);
            return conditionMatch;
        }
        
        //CanTarget 检查其他限制，通常用于选择目标或播放目标功能
        public bool CanTarget(Game data, Article caster, Player _target)
        {
            bool conditionMatch = AreTargetConditionsMet(data, caster, _target);
            return conditionMatch;
        }
        
        public bool CanTarget(Game data, Article caster, Slot _target)
        {
            return AreTargetConditionsMet(data, caster, _target); //No additional conditions for slots
        }
        
        //检查目标数组在过滤后是否有目标，用于支持CardSelector中的过滤器
        public bool IsCardSelectionValid(Game data, Card caster, Card target, ListSwap<Card> cardArray = null)
        {
            List<Card> targets = GetCardTargets(data, caster, cardArray);
            return targets.Contains(target); //Card is still in array after filtering
        }
        
        public bool IsCardSelectionValid(Game data, Article caster, Card target, ListSwap<Card> cardArray = null)
        {
            List<Card> targets = GetCardTargets(data, caster, cardArray);
            return targets.Contains(target); //Card is still in array after filtering
        }
        
        public void DoEffects(Gamelogic logic, Card caster)
        {
            foreach(EffectData effect in effects)
                effect?.DoEffect(logic, this, caster);
        }
        
        public void DoEffects(Gamelogic logic, Card caster, Card target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, target);
            foreach(StatusData stat in status)
                target.AddStatus(stat, value, duration);
        }
        
        public void DoEffects(Gamelogic logic, Card caster, Player target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, target);
            foreach (StatusData stat in status)
                target.AddStatus(stat, value, duration);
        }
        
        public void DoEffects(Gamelogic logic, Card caster, Slot target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, target);
        }
        
        public void DoEffects(Gamelogic logic, Card caster, CardData target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, target);
        }
        
        public void DoOngoingEffects(Gamelogic logic, Card caster, Card target)
        {
            foreach (EffectData effect in effects)
                effect?.DoOngoingEffect(logic, this, caster, target);
            foreach (StatusData stat in status)
                target.AddOngoingStatus(stat, value);
        }
        
        public void DoOngoingEffects(Gamelogic logic, Card caster, Player target)
        {
            foreach (EffectData effect in effects)
                effect?.DoOngoingEffect(logic, this, caster, target);
            foreach (StatusData stat in status)
                target.AddOngoingStatus(stat, value);
        }
        
        public bool HasEffect<T>() where T : EffectData
        {
            foreach (EffectData eff in effects)
            {
                if (eff != null && eff is T)
                    return true;
            }
            return false;
        }
        
        public bool HasStatus(StatusType type)
        {
            foreach (StatusData sta in status)
            {
                if (sta != null && sta.effect == type)
                    return true;
            }
            return false;
        }
        
        public int GetDamage()
        {
            int damage = 0;
            foreach (EffectData eff in effects)
            {
                if (eff is EffectDamage)
                {
                    damage += this.value;
                }
            }
            return damage;
        }
        
        private void AddValidCards(Game data, Card caster, List<Card> source, List<Card> targets)
        {
            foreach (Card card in source)
            {
                if (AreTargetConditionsMet(data, caster, card))
                    targets.Add(card);
            }
        }
        
        private void AddValidCards(Game data, Article caster, List<Card> source, List<Card> targets)
        {
            foreach (Card card in source)
            {
                if (AreTargetConditionsMet(data, caster, card))
                    targets.Add(card);
            }
        }
        
        //返回卡片目标，memory_array用于优化并避免分配新内存
        public List<Card> GetCardTargets(Game data, Card caster, ListSwap<Card> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Card>();
            List<Card> targets = memoryArray.Get();
            
            if (target == AbilityTarget.Self)
            {
                if (AreTargetConditionsMet(data, caster, caster))
                    targets.Add(caster);
            }
            
            if (target == AbilityTarget.AllCardsBoard || target == AbilityTarget.SelectTarget)
            {
                foreach (Player player in data.players)
                {
                    foreach (Card card in player.cardsBoard)
                    {
                        if (AreTargetConditionsMet(data, caster, card))
                            targets.Add(card);
                    }
                }
            }
            
            if (target == AbilityTarget.AllCardsHand)
            {
                foreach (Player player in data.players)
                {
                    foreach (Card card in player.cardsHand)
                    {
                        if (AreTargetConditionsMet(data, caster, card))
                            targets.Add(card);
                    }
                }
            }
            
            if (target == AbilityTarget.AllCardsAllPiles || target == AbilityTarget.CardSelector)
            {
                foreach (Player player in data.players)
                {
                    AddValidCards(data, caster, player.cardsDeck, targets);
                    AddValidCards(data, caster, player.cardsDiscard, targets);
                    AddValidCards(data, caster, player.cardsHand, targets);
                    AddValidCards(data, caster, player.cardsSecret, targets);
                    AddValidCards(data, caster, player.cardsBoard, targets);
                    AddValidCards(data, caster, player.cardsEquip, targets);
                    AddValidCards(data, caster, player.cardsTemp, targets);
                }
            }
            
            if (target == AbilityTarget.LastPlayed)
            {
                Card target = data.GetCard(data.lastPlayed);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastDestroyed)
            {
                Card target = data.GetCard(data.lastDestroyed);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastTargeted)
            {
                Card target = data.GetCard(data.lastTarget);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastSummoned)
            {
                Card target = data.GetCard(data.lastSummoned);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.AbilityTriggerer)
            {
                Card target = data.GetCard(data.abilityTrigger);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                {
                    targets.Add(target);
                }
            }
            
            if (target == AbilityTarget.EquippedCard)
            {
                if (caster.CardData.IsEquipment())
                {
                    //Get bearer of the equipment
                    Player player = data.GetPlayer(caster.playerID);
                    Card target = player.GetBearerCard(caster);
                    if (target != null && AreTargetConditionsMet(data, caster, target))
                        targets.Add(target);
                }
                else if(caster.equippedUid != null)
                {
                    //Get equipped card
                    Card target = data.GetCard(caster.equippedUid);
                    if (target != null && AreTargetConditionsMet(data, caster, target))
                        targets.Add(target);
                }
            }
            
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }
            
            return targets;
        }
        
        //返回玩家目标，memory_array用于优化，避免分配新内存
        public List<Player> GetPlayerTargets(Game data, Card caster, ListSwap<Player> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Player>();
            List<Player> targets = memoryArray.Get();
            if (target == AbilityTarget.PlayerSelf)
            {
                Player player = data.GetPlayer(caster.playerID);
                targets.Add(player);
            }
            else if (target == AbilityTarget.PlayerOpponent)
            {
                for (int tp = 0; tp < data.players.Length; tp++)
                {
                    if (tp != caster.playerID)
                    {
                        Player oplayer = data.players[tp];
                        targets.Add(oplayer);
                    }
                }
            }
            else if (target == AbilityTarget.AllPlayers)
            {
                foreach (Player player in data.players)
                {
                    if (AreTargetConditionsMet(data, caster, player))
                        targets.Add(player);
                }
                
            }
            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }
            return targets;
        }
        
        //Return slot targets,  memory_array is used for optimization and avoid allocating new memory
        public List<Slot> GetSlotTargets(Game data, Card caster, ListSwap<Slot> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Slot>();

            List<Slot> targets = memoryArray.Get();

            if (target == AbilityTarget.AllSlots)
            {
                List<Slot> slots = Slot.GetAll();
                foreach (Slot slot in slots)
                {
                    if (AreTargetConditionsMet(data, caster, slot))
                        targets.Add(slot);
                }
            }

            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }

            return targets;
        }
        
        public List<CardData> GetCardDataTargets(Game data, Card caster, ListSwap<CardData> memoryArray = null)
        {
            memoryArray ??= new ListSwap<CardData>();

            List<CardData> targets = memoryArray.Get();

            if (target == AbilityTarget.AllCardData)
            {
                foreach (CardData card in CardData.GetAll())
                {
                    if (AreTargetConditionsMet(data, caster, card))
                        targets.Add(card);
                }
            }

            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }

            return targets;
        }
        
        //Articles
        public List<Card> GetCardTargets(Game data, Article caster, ListSwap<Card> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Card>();
            List<Card> targets = memoryArray.Get();
            
            // if (target == AbilityTarget.Self)
            // {
            //     if (AreTargetConditionsMet(data, caster, caster))
            //         targets.Add(caster);
            // }
            
            if (target == AbilityTarget.AllCardsBoard || target == AbilityTarget.SelectTarget)
            {
                foreach (Player player in data.players)
                {
                    foreach (Card card in player.cardsBoard)
                    {
                        if (AreTargetConditionsMet(data, caster, card))
                            targets.Add(card);
                    }
                }
            }
            
            if (target == AbilityTarget.AllCardsHand)
            {
                foreach (Player player in data.players)
                {
                    foreach (Card card in player.cardsHand)
                    {
                        if (AreTargetConditionsMet(data, caster, card))
                            targets.Add(card);
                    }
                }
            }
            
            if (target == AbilityTarget.AllCardsAllPiles || target == AbilityTarget.CardSelector)
            {
                foreach (Player player in data.players)
                {
                    AddValidCards(data, caster, player.cardsDeck, targets);
                    AddValidCards(data, caster, player.cardsDiscard, targets);
                    AddValidCards(data, caster, player.cardsHand, targets);
                    AddValidCards(data, caster, player.cardsSecret, targets);
                    AddValidCards(data, caster, player.cardsBoard, targets);
                    AddValidCards(data, caster, player.cardsEquip, targets);
                    AddValidCards(data, caster, player.cardsTemp, targets);
                }
            }
            
            if (target == AbilityTarget.LastPlayed)
            {
                Card target = data.GetCard(data.lastPlayed);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastDestroyed)
            {
                Card target = data.GetCard(data.lastDestroyed);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastTargeted)
            {
                Card target = data.GetCard(data.lastTarget);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.LastSummoned)
            {
                Card target = data.GetCard(data.lastSummoned);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (target == AbilityTarget.AbilityTriggerer)
            {
                Card target = data.GetCard(data.abilityTrigger);
                if (target != null && AreTargetConditionsMet(data, caster, target))
                    targets.Add(target);
            }
            
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }
            
            return targets;
        }
        
        //返回玩家目标，memory_array用于优化，避免分配新内存
        public List<Player> GetPlayerTargets(Game data, Article caster, ListSwap<Player> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Player>();
            List<Player> targets = memoryArray.Get();
            if (target == AbilityTarget.PlayerSelf)
            {
                Player player = data.GetPlayer(caster.playerID);
                targets.Add(player);
            }
            else if (target == AbilityTarget.PlayerOpponent)
            {
                for (int tp = 0; tp < data.players.Length; tp++)
                {
                    if (tp != caster.playerID)
                    {
                        Player oplayer = data.players[tp];
                        targets.Add(oplayer);
                    }
                }
            }
            else if (target == AbilityTarget.AllPlayers)
            {
                foreach (Player player in data.players)
                {
                    if (AreTargetConditionsMet(data, caster, player))
                        targets.Add(player);
                }
                
            }
            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }
            return targets;
        }
        
        //Return slot targets,  memory_array is used for optimization and avoid allocating new memory
        public List<Slot> GetSlotTargets(Game data, Article caster, ListSwap<Slot> memoryArray = null)
        {
            memoryArray ??= new ListSwap<Slot>();

            List<Slot> targets = memoryArray.Get();

            if (target == AbilityTarget.AllSlots)
            {
                List<Slot> slots = Slot.GetAll();
                foreach (Slot slot in slots)
                {
                    if (AreTargetConditionsMet(data, caster, slot))
                        targets.Add(slot);
                }
            }

            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }

            return targets;
        }
        
        public List<CardData> GetCardDataTargets(Game data, Article caster, ListSwap<CardData> memoryArray = null)
        {
            memoryArray ??= new ListSwap<CardData>();

            List<CardData> targets = memoryArray.Get();

            if (target == AbilityTarget.AllCardData)
            {
                foreach (CardData card in CardData.GetAll())
                {
                    if (AreTargetConditionsMet(data, caster, card))
                        targets.Add(card);
                }
            }

            //Filter targets
            if (filtersTarget != null && targets.Count > 0)
            {
                foreach (FilterData filter in filtersTarget)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, targets, memoryArray.GetOther(targets));
                }
            }

            return targets;
        }
        
        // 检查是否有有效目标，如果没有，AI不会尝试施放激活能力
        public bool HasValidSelectTarget(Game gameData, Card caster)
        {
            if (target == AbilityTarget.SelectTarget)
            {
                if (HasValidBoardCardTarget(gameData, caster))
                    return true;
                if (HasValidPlayerTarget(gameData, caster))
                    return true;
                if (HasValidSlotTarget(gameData, caster))
                    return true;
                return false;
            }

            if (target == AbilityTarget.CardSelector)
            {
                if (HasValidCardTarget(gameData, caster))
                    return true;
                return false;
            }

            if (target == AbilityTarget.ChoiceSelector)
            {
                foreach (AbilityData choice in chainAbilities)
                {
                    if(choice.AreTriggerConditionsMet(gameData, caster))
                        return true;
                }
                return false;
            }

            return true; //Not selecting, valid
        }
        
        public bool HasValidBoardCardTarget(Game gameData, Card caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                for (int c = 0; c < player.cardsBoard.Count; c++)
                {
                    Card card = player.cardsBoard[c];
                    if (CanTarget(gameData, caster, card))
                        return true;
                }
            }
            return false;
        }
        
        public bool HasValidCardTarget(Game gameData, Card caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                bool v1 = HasValidCardTarget(gameData, caster, player.cardsDeck);
                bool v2 = HasValidCardTarget(gameData, caster, player.cardsDiscard);
                bool v3 = HasValidCardTarget(gameData, caster, player.cardsHand);
                bool v4 = HasValidCardTarget(gameData, caster, player.cardsBoard);
                bool v5 = HasValidCardTarget(gameData, caster, player.cardsEquip);
                bool v6 = HasValidCardTarget(gameData, caster, player.cardsSecret);
                bool v7 = HasValidCardTarget(gameData, caster, player.cardsTemp);
                if (v1 || v2 || v3 || v4 || v5 || v6 || v7)
                    return true;
            }
            return false;
        }
        
        public bool HasValidCardTarget(Game gameData, Card caster, List<Card> list)
        {
            for (int c = 0; c < list.Count; c++)
            {
                Card card = list[c];
                if (AreTargetConditionsMet(gameData, caster, card))
                    return true;
            }
            return false;
        }
        
        public bool HasValidPlayerTarget(Game gameData, Card caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                if (CanTarget(gameData, caster, player))
                    return true;
            }
            return false;
        }
        
        public bool HasValidSlotTarget(Game gameData, Card caster)
        {
            foreach (Slot slot in Slot.GetAll())
            {
                if (CanTarget(gameData, caster, slot))
                    return true;
            }
            return false;
        }
        
        //Article
        
         public bool HasValidSelectTarget(Game gameData, Article caster)
        {
            if (target == AbilityTarget.SelectTarget)
            {
                if (HasValidBoardCardTarget(gameData, caster))
                    return true;
                if (HasValidPlayerTarget(gameData, caster))
                    return true;
                if (HasValidSlotTarget(gameData, caster))
                    return true;
                return false;
            }

            if (target == AbilityTarget.CardSelector)
            {
                if (HasValidCardTarget(gameData, caster))
                    return true;
                return false;
            }

            if (target == AbilityTarget.ChoiceSelector)
            {
                foreach (AbilityData choice in chainAbilities)
                {
                    if(choice.AreTriggerConditionsMet(gameData, caster))
                        return true;
                }
                return false;
            }

            return true; //Not selecting, valid
        }
        
        public bool HasValidBoardCardTarget(Game gameData, Article caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                for (int c = 0; c < player.cardsBoard.Count; c++)
                {
                    Card card = player.cardsBoard[c];
                    if (CanTarget(gameData, caster, card))
                        return true;
                }
            }
            return false;
        }
        
        public bool HasValidCardTarget(Game gameData, Article caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                bool v1 = HasValidCardTarget(gameData, caster, player.cardsDeck);
                bool v2 = HasValidCardTarget(gameData, caster, player.cardsDiscard);
                bool v3 = HasValidCardTarget(gameData, caster, player.cardsHand);
                bool v4 = HasValidCardTarget(gameData, caster, player.cardsBoard);
                bool v5 = HasValidCardTarget(gameData, caster, player.cardsEquip);
                bool v6 = HasValidCardTarget(gameData, caster, player.cardsSecret);
                bool v7 = HasValidCardTarget(gameData, caster, player.cardsTemp);
                if (v1 || v2 || v3 || v4 || v5 || v6 || v7)
                    return true;
            }
            return false;
        }
        
        public bool HasValidCardTarget(Game gameData, Article caster, List<Card> list)
        {
            for (int c = 0; c < list.Count; c++)
            {
                Card card = list[c];
                if (AreTargetConditionsMet(gameData, caster, card))
                    return true;
            }
            return false;
        }
        
        public bool HasValidPlayerTarget(Game gameData, Article caster)
        {
            for (int p = 0; p < gameData.players.Length; p++)
            {
                Player player = gameData.players[p];
                if (CanTarget(gameData, caster, player))
                    return true;
            }
            return false;
        }
        
        public bool HasValidSlotTarget(Game gameData, Article caster)
        {
            foreach (Slot slot in Slot.GetAll())
            {
                if (CanTarget(gameData, caster, slot))
                    return true;
            }
            return false;
        }
        
        public bool IsSelector()
        {
            return target is AbilityTarget.SelectTarget or AbilityTarget.CardSelector or AbilityTarget.ChoiceSelector;
        }
        
        public static AbilityData Get(string id)
        {
            if (id == null)
                return null;
            bool success = abilityDict.TryGetValue(id, out AbilityData ability);
            return success ? ability : null;
        }

        public static AbilityData GetEvolveAbility()
        {
            return Get("common_evolve");
        }
        
        public static AbilityData GetSuperEvolveAbility()
        {
            return Get("super_evolve");
        }
        
        public static List<AbilityData> GetAll()
        {
            return abilityList;
        }
        
        #endregion
        
        
    }

   
    
    public enum AbilityTrigger
    {
        None = 0,

        Ongoing = 2,  //Always active (does not work with all effects)
        Activate = 5, //Action

        OnPlay = 10,  //When playeds
        OnPlayOther = 12,  //When another card played

        StartOfTurn = 20, //Every turn
        EndOfTurn = 22, //Every turn

        OnBeforeAttack = 30, //When attacking, before damaged
        OnAfterAttack = 31, //When attacking, after damaged if still alive
        OnBeforeDefend = 32, //When being attacked, before damaged
        OnAfterDefend = 33, //When being attacked, after damaged if still alive
        OnKill = 35,        //When killing another card during an attack

        OnDeath = 40, //When dying
        OnDeathOther = 42, //When another dying
        
        OnEvolved,
        OnPlayerEvolved,
        OnSuperEvolved,
        OnPlayerSuperEvolved,
        OnOtherEvolved,
        OnOtherSuperEvolved,
    }
    
    public enum AbilityTarget
    {
        None = 0,
        Self = 1,

        PlayerSelf = 4,
        PlayerOpponent = 5,
        AllPlayers = 7,

        AllCardsBoard = 10,
        AllCardsHand = 11,
        AllCardsAllPiles = 12,
        AllSlots = 15,
        AllCardData = 17,       //For card Create effects only

        PlayTarget = 20,        //在施展法术的同时选择的目标（仅限法术） 
        AbilityTriggerer = 25,   //触发陷阱的卡片
        EquippedCard = 27,       //If equipment, the bearer, if character, the item equipped

        SelectTarget = 30,        //Select a card, player or slot on board
        CardSelector = 40,          //Card selector menu
        ChoiceSelector = 50,        //Choice selector menu

        LastPlayed = 70,            //Last card that was played 
        LastTargeted = 72,          //Last card that was targeted with an ability
        LastDestroyed = 74,            //Last card that was killed
        LastSummoned = 77,            //Last card that was summoned or created
        
        

    }
}