using System.IO;

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
}