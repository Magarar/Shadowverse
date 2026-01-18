using System.Linq;
using Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace Editor
{
    public static class GamePlayDataMenuBuilder
    {
        public static void BuildMenuTree(OdinMenuTree tree)
        {
            var menuItem =tree.AddAssetAtPath("GamePlayData", "Assets/Resources/GamePlayData.asset", typeof(GamePlayData)).FirstOrDefault();
            
            if (tree.MenuItems.Count > 1)
            {
                var assetDataItem = tree.MenuItems[tree.MenuItems.Count - 1];
                tree.MenuItems.RemoveAt(tree.MenuItems.Count - 1);
                tree.MenuItems.Insert(0, assetDataItem);
            }
        
            // 设置高亮样式
            menuItem.Icon = EditorIcons.UnityGameObjectIcon; // 可以设置一个醒目的图标
            menuItem.Style.Height = 28; // 增加高度使其更突出
        
            // 默认选中这个菜单项
            tree.Selection.Clear();
            tree.Selection.Add(menuItem);
        }
    }
}