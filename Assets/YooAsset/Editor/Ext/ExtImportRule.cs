

using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityNavMeshAgent;
using UnityEngine;

namespace YooAsset.Editor
{

    public class ExtImportRule
    {

        static string[] LanFix = new string[] {"cn","en","tw" };

        static HashSet<string> RawExtHash = new HashSet<string>() { ".data", ".bnk", ".wem" };

        public static void ImportCsvConfig(string filePath)
        {
            var csvRead = new CsvReader(filePath, true,true);


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

                    if (filename.StartsWith("/"))
                    {
                        filename = filename.Substring(1);
                    }

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
                            var isDir = !resPath.Contains(".");

                            if (string.IsNullOrEmpty(resPath) || exclude.Contains(resPath))
                            {
                                continue;
                            }
                            if (resPath.Contains("{0}"))
                            {
                                foreach (var item in LanFix)
                                {
                                    string newPath = string.Format(resPath, item);
                                    AddRulePath(newPath, ref group, ref exclude);
                                }
                            }
                            else if (resPath.Contains("bnk")||resPath.Contains("wem"))
                            {
                                string[] arr = resPath.Split('|');

                                //foreach (var item in arr) {
                                //    string fullPath = $"data/assets/gameres/Audio/GeneratedSoundBanks/android/{item}";
                                //    AddRulePath(fullPath, ref group, ref exclude);
                                //}
                            }
                            else 
                            {
                                AddRulePath(resPath, ref group, ref exclude,isDir);
                            }
                            
                            
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

        public static void AddRulePath(string resPath,ref AssetBundleCollectorGroup group,ref HashSet<string> exclude,bool isDir=false) {
            
            if(!isDir && !File.Exists(resPath)) {
                Debug.Log($"文件不存在{resPath}");
                return; 
            }
                
            
            exclude.Add(resPath);

            AssetBundleCollector collector = new AssetBundleCollector();
            collector.CollectorGUID = AssetDatabase.GUIDFromAssetPath(resPath).ToString();
            collector.CollectPath = AssetDatabase.GUIDToAssetPath(collector.CollectorGUID);
            collector.CollectorType = ECollectorType.MainAssetCollector;
            collector.AddressRuleName = "AddressByFileName";

            string ext = Path.GetExtension(resPath);

            if (isDir)
            {
                //目录收集
                collector.PackRuleName = "PackCollector";
                collector.FilterRuleName = "CollectAll";
            }
            else {
                //文件收集
                if (RawExtHash.Contains(ext))
                {
                    collector.PackRuleName = "PackRawFile";
                }
                else
                {
                    collector.PackRuleName = "PackSeparatelyExt";
                }

                collector.FilterRuleName = "CollectCustomExt";
                collector.UserData = Path.GetExtension(collector.CollectPath);
                
            }
          

            collector.AssetTags = "";
            group.Collectors.Add(collector);

        }

    }




}