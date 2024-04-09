

using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;

namespace YooAsset.Editor
{

    public class ExtImportRule
    {

        public static void ImportCsvConfig(string filePath)
        {
            var csvRead = new CsvReader(filePath, true);


            List<AssetBundleCollectorPackage> packages = AssetBundleCollectorSettingData.Setting.Packages;// new List<AssetBundleCollectorPackage>();

            AssetBundleCollectorPackage package = packages[0];

            HashSet<string> exclude = new HashSet<string>();

            while (package.Groups.Count > 1)
            {
                package.Groups.RemoveAt(package.Groups.Count - 1);
            }

            try
            {
                for (int i = 0; i < csvRead.Count; i++)
                {
                    csvRead.Read();



                    var filename = csvRead.GetFieldString("file");
                    var key = csvRead.GetFieldString("field");
                    var keys = key.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var read = new CsvReader(filename, true);

                    AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
                    group.ActiveRuleName = "EnableGroup";
                    group.GroupName = Path.GetFileNameWithoutExtension(filename);
                    group.GroupDesc = "";
                    group.AssetTags = "";


                    package.Groups.Add(group);



                    for (int j = 0; j < read.Count; j++)
                    {
                        read.Read();

                        foreach (var fkey in keys)
                        {
                            var resPath = read.GetFieldString(fkey);

                            if (resPath.StartsWith("assets/"))
                            {
                                resPath = "A" + resPath.Substring(1);
                            }

                            if (string.IsNullOrEmpty(resPath) || !File.Exists(resPath) || exclude.Contains(resPath))
                            {
                                continue;
                            }

                            exclude.Add(resPath);

                            AssetBundleCollector collector = new AssetBundleCollector();
                            collector.CollectorGUID = AssetDatabase.GUIDFromAssetPath(resPath).ToString();
                            collector.CollectPath = AssetDatabase.GUIDToAssetPath(collector.CollectorGUID);
                            collector.CollectorType = ECollectorType.MainAssetCollector;
                            collector.AddressRuleName = "AddressByFileName";

                            if (resPath.EndsWith(".data"))
                            {
                                collector.PackRuleName = "PackRawFile";
                            }
                            else
                            {
                                collector.PackRuleName = "PackSeparatelyExt";
                            }

                            collector.FilterRuleName = "CollectCustom";

                            if (!resPath.Contains("|"))
                            {
                                collector.UserData = Path.GetExtension(resPath);
                            }

                            collector.AssetTags = "";
                            group.Collectors.Add(collector);
                        }
                    }
                    read.Close();
                }
            }
            finally
            {

            }

            // 检测配置错误
            foreach (var packaget in packages)
            {
                packaget.CheckConfigError();
            }

            AssetBundleCollectorSettingData.SaveFile();
        }

    }

}