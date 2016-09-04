using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace collectResourcesOfHACG
{
    class Program
    {
        public const int MAX_RETRY_TIMES = 1;//获取HTML时的最大重试次数
        public const bool MANUAL_INPUT = false;//是否手动输入信息
        public const bool INDEX_CUSTOMED = false;//是否自定义网页编号
        public static int[] INDEXS = { 1, 2, 6, 13, 15, 17, 19, 21, 23, 38, 42, 44, 46, 49, 52, 54, 57, 60, 63, 65, 68, 75, 78, 81, 86, 87, 90, 91, 94, 95, 97, 102, 103, 110, 111, 113, 116, 119, 121, 123, 124, 125, 132, 159, 161, 163, 166, 168, 170, 172, 174, 178, 180, 183, 185, 187, 189, 191, 193, 195, 197, 200, 202, 205, 207, 210, 214, 216, 220, 224, 226, 228, 230, 234, 236, 238, 240, 242, 244, 246, 248, 250, 252, 254, 258, 260, 262, 264, 266, 268, 270, 272, 274, 276, 278, 280, 284, 286, 288, 290, 292, 294, 296, 298, 300, 303, 305, 307, 310, 312, 314, 316, 318, 320, 322, 324, 326, 328, 330, 332, 334, 336, 338, 340, 342, 344, 346, 348, 349, 350, 352, 354, 356, 358, 360, 362, 364, 366, 368, 372, 374, 376, 378, 380, 382, 384, 386, 389, 391, 393, 395, 397, 399, 401, 403, 405, 411, 413, 415, 417, 419, 421, 423, 425, 428, 430, 432, 434, 436, 438, 440, 442, 444, 446, 449, 451, 453, 455, 457, 459, 462, 464, 466, 468, 469, 473, 476, 478, 481, 483, 484, 485, 487, 489, 491, 493, 495, 497, 499, 501, 503, 505, 507, 510, 512, 514, 516, 518, 520, 522, 524, 526, 528, 530, 532, 535, 537, 539, 543, 545, 547, 549, 551, 553, 555, 557, 559, 561, 563, 565, 567, 569, 571, 573, 576, 578, 581, 583, 585, 587, 590, 593, 594, 597, 598, 599, 600, 602, 604, 607, 609, 612, 613, 615, 616, 619, 621, 626, 628, 630, 632, 633, 635, 637, 640, 643, 645, 647, 650, 653, 655, 657, 660, 662, 664, 666, 668, 670, 672, 675, 679, 681, 686, 690, 692, 694, 698, 700, 702, 703, 713, 716, 723, 726, 728, 730, 735, 738, 739, 742, 744, 747, 752, 755, 756, 764, 767, 770, 775, 781, 783, 785, 787, 789, 792, 794, 796, 798, 800, 802, 804, 806, 808, 810, 812, 815, 818, 819, 831, 833, 838, 840, 843, 847, 850, 852, 854, 856, 861, 863, 866, 868, 873, 875, 878, 884, 887, 890, 892, 893, 896, 899, 905, 907, 908, 912, 916, 919, 922, 925, 928, 931, 933, 937, 942, 943, 947, 950, 953, 967, 1166, 1235, 1263, 1292, 1315, 1321, 1371, 1700, 1780, 1832, 1881, 1935, 2001, 2094, 2106, 2136, 2151, 2257, 2276, 2299, 2305, 2306, 2443, 2874, 2917, 2981, 3020, 3055, 3098, 3110, 33222, 3238, 3252, 3280, 3334 };
        public const int INDEXS_START = 0;
        public static int INDEXS_LENGTH = INDEXS.Length;//自定义的网页编号数量
        public static int START_INDEX = 0;//开始网页编号
        public static int END_INDEX = 100000;//结束网页编号
        public static bool URL_CUSTOMED = false;//自定义URL
        public static string URL = "http://www.hacg.li/wp/3114.html";
        public static string fileLocation = "E:\\HACG\\";

        //手动输入
        static void input()
        {
            char isCustomed;
            do
            {
                Console.WriteLine("是否自定义一个测试网页？输入y或n（否则输入页面编号范围批量收集）");
                isCustomed = Convert.ToChar(Console.ReadLine());
                switch (isCustomed)
                {
                    case 'y':
                        {
                            URL_CUSTOMED = true;
                            Console.WriteLine("输入URL");
                            URL = Console.ReadLine();
                            break;
                        }
                    case 'n':
                        {
                            URL_CUSTOMED = false;
                            Console.WriteLine("输入开始网页编号（需大于1）");
                            START_INDEX = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("输入结束网页编号（推荐不要大于25000）");
                            END_INDEX = Convert.ToInt32(Console.ReadLine());
                            break;
                        }
                    default: Console.WriteLine("输入错误，请重新输入"); break;
                }
            } while (isCustomed != 'y' && isCustomed != 'n');
            Console.WriteLine("输入文件保存位置，如E:\\\\HACG\\");
            fileLocation = Console.ReadLine();
        }

        //获取HTML
        static string GetWebClient(string url)
        {
            string strHTML = "";
            WebClient myWebClient = new WebClient();
            Stream myStream = myWebClient.OpenRead(url);
            StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("utf-8"));
            strHTML = sr.ReadToEnd();
            myStream.Close();
            return strHTML;
        }

        //收集标题、作者、日期信息
        static void collectInformation(ref string HTML, ref resources res)
        {
            //title
            int indexOfTitle = HTML.IndexOf("<title>");
            if (indexOfTitle != -1 && HTML.IndexOf(" | 琉璃神社 ★ HACG</title>") != -1)
                res.title = HTML.Substring(indexOfTitle + 7, HTML.IndexOf(" | 琉璃神社 ★ HACG</title>") - indexOfTitle - 7);
            //datetime
            int indexOfDatetime = HTML.IndexOf("\"entry-date\" datetime=\"");
            if (indexOfDatetime != -1)
            {
                string datetime = HTML.Substring(indexOfDatetime + 23, HTML.IndexOf("\" pubdate>") - indexOfDatetime - 23);
                res.datetime = new DateTime(
                    Convert.ToInt32(datetime.Substring(0, 4)),
                    Convert.ToInt32(datetime.Substring(5, 2)),
                    Convert.ToInt32(datetime.Substring(8, 2)),
                    Convert.ToInt32(datetime.Substring(11, 2)),
                    Convert.ToInt32(datetime.Substring(14, 2)),
                    Convert.ToInt32(datetime.Substring(17, 2)));
            }
            //author
            int indexOfAuthor = HTML.IndexOf("发布的文章\" rel=\"author\">");
            if (indexOfAuthor != -1)
                res.author = HTML.Substring(indexOfAuthor + 20, HTML.IndexOf("</a></span></span>") - indexOfAuthor - 20);
        }

        //判断是否是资源链接
        static bool isResourcesLink(string link, string type)
        {
            switch (type)
            {
                case "magnet":
                    {
                        Regex character = new Regex("[a-zA-Z]");
                        Regex number = new Regex("[0-9]");
                        if (character.IsMatch(link) && number.IsMatch(link))
                            return true;
                        else
                            return false;
                    }
                case "baidupan":
                    {
                        Regex upperCaseCharacter = new Regex("[A-Z]");
                        Regex lowerCaseCharacter = new Regex("[a-z]");
                        Regex number = new Regex("[0-9]");
                        if (upperCaseCharacter.IsMatch(link) && lowerCaseCharacter.IsMatch(link) && number.IsMatch(link))
                            return true;
                        else
                            return false;
                    }
                default: return false;
            }
        }

        //收集磁力链接
        static void collectMagnets(ref string HTML, ref resources res)
        {
            Regex magnet40RE = new Regex("[^a-zA-Z0-9/\"\'-.;?\\[_=]([a-z0-9]{40}|[A-Z0-9]{40})[^a-zA-Z0-9/\"\'-.:;?\\[\\]_=]");
            Regex magnet32RE = new Regex("[^a-zA-Z0-9/\"\'-.;?\\[_=]([a-z0-9]{32}|[A-Z0-9]{32})[^a-zA-Z0-9/\"\'-.:;?\\[\\]_=]");
            Regex partOfMagnetRE = new Regex("[^a-zA-Z0-9/\"\'-.;?\\[_=]([A-Z0-9]{10,30}|[a-z0-9]{10,30})[^a-zA-Z0-9/\"\'-.:;?\\[\\]_=]");
            MatchCollection matches1, matches2, matches3;
            matches1 = magnet40RE.Matches(HTML);
            matches2 = magnet32RE.Matches(HTML);
            matches3 = partOfMagnetRE.Matches(HTML);
            if (matches1.Count != 0 || matches3.Count > 1)
            {
                for (int i = 0; i < matches1.Count; i++)
                    if (isResourcesLink(matches1[i].ToString(), "magnet"))
                    {
                        res.numberOfMagnets++;
                        res.magnets[res.numberOfMagnets - 1] = "magnet:?xt=urn:btih:" + matches1[i].ToString().Substring(1, 40);
                    }
                for (int i = 0; i < matches2.Count; i++)
                    if (isResourcesLink(matches2[i].ToString(), "magnet"))
                    {
                        res.numberOfMagnets++;
                        res.magnets[res.numberOfMagnets - 1] = "magnet:?xt=urn:btih:" + matches2[i].ToString().Substring(1, 32);
                    }
                for (int i = 0; i + 1 < matches3.Count;)
                {
                    int length1 = matches3[i].ToString().Length, length2 = matches3[i + 1].ToString().Length;
                    if (length1 + length2 == 40 || length1 + length2 == 32)
                    {
                        string combinedMagnet = matches3[i].ToString().Substring(1, length1 - 2) + matches3[i + 1].ToString().Substring(1, length2 - 2);
                        if (isResourcesLink(combinedMagnet, "magnet"))
                        {
                            res.numberOfMagnets++;
                            res.magnets[res.numberOfMagnets - 1] = "magnet:?xt=urn:btih:" + combinedMagnet;
                        }
                        i = i + 2;
                    }
                    else
                        i++;
                }
            }
        }

        //收集百度盘链接
        static void collectBaidupan(ref string HTML, ref resources res)
        {
            Regex baidupanRE = new Regex("[^a-zA-Z0-9\"\'-.;:?=\\[\\]_][a-zA-Z0-9]{8}[^a-zA-Z0-9/\"\'-.;:?=\\[\\]_]");
            Regex passwordRE = new Regex("[^a-z0-9][a-z0-9]{4}[^a-z0-9]");
            MatchCollection matches;
            matches = baidupanRE.Matches(HTML);
            if (matches.Count != 0)
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if (!isResourcesLink(matches[i].ToString(), "baidupan"))
                        continue;
                    if (res.numberOfBaidupanLinks >= 1 && res.baidupanLinks[res.numberOfBaidupanLinks - 1].link.IndexOf(matches[i].ToString().Substring(1, 8)) >= 0)
                        continue;
                    res.numberOfBaidupanLinks++;
                    res.baidupanLinks[res.numberOfBaidupanLinks - 1].link = "http://pan.baidu.com/s/" + matches[i].ToString().Substring(1, 8);
                    string password = HTML.Substring(matches[i].Index + 9, 6);
                    if (passwordRE.IsMatch(password))
                    {
                        res.baidupanLinks[res.numberOfBaidupanLinks - 1].havePassword = true;
                        res.baidupanLinks[res.numberOfBaidupanLinks - 1].password = password.Substring(1, 4);
                    }
                    else
                    {
                        password = HTML.Substring(matches[i].Index + 9, 20);
                        int indexOfPassword = password.IndexOf("密码");
                        if (indexOfPassword >= 0)
                        {
                            for (int j = 1; j < 3; j++)
                                if (passwordRE.IsMatch(password.Substring(indexOfPassword + j, 6)))
                                {
                                    res.baidupanLinks[res.numberOfBaidupanLinks - 1].havePassword = true;
                                    res.baidupanLinks[res.numberOfBaidupanLinks - 1].password = password.Substring(indexOfPassword + j + 1, 4);
                                }
                        }
                    }
                }
            }
        }

        //显示收集到的信息和资源
        static void display(ref resources res)
        {
            Console.WriteLine("title:" + res.title);
            Console.WriteLine("datetime:" + res.datetime);
            Console.WriteLine("author:" + res.author);
            for (int i = 0; i < res.numberOfMagnets; i++)
                Console.WriteLine(res.magnets[i]);
            for (int i = 0; i < res.numberOfBaidupanLinks; i++)
            {
                Console.Write(res.baidupanLinks[i].link);
                if (res.baidupanLinks[i].havePassword)
                    Console.WriteLine(" " + res.baidupanLinks[i].password);
                else
                    Console.WriteLine();
            }
            Console.WriteLine();
        }

        //输出目录
        static void exportCatalog(ref string HTML, int index, string title)
        {
            Regex labelRE = new Regex("<[brpe/ ]{1,4}>");
            Match match;
            int lastIndex = 0;
            string catalog = "";
            while (true)
            {
                int indexOfStart = HTML.IndexOf("<pre>", lastIndex + 5);
                if (indexOfStart < 0)
                    break;
                lastIndex = indexOfStart;
                catalog += HTML.Substring(indexOfStart + 5, (HTML.IndexOf("</pre>", indexOfStart) >= 0 ? HTML.IndexOf("</pre>", indexOfStart) : HTML.IndexOf("</div>", indexOfStart)) - indexOfStart - 5);
                catalog += "\n";
            }
            while (true)
            {
                match = labelRE.Match(catalog);
                if (match.ToString() == "")
                    break;
                catalog = catalog.Replace(match.ToString(), "");
            }
            title = title.Replace('/', ' ');
            title = title.Replace('\\', ' ');
            title = title.Replace(':', ' ');
            title = title.Replace('*', ' ');
            title = title.Replace('?', ' ');
            title = title.Replace('"', ' ');
            title = title.Replace('<', ' ');
            title = title.Replace('>', ' ');
            title = title.Replace('|', ' ');
            FileStream file = new FileStream(fileLocation + index + "_" + title + ".txt", FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(catalog);
            file.Write(data, 0, data.Length);
            file.Flush();
            file.Close();
        }

        //保存收集到的信息和资源
        static void saveCollection(resources res)
        {
            string text = "";
            text += "index:" + res.index + "\nurl:" + res.url + "\ntitle:" + res.title + "\nanthor:" + res.author + "\ndatetime:" + res.datetime + "\n";
            if (res.numberOfMagnets != 0)
                for (int i = 0; i < res.numberOfMagnets; i++)
                    text += (res.magnets[i] + "\n");
            if (res.numberOfBaidupanLinks != 0)
                for (int i = 0; i < res.numberOfBaidupanLinks; i++)
                {
                    text += res.baidupanLinks[i].link;
                    if (res.baidupanLinks[i].havePassword)
                        text += (" " + res.baidupanLinks[i].password + "\n");
                    else
                        text += "\n";
                }
            text += "\n";
            FileStream file = new FileStream(fileLocation + "resourcesOfHACG.txt", FileMode.Append);
            byte[] data = System.Text.Encoding.Default.GetBytes(text);
            file.Write(data, 0, data.Length);
            file.Flush();
            file.Close();

            text = "";
            if (res.numberOfMagnets != 0)
                for (int i = 0; i < res.numberOfMagnets; i++)
                    text += (res.magnets[i] + "\n");
            FileStream file2 = new FileStream(fileLocation + "magnets.txt", FileMode.Append);
            data = System.Text.Encoding.Default.GetBytes(text);
            file2.Write(data, 0, data.Length);
            file2.Flush();
            file2.Close();
        }

        static void Main(string[] args)
        {
            string HTML = "", url = "";
            if (MANUAL_INPUT)
                input();
            Stopwatch HTTPRequestTimer = new Stopwatch();
            Stopwatch HTTPAnalysisTimer = new Stopwatch();
            int retryTimes = 0;
            int iStatrt = (INDEX_CUSTOMED ? INDEXS_START : START_INDEX);
            int iEnd = (INDEX_CUSTOMED ? INDEXS_LENGTH - 1 : END_INDEX);
            for (int i = iStatrt; i <=iEnd ; i++)
            {
                int index = (INDEX_CUSTOMED ? INDEXS[i] : i);
                if (URL_CUSTOMED)
                {
                    url = URL;
                    i = END_INDEX;
                }
                else
                    url = "http://www.hacg.li/wp/" + index + ".html";
                resources res = new resources();
                res.index = index;
                res.url = url;
                if (retryTimes == 0)
                    Console.WriteLine("正在加载 " + url);
                try
                {
                    HTTPRequestTimer.Start();
                    HTML = GetWebClient(url);
                    HTTPRequestTimer.Stop();
                    HTTPAnalysisTimer.Start();
                    if (retryTimes > 0)
                        Console.WriteLine();
                    retryTimes = 0;
                    Console.WriteLine("加载成功");

                    collectInformation(ref HTML, ref res);
                    collectMagnets(ref HTML, ref res);
                    collectBaidupan(ref HTML, ref res);
                    display(ref res);
                    if (HTML.IndexOf("<pre>") >= 0)
                        exportCatalog(ref HTML, res.index, res.title);
                    saveCollection(res);
                    HTTPAnalysisTimer.Stop();
                }
                catch (WebException error)
                {
                    res.errorMessage = error.Message;
                    Console.WriteLine("加载失败，" + error.Message);
                    if (retryTimes < MAX_RETRY_TIMES)
                    {
                        retryTimes++;
                        i--;
                        Console.Write("第" + retryTimes + "次重试...");
                    }
                    else
                    {
                        retryTimes = 0;
                        Console.WriteLine("已达最大重试次数，加载仍然失败\n");
                    }
                    //if (e.Status == WebExceptionStatus.ProtocolError)
                    //{
                    //    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)error.Response).StatusCode);
                    //    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)error.Response).StatusDescription);
                    //}
                }
            }
            Console.WriteLine("获取HTTP所花时间：" + HTTPRequestTimer.Elapsed.ToString() + " 即" + HTTPRequestTimer.ElapsedMilliseconds + "毫秒");
            Console.WriteLine("分析HTTP所花时间：" + HTTPAnalysisTimer.Elapsed.ToString() + " 即" + HTTPAnalysisTimer.ElapsedMilliseconds + "毫秒");
            //Console.WriteLine("分析所占百分比：" + HTTPAnalysisTimer.ElapsedMilliseconds/(HTTPRequestTimer.ElapsedMilliseconds+HTTPAnalysisTimer.ElapsedMilliseconds));
            Console.ReadKey();
        }
    }
}