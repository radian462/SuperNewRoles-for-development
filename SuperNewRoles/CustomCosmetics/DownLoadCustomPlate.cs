﻿using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    [HarmonyPatch]
    public class CustomPlates
    {

        public class CustomPlate
        {
            public string author { get; set; }
            public string name { get; set; }
            public string resource { get; set; }
            public string reshasha { get; set; }
        }
    }
    public static class DownLoadClass
    {
        public static List<CustomPlates.CustomPlate> platedetails = new List<CustomPlates.CustomPlate>();
        public static void Load()
        {
            SuperNewRolesPlugin.Logger.LogInfo("Load前");
            FetchHats("https://raw.githubusercontent.com/ykundesu/SuperNewNamePlates/main");
            SuperNewRolesPlugin.Logger.LogInfo("Load後");
            foreach (CustomPlates.CustomPlate plate in platedetails)
            {
                SuperNewRolesPlugin.Logger.LogInfo(plate.name);
            }
        }
        private static string sanitizeResourcePath(string res)
        {
            if (res == null || !res.EndsWith(".png"))
                return null;

            res = res.Replace("\\", "")
                     .Replace("/", "")
                     .Replace("*", "")
                     .Replace("..", "");
            return res;
        }
        private static bool doesResourceRequireDownload(string respath, string reshash, MD5 md5)
        {
            if (reshash == null || !File.Exists(respath))
                return true;

            using (var stream = File.OpenRead(respath))
            {
                var hash = System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                return !reshash.Equals(hash);
            }
        }
        public static async Task<HttpStatusCode> FetchHats(string repo)
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var response = await http.GetAsync(new System.Uri($"{repo}/CustomNamePlates.json"), HttpCompletionOption.ResponseContentRead);
            try
            {
                SuperNewRolesPlugin.Logger.LogInfo("OK前");
                if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
                if (response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return HttpStatusCode.ExpectationFailed;
                }
                string json = await response.Content.ReadAsStringAsync();
                JToken jobj = JObject.Parse(json)["hats"];
                if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

                List<CustomPlates.CustomPlate> platedatas = new List<CustomPlates.CustomPlate>();

                for (JToken current = jobj.First; current != null; current = current.Next)
                {
                    if (current.HasValues)
                    {
                        CustomPlates.CustomPlate info = new CustomPlates.CustomPlate();

                        info.name = current["name"]?.ToString();
                        SuperNewRolesPlugin.Logger.LogInfo("ADDNAME:"+ current["name"]?.ToString());
                        info.resource = sanitizeResourcePath(current["resource"]?.ToString());
                        if (info.resource == null || info.name == null) // required
                            continue;
                        info.author = current["author"]?.ToString();
                        info.reshasha = current["name"]?.ToString();
                        platedatas.Add(info);
                    }
                }

                List<string> markedfordownload = new List<string>();

                string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomPlatesChache\";
                MD5 md5 = MD5.Create();
                foreach (CustomPlates.CustomPlate data in platedatas)
                {
                    if (doesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                        markedfordownload.Add(data.resource);
                }

                foreach (var file in markedfordownload)
                {

                    var hatFileResponse = await http.GetAsync($"{repo}/NamePlates/{file}", HttpCompletionOption.ResponseContentRead);
                    if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                    using (var responseStream = await hatFileResponse.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = File.Create($"{filePath}\\{file}"))
                        {
                            responseStream.CopyTo(fileStream);
                        }
                    }
                }

                platedetails.AddRange(platedatas);
            }
            catch (System.Exception ex)
            {
                SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            SuperNewRolesPlugin.Logger.LogInfo("ダウンロード終了");
            return HttpStatusCode.OK;
        }
    }
}
