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

}