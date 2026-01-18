using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "New Article", menuName = "Data/Article")]
    public class ArticleData:ScriptableObject
    {
        public string id;

        [Header("Display")] 
        public string title;
        public Sprite art;
        
        
        [Header("Abilities")]
        public AbilityData[] abilityDatas;
        
        [Header("Article Text")]
        [TextArea(3,5)]
        public string text;
        
        [Header("Description")]
        [TextArea(5, 10)]
        public string desc;
        
        [Header("FX")]
        public GameObject spawnFX;
        public GameObject destroyFX;
        public GameObject idleFX;
        public AudioClip spawnAudio;
        public AudioClip destroyAudio;
        
        public static List<ArticleData> articleList = new List<ArticleData>();
        public static Dictionary<string, ArticleData> articleDict = new Dictionary<string, ArticleData>();

        public static void Load(string folder = "")
        {
            if (articleList.Count == 0)
            {
                articleList.AddRange(Resources.LoadAll<ArticleData>(folder));
                foreach (ArticleData article in articleList)
                    articleDict.Add(article.id, article);
            }
        }

        public static ArticleData Get(string id)
        {
            if(id==null)
                return null;
            bool success = articleDict.TryGetValue(id, out ArticleData articleData);
            
            return success ? articleData : null;
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

        public static List<ArticleData> GetAll()
        {
            return articleList;
        }
        
    }
}