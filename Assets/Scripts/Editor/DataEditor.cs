using System.Linq;
using Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace Editor
{
    public class DataEditor : OdinMenuEditorWindow
    {
        
        [MenuItem("Tools/DataEditor")]
        private static void OpenWindow()
        {
            GetWindow<DataEditor>().Show();
        }
        
        protected override void OnEnable()
        {
           
        }
        
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            
            tree.Config.AutoFocusSearchBar = false; // 自动展开折叠设置为false
            tree.Config.DrawScrollView = true;     // 保持滚动视图
            tree.Config.DefaultMenuStyle.Height = 22; // 可选的菜单项高度设置
            
            GamePlayDataMenuBuilder.BuildMenuTree(tree);
            AssetDataMenuBuilder.BuildMenuTree(tree);
            
            AbilityDataMenuBuilder.BuildMenuTree(tree);
            CardDataMenuBuilder.BuildMenuTree(tree);
            RarityDataMenuBuilder.BuildMenuTree(tree);
            StatusDataMenuBuilder.BuildMenuTree(tree);
            TeamDataMenuBuilder.BuildMenuTree(tree);
            TraitDataMenuBuilder.BuildMenuTree(tree);
            EffectDataMenuBuilder.BuildMenuTree(tree);
            ConditionDataMenuBuilder.BuildMenuTree(tree);
            FilterDataMenuBuilder.BuildMenuTree(tree);
            VariantDataMenuBuilder.BuildMenuTree(tree);
            DeckDataMenuBuilder.BuildMenuTree(tree);

            
            AvatarDataMenuBuilder.BuildMenuTree(tree);
            CardBackDataMenuBuilder.BuildMenuTree(tree);
            PackDataMenuBuilder.BuildMenuTree(tree);
            LevelDataMenuBuilder.BuildMenuTree(tree);
            RewardDataMenuBuilder.BuildMenuTree(tree);
            
            
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            OdinMenuTreeSelection selected = this.MenuTree.Selection;
            SirenixEditorGUI.BeginHorizontalToolbar();
            GUILayout.FlexibleSpace();

            if (SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                if (selected.SelectedValue is ScriptableObject so)
                {
                    string path = AssetDatabase.GetAssetPath(so);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                }
                    
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            AbilityDataMenuBuilder.OnDestroy();
            CardDataMenuBuilder.OnDestroy();
            EffectDataMenuBuilder.OnDestroy();
            ConditionDataMenuBuilder.OnDestroy();
            FilterDataMenuBuilder.OnDestroy();
            TeamDataMenuBuilder.OnDestroy();
            TraitDataMenuBuilder.OnDestroy();
            DeckDataMenuBuilder.OnDestroy();
            RarityDataMenuBuilder.OnDestroy();
            StatusDataMenuBuilder.OnDestroy();
            VariantDataMenuBuilder.OnDestroy();

            
            AvatarDataMenuBuilder.OnDestroy();
            CardBackDataMenuBuilder.OnDestroy();
            LevelDataMenuBuilder.OnDestroy();
            PackDataMenuBuilder.OnDestroy();
            RewardDataMenuBuilder.OnDestroy();
        }
    }
    
   
}