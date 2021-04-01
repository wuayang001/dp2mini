//using DigitalPlatform.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;

namespace DigitalPlatform.Text
{
    public class StringUtil
    {
        #region 和 Application 有关的功能

        public static bool IsDevelopMode()
        {
            List<string> args = GetCommandLineArgs();
            return args.IndexOf("develop") != -1;
        }

        public static bool IsNewInstance()
        {
            List<string> args = GetCommandLineArgs();
            return args.IndexOf("newinstance") != -1;
        }

        // 取得命令行参数
        // 丢掉第一个元素
        public static List<string> GetCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            List<string> list = new List<string>(args);
            if (list.Count == 0)
                return new List<string>();
            list.RemoveAt(0);
            return list;
        }

        public static List<string> GetClickOnceCommandLineArgs(string query)
        {
            List<string> args = new List<string>();
            if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
            {
                args = StringUtil.SplitList(query.Substring(1), '&');
                for (int i = 0; i < args.Count; i++)
                {
                    args[i] = HttpUtility.UrlDecode(args[i]);
                }
            }

            return args;
        }
        #endregion



        public static List<string> SplitList(string strText)
        {
            // 2011/12/26
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { ',' });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        public static List<string> SplitList(string strText,
            char delimeter)
        {
            // 2011/12/26
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { delimeter });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        // 2015/7/16
        public static List<string> SplitList(string strText,
            string strSep)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            char delimeter = (char)1;
            strText = strText.Replace(strSep, new string(delimeter, 1));

