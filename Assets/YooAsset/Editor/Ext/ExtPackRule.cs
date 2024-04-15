using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YooAsset.Editor
{
    [DisplayName("资源包名: 文件路径.后缀")]
    public class PackSeparatelyExt : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = data.AssetPath.Replace(".", "_");
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }

        bool IPackRule.IsRawFilePackRule()
        {
            return false;
        }
    }


    [DisplayName("资源包名: 打包路径正则表达式")]
    public class PackDirRegex : IPackRule
    {
        string assetPath;
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = Rule.ParseReplacement(data.AssetPath, data.TagData, data.CollectPath + data.UserData);
            //Debug.Log(bundleName+" => "+data.TagData);
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
        bool IPackRule.IsRawFilePackRule()
        {
            return false;
        }
    }

}