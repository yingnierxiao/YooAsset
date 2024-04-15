
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YooAsset.Editor
{
    [DisplayName("收集自定义后缀")]
    public class CollectCustomExt : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == data.UserData;
        }
    }


    [DisplayName("收集正则表达式")]
    public class CollectDirRegex : IFilterRule
    {
        public CollectDirRegex() {
        
        }
        public bool IsCollectAsset(FilterRuleData data)
        {
            //Debug.Log(data.CollectPath + data.UserData);
            return Regex.IsMatch(data.AssetPath,data.CollectPath+data.UserData);
            //return Path.GetExtension(data.AssetPath) == data.UserData;
        }
    }
}