            string[] parts = strText.Split(new char[] { delimeter });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText)
        {
            return ParseParameters(strText, ',', '=');
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText,
            char chSegChar,
            char chEqualChar,
            string strDecodeStyle = "")
        {
            Hashtable results = new Hashtable();

            if (string.IsNullOrEmpty(strText) == true)
                return results;

            string[] parts = strText.Split(new char[] { chSegChar });   // ','
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                string strName = "";
                string strValue = "";
                int nRet = strPart.IndexOf(chEqualChar);    // '='
                if (nRet == -1)
                {
                    strName = strPart;
                    strValue = "";
                }
                else
                {
                    strName = strPart.Substring(0, nRet).Trim();
                    strValue = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strName) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (strDecodeStyle == "url")
                    strValue = HttpUtility.UrlDecode(strValue);

                results[strName] = strValue;
            }

            return results;
        }


        /// <summary>
        /// 忽略大小写
        /// 查找一个小字符串是否包含在大字符串，
        /// 内部调isInAList函数
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="strList">大字符串</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInList(string strSub,
            string strList)
        {
            /*
            string[] aTemp;
            aTemp = strList.Split(new char[]{','});

            int nRet = strList.IndexOfAny(new char[]{' ','\t'});
            if (nRet != -1) 
            {
                for(int i=0;i<aTemp.Length;i++) {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }
 
            return IsInAlist(strSub,aTemp);
            */
            return IsInList(strSub,
                strList,
                true);
        }

        // parameters:
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInList(string strSub,
            string strList,
            bool bIgnoreCase)
        {
            if (String.IsNullOrEmpty(strList) == true)
                return false;	// 优化

            string[] aTemp;
            aTemp = strList.Split(new char[] { ',' });

            int nRet = strList.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < aTemp.Length; i++)
                {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }

            return IsInAlist(strSub,
                aTemp,
                bIgnoreCase);
        }


        // TODO: 似乎可以用 IndexOf() 代替
        public static bool IsInList(int v, int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (v == a[i])
                    return true;
            }

            return false;
        }


        // 在列举值中增加或清除一个值
        // parameters:
        //      strSub  里面可以包含多个值
        public static void SetInList(ref string strList,
            string strSub,
            bool bOn)
        {
            if (bOn == false)
            {
                RemoveFromInList(strSub,
                    true,
                    ref strList);
            }
            else
            {
                // 单个值的情况
                if (strSub.IndexOf(',') == -1)
                {
                    if (IsInList(strSub, strList) == true)
                        return;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strSub;
                    return;
                }

                // 2012/2/2
                // 多个值的情况
                string[] sub_parts = strSub.Split(new char[] { ',' });
                foreach (string sub in sub_parts)
                {
                    if (sub == null)
                        continue;

                    string strOne = sub.Trim();
                    if (string.IsNullOrEmpty(strOne) == true)
                        continue;

                    if (IsInList(strOne, strList) == true)
                        continue;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strOne;
                }
            }
        }


        // 从逗号间隔的list中去除一个特定的style值。大小写不敏感
        // parameters:
        //      strSub  要去除的值列表。字符串中可以包含多个值。
        //      bRemoveMultiple	是否具有去除多个相同strSub值的能力。==false，只去除发现的第一个
        public static bool RemoveFromInList(string strSub,
            bool bRemoveMultiple,
            ref string strList)
        {
            string[] sub_parts = strSub.Split(new char[] { ',' });

            string[] list_parts = strList.Split(new char[] { ',' });

            bool bChanged = false;
            foreach (string temp in sub_parts)
            {
                string sub = temp.Trim();
                if (string.IsNullOrEmpty(sub) == true)
                    continue;

                for (int j = 0; j < list_parts.Length; j++)
                {
                    string list = list_parts[j];
                    if (list == null)
                        continue;

                    list = list.Trim();
                    if (string.IsNullOrEmpty(list) == true)
                        continue;

                    if (String.Compare(sub, list, true) == 0)
                    {
                        bChanged = true;
                        list_parts[j] = null;
                        if (bRemoveMultiple == false)
                            break;
                    }
                }
            }

            StringBuilder result = new StringBuilder(4096);
            foreach (string list in list_parts)
            {
                if (string.IsNullOrEmpty(list) == false)
                {
                    if (result.Length > 0)
                        result.Append(",");
                    result.Append(list);
                }
            }

            strList = result.ToString();

            return bChanged;
        }

        /// <summary>
        /// 查找一个小字符串是否包含在一个字符串数组中
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="aList">字符串数组</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInAlist(string strSub,
            string[] aList)
        {
            /*
            for(int i=0;i<aList.Length;i++)
            {
                if (String.Compare(strSub,aList[i],true) == 0) 
                {
                    return true;
                }
            }
            return false;
            */
            return IsInAlist(strSub,
                aList,
                true);
        }

        // parameters:
        //      strSub      要比较的单个值。可以包含多个单独的值，用逗号连接。注：如果是多个值，则只要有一个匹配上，就返回true
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInAlist(string strSub,
            string[] aList,
            bool bIgnoreCase)
        {
            // 2015/5/27
            if (string.IsNullOrEmpty(strSub) == true)
                return false;

            string[] sub_parts = strSub.Split(new char[] { ',' });

            // 2012/2/2 增加了处理strSub中包含多个值的能力
            foreach (string sub in sub_parts)
            {
                if (sub == null)
                    continue;

                string strOne = sub.Trim();
                if (string.IsNullOrEmpty(strOne) == true)
                    continue;

                for (int i = 0; i < aList.Length; i++)
                {
                    if (String.Compare(strOne, aList[i], bIgnoreCase) == 0)
                        return true;
                }
            }
            return false;
        }



        // 修改字符串某一个位字符
        public static string SetAt(string strText, int index, char c)
        {
            strText = strText.Remove(index, 1);
            strText = strText.Insert(index, new string(c, 1));

            return strText;
        }

        // 比较两个版本号
        public static int CompareVersion(string strVersion1, string strVersion2)
        {
            if (string.IsNullOrEmpty(strVersion1) == true)
                strVersion1 = "0.0";
            if (string.IsNullOrEmpty(strVersion2) == true)
                strVersion2 = "0.0";

            try
            {
                Version version1 = new Version(strVersion1);
                Version version2 = new Version(strVersion2);

                return version1.CompareTo(version2);
            }
            catch (Exception ex)
            {
                throw new Exception("比较版本号字符串 '" + strVersion1 + "' 和 '" + strVersion2 + "' 过程出现异常: " + ex.Message, ex);
            }
        }

        public static List<string> ParseTwoPart(string strText, string strSep)
        {
            string strLeft = "";
            string strRight = "";
            ParseTwoPart(strText, strSep, out strLeft, out strRight);
            List<string> results = new List<string>();
            results.Add(strLeft);
            results.Add(strRight);
            return results;
        }

        public static List<string> ParseTwoPart(string strText, string[] seps)
        {
            string strLeft = "";
            string strRight = "";

            if (string.IsNullOrEmpty(strText) == true)
                goto END1;

            int nRet = 0;
            string strSep = "";
            foreach (string sep in seps)
            {
                nRet = strText.IndexOf(sep);
                if (nRet != -1)
                {
                    strSep = sep;
                    goto FOUND;
                }
            }

            strLeft = strText;
            goto END1;

        FOUND:
            Debug.Assert(nRet != -1, "");
            strLeft = strText.Substring(0, nRet).Trim();
            strRight = strText.Substring(nRet + strSep.Length).Trim();

        END1:
            List<string> results = new List<string>();
            results.Add(strLeft);
            results.Add(strRight);
            return results;
        }

        /// <summary>
        /// 通用的，切割两个部分的函数
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <param name="strSep">分隔符号</param>
        /// <param name="strPart1">返回第一部分</param>
        /// <param name="strPart2">返回第二部分</param>
        public static void ParseTwoPart(string strText,
            string strSep,
            out string strPart1,
            out string strPart2)
        {
            strPart1 = "";
            strPart2 = "";

            if (string.IsNullOrEmpty(strText) == true)
                return;

            int nRet = strText.IndexOf(strSep);
            if (nRet == -1)
            {
                strPart1 = strText;
                return;
            }

            strPart1 = strText.Substring(0, nRet).Trim();
            strPart2 = strText.Substring(nRet + strSep.Length).Trim();
        }


    }

}
