using System;
using System.Collections.Generic;
using Data;
using Unit;
using UnityEngine;

namespace GameLogic
{
    [Serializable]
    public class Article
    {
        public string articleId;
        public string uid;
        public int playerID;
        public string variantId;

        public int value;
        
        public List<string> abilities = new List<string>();
        public List<string> ongoingAbilities = new List<string>();
        
        [NonSerialized] private int hash = 0;
        [NonSerialized] private ArticleData data= null;
        [NonSerialized] private VariantData vdata= null;
        [NonSerialized] private List<AbilityData> abilitiesData = null;

        public Article(string articleId, string uid, int playerID)
        {
            this.articleId = articleId;
            this.uid = uid;
            this.playerID = playerID;
        }

        public virtual void Refresh()
        {
            
        }
        
        public virtual void ClearOngoing()
        {
            ClearOngoingAbility();
        }
        
        public virtual void Clear()
        {
            ClearOngoing();
            Refresh();
            SetArticle(ArticleData);
            
            
        }
        
        public void SetArticle(ArticleData iarticle)
        {
            data = iarticle;
            articleId = iarticle.id;
            SetAbilities(iarticle);
        }
        
        public void SetAbilities(ArticleData iarticle)
        {
            abilities.Clear();
            ongoingAbilities.Clear();
            abilitiesData?.Clear();
            foreach (AbilityData ability in iarticle.abilityDatas)
                AddAbility(ability);
        }
        
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
        
        public virtual bool CanDoActivatedAbilities()
        {
            return true;
        }
        
        public bool CanDoAbilities()
        {
            return true;
        }
        

        public ArticleData ArticleData
        {
            get
            {
                if (data == null || data.id != articleId)
                {
                    data = ArticleData.Get(articleId);
                }
                return data;
            }
        }
        
        public ArticleData Data => ArticleData;
        
        public int Hash
        {
            get {
                if (hash == 0)
                    hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return hash;
            }
        }
        
        public static Article Create(ArticleData iarticle, Player iplayer)
        {
            return Create(iarticle, iplayer,GameTool.GenerateRandomID(11, 15));
        }
        
        private static Article Create(ArticleData icard, Player iplayer, string uid)
        {
            Article article = new Article(icard.id, uid, iplayer.id);
            article.SetArticle(icard);
            iplayer.articlesAll[article.uid] = article;
            return article;
        }
        
        public static Article CloneNew(Article source)
        {
            Article article = new Article(source.articleId, source.uid, source.playerID);
            Clone(source, article);
            return article;
        }
        
        public static void Clone(Article source, Article dest)
        {
            dest.articleId = source.articleId;
            dest.uid = source.uid;
            dest.playerID = source.playerID;
            
            dest.variantId = source.variantId;
            
            
            GameTool.CloneList(source.abilities, dest.abilities); 
            GameTool.CloneList(source.ongoingAbilities, dest.ongoingAbilities); 
            GameTool.CloneListRefNull<AbilityData>(source.abilitiesData, ref dest.abilitiesData); //No need to deep copy since AbilityData doesn't change dynamically, its just a reference

        }
        
        public static void CloneNone(Article source, ref Article dests)
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
        public static void CloneDict(Dictionary<string, Article> source, Dictionary<string, Article> dests)
        {
            foreach (KeyValuePair<string, Article> pair in source)
            {
                bool valid = dests.TryGetValue(pair.Key, out Article dest);
                if (valid)
                    Clone(pair.Value, dest);
                else
                    dests[pair.Key] = CloneNew(pair.Value);
            }
        }
        
        public static void CloneListRef(Dictionary<string, Article> refDict, List<Article> source, List<Article> dests)
        {
            for (int i = 0; i < source.Count; i++)
            {
                Article scard = source[i];
                bool valid = refDict.TryGetValue(scard.uid, out Article dest);
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
    public class ArticleStatus
    {
        public StatusType type;
        public int value;
        public int duration;
        public bool permanent;
        
        [NonSerialized]
        private StatusData data;

        public ArticleStatus(StatusType type, int value, int duration)
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
        
        public static ArticleStatus CloneNew(ArticleStatus copy)
        {
            ArticleStatus status = new ArticleStatus(copy.type, copy.value, copy.duration);
            status.permanent = copy.permanent;
            return status;
        }

        public static void Clone(ArticleStatus sources, ArticleStatus dest)
        {
            dest.type = sources.type;
            dest.value = sources.value;
            dest.duration = sources.duration;
            dest.permanent = sources.permanent;
        }
        
        public static void CloneList(List<ArticleStatus> sources, List<ArticleStatus> dest)
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
    public class ArticleTrait
    {
        public string id;
        public int value;
        
        [NonSerialized]
        private TraitData data = null;

        public ArticleTrait(string id, int value)
        {
            this.id = id;
            this.value = value;
        }
        
        public ArticleTrait(TraitData trait, int value)
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
        
        public static ArticleTrait CloneNew(ArticleTrait copy)
        {
            ArticleTrait status = new ArticleTrait(copy.id, copy.value);
            return status;
        }

        public static void Clone(ArticleTrait sources, ArticleTrait dest)
        {
            dest.id = sources.id;
            dest.value = sources.value;
        }
        
        public static void CloneList(List<ArticleTrait> source, List<ArticleTrait> dest)
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