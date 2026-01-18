using System.Linq;
using Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Editor
{
    public static class AssetDataMenuBuilder
    {
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            var menuItem =tree.AddAssetAtPath("AssetData", "Assets/Resources/AssetData.asset", typeof(AssetData)).FirstOrDefault();
            
            if (tree.MenuItems.Count > 1)
            {
                var assetDataItem = tree.MenuItems[tree.MenuItems.Count - 1];
                tree.MenuItems.RemoveAt(tree.MenuItems.Count - 1);
                tree.MenuItems.Insert(1, assetDataItem);
            }
        
            // 设置高亮样式
            menuItem.Icon = EditorIcons.UnityGameObjectIcon; // 可以设置一个醒目的图标
            menuItem.Style.Height = 28; // 增加高度使其更突出
            
        }
    }
}