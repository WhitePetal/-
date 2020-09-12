using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml; // 可能需要添加引用
using UnityEngine;

namespace FilterWord
{
    class FilterWord
    {
        // 将序列化和GZIP 压缩后的xml文件 进行加载 并解压 和 反序列化
        private string DeSerializeXML(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            // 使用 Unity Resources.Load 加载
            // 非 Unity 使用，请改为 IO 加载
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.log("未加载到配置文件，Path: " + path);
                return null;
            }
            byte[] bytes = textAsset.bytes;

            using (MemoryStream rms = new MemoryStream(bytes))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(rms, CompressionMode.Decompress, true))
                    {
                        byte[] buffer = new byte[1024];
                        int len = 0;
                        while ((len = gzs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, len);
                        }

                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        // DFA算法，实现敏感词过滤
        private Hashtable map;

        public void InitFilterWordCfg(string path)
        {
            List<string> filterWords = new List<string>();
            // LoadCfg
            // 这里由于加载的是 序列化 和 GZIP 压缩后的 xml文件，因此要反序列化回来
            // 如果没有这样的需求的话，可以直接用 IO 或其它方式 直接加载 xml 文件
            string xml = DeSerializeXML(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode root = doc.SelectSingleNode("root");
            foreach (XmlElement item in root.ChildNodes)
            {
                int id = int.Parse(item.GetAttribute("ID"));
                foreach (XmlElement ele in item.ChildNodes)
                {
                    if (ele.Name == "word") filterWords.Add(ele.InnerText);
                }
            }

            InitFilter(filterWords);
        }

        private void InitFilter(List<string> words)
        {
            map = new Hashtable(words.Count);
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                Hashtable indexMap = map;
                for (int j = 0; j < word.Length; j++)
                {
                    char c = word[j];
                    if (indexMap.ContainsKey(c))
                    {
                        indexMap = (Hashtable)indexMap[c];
                    }
                    else
                    {
                        Hashtable newMap = new Hashtable();
                        newMap.Add("IsEnd", 0);
                        indexMap.Add(c, newMap);
                        indexMap = newMap;
                    }

                    if (j == word.Length - 1)
                    {
                        if (indexMap.ContainsKey("IsEnd")) indexMap["IsEnd"] = 1;
                        else indexMap.Add("IsEnd", 1);
                    }
                }
            }
        }

        private int CheckFilterWord(string txt, int beginIndex)
        {
            bool flag = false;
            int len = 0;
            Hashtable curMap = map;
            for (int i = beginIndex; i < txt.Length; i++)
            {
                char c = txt[i];
                Hashtable temp = (Hashtable)curMap[c];
                if (temp != null)
                {
                    if ((int)temp["IsEnd"] == 1) flag = true;
                    else curMap = temp;

                    len++;
                }
                else break;
            }

            if (!flag) len = 0;

            return len;
        }

        public string SerachFilterWordAndReplace(string txt)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder(txt);
            while (i < txt.Length)
            {
                int len = CheckFilterWord(txt, i);
                if (len > 0)
                {
                    for (int j = 0; j < len; j++)
                    {
                        sb[i + j] = '*';
                    }
                    i += len;
                }
                else ++i;
            }
            return sb.ToString();
        }
    }
}
