#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace DataProcessor
{
    public class AssetDataAttributeProcessor : OdinAttributeProcessor<AssetData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));

            if (member.GetReturnType() == typeof(GameObject))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));
            }

            if (member.GetReturnType() == typeof(AudioClip))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));

            }
        }
    }
    
     public class AbilityDataAttributeProcessor : OdinAttributeProcessor<AbilityData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "id":
                    attributes.Add(new PropertyOrderAttribute(0));
                    attributes.Add(new BoxGroupAttribute("Ability ID"));
                    break;
                
                case "trigger":
                case "conditionsTrigger":
                    attributes.Add(new BoxGroupAttribute("Trigger"));
                    break;

                case "target":
                case "conditionsTarget":
                case "filtersTarget":
                    attributes.Add(new BoxGroupAttribute("Target"));
                    break;
                
                case "effects":
                case "status":
                case "value":
                case "duration":
                    attributes.Add(new BoxGroupAttribute("Effect"));
                    break;
                
                case "chainAbilities":
                case "manaCost":
                case  "exhaust":
                    attributes.Add(new BoxGroupAttribute("Ability"));
                    break;
                
                case "title":
                case "desc":
                    attributes.Add(new BoxGroupAttribute("Text"));
                    break;
                
                case "chargeTarget":
                    attributes.Add(new BoxGroupAttribute("FX"));
                    break;
            }
            
            if (member.GetReturnType() == typeof(GameObject))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));
            }

            if (member.GetReturnType() == typeof(AudioClip))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));

            }

            if (member.GetReturnType() == typeof(bool))
            {
                attributes.Add(new EnumToggleButtonsAttribute());
            }
            
            if (member.GetReturnType().IsEnum)
            {
                attributes.Add(new EnumPagingAttribute());
            }
        }
        
    }
     
    public class AvatarDataAttributeProcessor : OdinAttributeProcessor<AvatarData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "id":
                    attributes.Add(new BoxGroupAttribute("Avatar"));
                    attributes.Add(new HorizontalGroupAttribute("Avatar/Horizontal", 0.75f));
                    attributes.Add(new VerticalGroupAttribute("Avatar/Horizontal/RightColumn"));
                    attributes.Add(new PropertyOrderAttribute(1)); 
                    attributes.Add(new TitleAttribute("ID"));
                    break;
                case "avatar":
                    attributes.Add(new BoxGroupAttribute("Avatar"));
                    attributes.Add(new HorizontalGroupAttribute("Avatar/Horizontal", 0.25f));
                    attributes.Add(new PropertyOrderAttribute(0)); 
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                case "sortOrder":
                    attributes.Add(new BoxGroupAttribute("Avatar"));
                    attributes.Add(new HorizontalGroupAttribute("Avatar/Horizontal", 0.75f));
                    attributes.Add(new VerticalGroupAttribute("Avatar/Horizontal/RightColumn"));
                    attributes.Add(new PropertyOrderAttribute(1)); 
                    attributes.Add(new TitleAttribute("sortOrder"));
                    break;
            }
        }
    }
    
    public class CardBackDataAttributeProcessor : OdinAttributeProcessor<CardBackData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "id":
                    attributes.Add(new BoxGroupAttribute("ID"));
                    attributes.Add(new PropertyOrderAttribute(0));
                    break;
                case "cardBack":
                    attributes.Add(new BoxGroupAttribute("Sprite"));
                    attributes.Add(new HorizontalGroupAttribute("Sprite/Horizontal", 0.5f));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;

                case "deck":
                    attributes.Add(new BoxGroupAttribute("Sprite"));
                    attributes.Add(new HorizontalGroupAttribute("Sprite/Horizontal", 0.5f));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                case "sortOrder":
                    attributes.Add(new BoxGroupAttribute("Sprite"));
                    attributes.Add(new HorizontalGroupAttribute("Sprite/Horizontal_2", 0.5f));
                    break;
            }
        }
    }
    
    public class CardDataAttributeProcessor:OdinAttributeProcessor<CardData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            
            switch (member.Name)
            {
                case "id":
                    attributes.Add(new PropertyOrderAttribute(0));
                    attributes.Add(new BoxGroupAttribute("Card ID"));
                    break;
                
                case "title":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new VerticalGroupAttribute("Display/Rows"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Rows/TopRow"));
                    break;

                case "artFull":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new VerticalGroupAttribute("Display/Rows"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Rows/MiddleRow", 0.333f)); // 三列中的第一列
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                
                case "artBoard":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new VerticalGroupAttribute("Display/Rows"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Rows/MiddleRow", 0.333f)); // 三列中的第二列
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;

                case "artEvolveBoard":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new VerticalGroupAttribute("Display/Rows"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Rows/MiddleRow", 0.333f)); // 三列中的第三列
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;

                case "variant":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new VerticalGroupAttribute("Display/Rows"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Rows/BottomRow"));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                
                case "type":
                    attributes.Add(new BoxGroupAttribute("Stats"));
                    attributes.Add(new EnumToggleButtonsAttribute());
                    break;
                
                case "team":
                case "rarity":
                    attributes.Add(new BoxGroupAttribute("Stats"));
                    break;
                
                case "mana":
                case "hp":
                case "attack":
                    attributes.Add(new BoxGroupAttribute("Stats"));
                    break;
                
                case "traitDatas":
                case "traitStats":
                    attributes.Add(new BoxGroupAttribute("Traits"));
                    break;
                
                case "abilityDatas":
                    attributes.Add(new BoxGroupAttribute("Abilities"));
                    break;
                
                case "desc":
                case "text":
                    attributes.Add(new BoxGroupAttribute("Card Text"));
                    break;
            }

            if (member.GetReturnType() == typeof(GameObject))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                //attributes.Add(new PreviewFieldAttribute(60));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));
            }

            if (member.GetReturnType() == typeof(AudioClip))
            {
                attributes.Add(new BoxGroupAttribute("FX"));
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));

            }
        }
    }
    
    public class ConditionDataAttributeProcessor : OdinAttributeProcessor<ConditionData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            attributes.Add(new BoxGroupAttribute("Condition"));
        }
    }
    
    public class DeckDataAttributeProcessor : OdinAttributeProcessor<DeckData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "id":
                    attributes.Add(new BoxGroupAttribute("ID"));
                    attributes.Add(new PropertyOrderAttribute(0));
                    break;
                case "title":
                    attributes.Add(new BoxGroupAttribute("Title"));
                    break;

                case "hero":
                    attributes.Add(new BoxGroupAttribute("Deck"));
                    break;
                case "cards":
                    attributes.Add(new BoxGroupAttribute("Deck"));
                    break;
            }
            
            
        }
    }
    
    public class DeckPuzzleDataAttributeProcessor : DeckDataAttributeProcessor
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            attributes.Add(new BoxGroupAttribute("Start Card"));
        }
    }
    
    public class EffectDataAttributeProcessor : OdinAttributeProcessor<EffectData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            attributes.Add(new BoxGroupAttribute("Effect"));
            
            if (member.GetReturnType() == typeof(bool))
            {
                attributes.Add(new EnumToggleButtonsAttribute());
            }
        }
    }
    
    public class FilterDataAttributeProcessor : OdinAttributeProcessor<FilterData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            attributes.Add(new BoxGroupAttribute("Effect"));
            
            if (member.GetReturnType() == typeof(bool))
            {
                attributes.Add(new EnumToggleButtonsAttribute());
            }
        }
    }
    
    public class GamePlayDataAttributeProcessor : OdinAttributeProcessor<GamePlayData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(GamePlayData.hpStart):
                    attributes.Add(new InfoBoxAttribute("玩家初始生命值"));
                    attributes.Add(new MinValueAttribute(1));
                    break;
                    
                case nameof(GamePlayData.manaStart):
                    attributes.Add(new InfoBoxAttribute("初始法力值"));
                    attributes.Add(new RangeAttribute(0, 10));
                    break;
                    
                case nameof(GamePlayData.manaPerTurn):
                    attributes.Add(new InfoBoxAttribute("每回合获得的法力值"));
                    attributes.Add(new MinValueAttribute(0));
                    break;
                    
                case nameof(GamePlayData.manaMax):
                    attributes.Add(new InfoBoxAttribute("法力值上限"));
                    attributes.Add(new RangeAttribute(5, 20));
                    break;
                    
                case nameof(GamePlayData.cardsStart):
                    attributes.Add(new InfoBoxAttribute("初始手牌数量"));
                    attributes.Add(new RangeAttribute(1, 9));
                    break;
                    
                case nameof(GamePlayData.cardPerTurn):
                    attributes.Add(new InfoBoxAttribute("每回合抽牌数量"));
                    attributes.Add(new MinValueAttribute(0));
                    break;
                    
                case nameof(GamePlayData.cardMax):
                    attributes.Add(new InfoBoxAttribute("手牌上限"));
                    attributes.Add(new RangeAttribute(5, 15));
                    break;
                    
                case nameof(GamePlayData.turnDuration):
                    attributes.Add(new InfoBoxAttribute("每个回合的持续时间(秒)"));
                    attributes.Add(new MinValueAttribute(10.0f));
                    break;
                    
                case nameof(GamePlayData.deckSize):
                    attributes.Add(new InfoBoxAttribute("标准牌组包含的卡牌数量"));
                    attributes.Add(new RangeAttribute(20, 50));
                    break;
                    
                case nameof(GamePlayData.deckDuplicateNax):
                    attributes.Add(new InfoBoxAttribute("同名卡牌在牌组中的最大数量"));
                    attributes.Add(new RangeAttribute(1, 5));
                    break;
                    
                case nameof(GamePlayData.sellRatio):
                    attributes.Add(new InfoBoxAttribute("卡牌出售价格与购买价格的比例"));
                    attributes.Add(new RangeAttribute(0.1f, 1.0f));
                    break;
                    
                case nameof(GamePlayData.aiLevel):
                    attributes.Add(new InfoBoxAttribute("AI难度等级，1=最简单，10=最难"));
                    attributes.Add(new RangeAttribute(1, 10));
                    break;
                    
                case nameof(GamePlayData.secondBouns):
                    attributes.Add(new AssetsOnlyAttribute());
                    break;
            }
        }
    }
    
    public class LevelDataAttributeProcessor : OdinAttributeProcessor<LevelData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "id":
                case "level":
                    attributes.Add(new PropertyOrderAttribute(0));
                    attributes.Add(new BoxGroupAttribute("ID"));
                    break;
                case "title":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new TextAreaAttribute(2, 1));
                    break;
                case "scene":
                case "playerDeck":
                case "aiDeck":
                case "aiLevel":     
                case "firstPlayer":    
                case "tutoPrefab":
                case "mulligan":
                    attributes.Add(new BoxGroupAttribute("Gameplay"));
                    break;
                case "rewardXp":
                case "rewardCoins":
                case "rewardPacks":
                case "rewardCards":
                case "rewardDecks":
                    attributes.Add(new BoxGroupAttribute("Rewards"));
                    break;
            }

            if (member.GetReturnType().IsEnum)
            {
                attributes.Add(new EnumToggleButtonsAttribute());
            }

            if (member.GetReturnType() == typeof(GameObject))
            {
                attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));
            }
        }
    }
    
    public class PackDataAttributeProcessor : OdinAttributeProcessor<PackData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(120));
            
            // 根据成员名称将其分配到相应的BoxGroup
            switch (member.Name)
            {
                // Content Group
                case nameof(PackData.id):
                case nameof(PackData.type):
                case nameof(PackData.cards):
                case nameof(PackData.rarities1St):
                case nameof(PackData.rarities):
                case nameof(PackData.variants):
                    attributes.Add(new BoxGroupAttribute("Content"));
                    break;
                    
                // Display Group
                case nameof(PackData.title):
                case nameof(PackData.packImg):
                case nameof(PackData.cardBackImg):
                case nameof(PackData.desc):
                case nameof(PackData.sortOrder):
                    attributes.Add(new BoxGroupAttribute("Display"));
                    break;
                    
                // Availability Group
                case nameof(PackData.available):
                case nameof(PackData.cost):
                    attributes.Add(new BoxGroupAttribute("Availability"));
                    break;
            }
            
            // 为特定字段添加额外属性
            switch (member.Name)
            {
                case nameof(PackData.cards):
                    attributes.Add(new MinValueAttribute(1));
                    attributes.Add(new InfoBoxAttribute("每包卡牌数量"));
                    break;
                    
                case nameof(PackData.cost):
                    attributes.Add(new MinValueAttribute(0));
                    attributes.Add(new InfoBoxAttribute("购买所需费用"));
                    break;
                    
                case nameof(PackData.sortOrder):
                    attributes.Add(new InfoBoxAttribute("在商店中的排序顺序"));
                    break;
                    
                case nameof(PackData.desc):
                    attributes.Add(new HideLabelAttribute()); // TextArea already provides context
                    break;
            }
        }
    }
    
    public class RarityDataAttributeProcessor : OdinAttributeProcessor<RarityData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(100));
        
            attributes.Add(new BoxGroupAttribute("Rarity Settings"));
        
            switch (member.Name)
            {
                case nameof(RarityData.id):
                    attributes.Add(new InfoBoxAttribute("稀有度唯一标识符"));
                    break;
                
                case nameof(RarityData.title):
                    attributes.Add(new InfoBoxAttribute("稀有度显示名称"));
                    break;
                
                case nameof(RarityData.icon):
                    attributes.Add(new InfoBoxAttribute("稀有度图标"));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                
                case nameof(RarityData.rank):
                    attributes.Add(new InfoBoxAttribute("稀有度等级，数值越小越常见(1为最普通)"));
                    attributes.Add(new MinValueAttribute(1));
                    break;
            }
        }
    }
    
    public class RewardDataAttributeProcessor : OdinAttributeProcessor<RewardData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(100));
        
            switch (member.Name)
            {
                case nameof(RewardData.id):
                    attributes.Add(new BoxGroupAttribute("Basic Info"));
                    attributes.Add(new InfoBoxAttribute("奖励数据唯一标识符"));
                    break;
                
                case nameof(RewardData.group):
                    attributes.Add(new BoxGroupAttribute("Basic Info"));
                    attributes.Add(new InfoBoxAttribute("奖励分组标识"));
                    break;
                
                case nameof(RewardData.coins):
                    attributes.Add(new BoxGroupAttribute("Basic Info"));
                    attributes.Add(new InfoBoxAttribute("奖励的游戏币数量"));
                    attributes.Add(new MinValueAttribute(0));
                    break;
                
                case nameof(RewardData.xp):
                    attributes.Add(new BoxGroupAttribute("Basic Info"));
                    attributes.Add(new InfoBoxAttribute("奖励的经验值数量"));
                    attributes.Add(new MinValueAttribute(0));
                    break;
                
                case nameof(RewardData.packs):
                    attributes.Add(new BoxGroupAttribute("Reward Items"));
                    attributes.Add(new InfoBoxAttribute("奖励的卡包列表"));
                    break;
                
                case nameof(RewardData.cards):
                    attributes.Add(new BoxGroupAttribute("Reward Items"));
                    attributes.Add(new InfoBoxAttribute("奖励的卡牌列表"));
                    break;
                
                case nameof(RewardData.decks):
                    attributes.Add(new BoxGroupAttribute("Reward Items"));
                    attributes.Add(new InfoBoxAttribute("奖励的牌组列表"));
                    break;
            }
        }
    }
    
    public class StatusDataAttributeProcessor : OdinAttributeProcessor<StatusData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new LabelWidthAttribute(110));
            switch (member.Name)
            {
                case "effect":
                    attributes.Add(new PropertyOrderAttribute(0));
                    attributes.Add(new PropertySpaceAttribute());
                    attributes.Add(new BoxGroupAttribute("Status ID"));
                    attributes.Add(new EnumPagingAttribute());
                    break;
            
                case "icon":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Horizontal", 0.25f));
                    attributes.Add(new PropertyOrderAttribute(0)); 
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;

                case "title":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Horizontal", 0.75f));
                    attributes.Add(new VerticalGroupAttribute("Display/Horizontal/RightColumn"));
                    attributes.Add(new PropertyOrderAttribute(1)); 
                    attributes.Add(new TitleAttribute("Status Title"));
                    break;

                case "desc":
                    attributes.Add(new BoxGroupAttribute("Display"));
                    attributes.Add(new HorizontalGroupAttribute("Display/Horizontal", 0.75f)); // 第二列占75%宽度
                    attributes.Add(new VerticalGroupAttribute("Display/Horizontal/RightColumn"));
                    attributes.Add(new PropertyOrderAttribute(1)); 
                    attributes.Add(new TitleAttribute("Description"));
                    break;
                
                case "statusFX":
                    attributes.Add(new BoxGroupAttribute("FX"));
                    attributes.Add(new InlineEditorAttribute(InlineEditorModes.LargePreview));
                    break;
                
                case "hValue":
                    attributes.Add(new BoxGroupAttribute("AI"));
                    attributes.Add(new PropertyRangeAttribute(-10000, 10000));
                    break;
            }
        }
    }
    
    public class TeamDataAttributeProcessor : OdinAttributeProcessor<TeamData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(100));
        
            // 为所有字段添加到同一个BoxGroup中
            attributes.Add(new BoxGroupAttribute("Team Settings"));
        
            switch (member.Name)
            {
                case nameof(TeamData.id):
                    attributes.Add(new InfoBoxAttribute("队伍唯一标识符"));
                    break;
                
                case nameof(TeamData.title):
                    attributes.Add(new InfoBoxAttribute("队伍显示名称"));
                    break;
                
                case nameof(TeamData.icon):
                    attributes.Add(new InfoBoxAttribute("队伍图标"));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
                
                case nameof(TeamData.color):
                    attributes.Add(new InfoBoxAttribute("队伍主题颜色"));
                    break;
            }
        }
    }
    
    public class TraitDataAttributeProcessor : OdinAttributeProcessor<TraitData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(100));
        
            // 为所有字段添加到同一个BoxGroup中
            attributes.Add(new BoxGroupAttribute("Trait Settings"));
        
            switch (member.Name)
            {
                case nameof(TraitData.id):
                    attributes.Add(new InfoBoxAttribute("特征唯一标识符"));
                    break;
                
                case nameof(TraitData.title):
                    attributes.Add(new InfoBoxAttribute("特征显示名称"));
                    break;
                
                case nameof(TraitData.icon):
                    attributes.Add(new InfoBoxAttribute("特征图标"));
                    attributes.Add(new PreviewFieldAttribute(60));
                    break;
            }
        }
    }
    
    public class VariantDataAttributeProcessor : OdinAttributeProcessor<VariantData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            // 设置默认标签宽度
            attributes.Add(new LabelWidthAttribute(120));
        
            // 为所有字段添加到同一个BoxGroup中
            attributes.Add(new BoxGroupAttribute("Variant Settings"));
        
            switch (member.Name)
            {
                case nameof(VariantData.id):
                    attributes.Add(new InfoBoxAttribute("变体唯一标识符"));
                    break;
                
                case nameof(VariantData.title):
                    attributes.Add(new InfoBoxAttribute("变体显示名称"));
                    break;
                
                case nameof(VariantData.frame):
                    attributes.Add(new InfoBoxAttribute("卡牌边框图片"));
                    break;
                
                case nameof(VariantData.frameBoard):
                    attributes.Add(new InfoBoxAttribute("棋盘边框图片"));
                    break;
                
                case nameof(VariantData.color):
                    attributes.Add(new InfoBoxAttribute("变体主题颜色"));
                    break;
                
                case nameof(VariantData.costFactor):
                    attributes.Add(new InfoBoxAttribute("费用系数"));
                    attributes.Add(new MinValueAttribute(1));
                    break;
                
                case nameof(VariantData.isDefault):
                    attributes.Add(new InfoBoxAttribute("是否为默认变体"));
                    break;
            }

            if (member.GetReturnType() == typeof(Sprite))
            {
                attributes.Add(new PreviewFieldAttribute(60));
            }
        }
    }

    
    
    
}
# endif