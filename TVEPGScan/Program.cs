using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Xml;

namespace TVEPGScan
{
    class Program
    {
        static void Main(string[] args)
        {
            string chfile = "ch.xml";

            if(!Directory.Exists("out"))
            {
                Directory.CreateDirectory("out");
            }

            XmlDocument doc = new XmlDocument();

            if (File.Exists(chfile))
            {
                doc.Load(chfile);
                XmlNode root = doc.DocumentElement;
                foreach (XmlNode xn in root.ChildNodes)
                {
                    if (xn.Attributes["source"].Value=="m")
                    {
                        GetTVmaoToday(xn.Attributes["id"].Value);
                    }
                    else if (xn.Attributes["source"].Value=="s")
                    {
                        GetTVsouToday(xn.Attributes["id"].Value, xn.Attributes["url"].Value, xn.InnerText);
                    }

                }

            } 
        }

        private static void GetTVsouToday(string tvid, string url, string channelname)
        {
            //http://m.tvsou.com/EPG.asp?TVid=280&Channelid=2751
            HttpWebRequest httpWebRequest = null;

            httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://m.tvsou.com/EPG.asp?" + url);
            httpWebRequest.UserAgent = @"Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920) like iPhone OS 7_0_3 Mac OS X AppleWebKit/537 (KHTML, like Gecko) Mobile Safari/537";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;";
            httpWebRequest.CookieContainer = new CookieContainer();
            HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();
            HtmlDocument document = new HtmlDocument();

            Stream responseStream = httpWebResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(responseStream, Encoding.GetEncoding("gbk"));
            String value = sReader.ReadToEnd();
            document.LoadHtml(value);
            HtmlNode rootNode = document.DocumentNode;

            string xpathstringweek = "//div[@class='week']/ul[1]/li[1]";
            HtmlNodeCollection a0 = rootNode.SelectNodes(xpathstringweek);
            string channeldate="";
            foreach(HtmlNode h in a0)
            {
                channeldate = h.InnerText;
            }

            string xpathstring = "//div[@class='time']/ul[1]";
            HtmlNodeCollection aa = rootNode.SelectNodes(xpathstring); 

            if (aa != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode docNodeDeclare = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(docNodeDeclare);
                XmlElement xmlRoot = xmlDoc.CreateElement("TV");
                xmlDoc.AppendChild(xmlRoot);
                XmlAttribute chname = xmlDoc.CreateAttribute("Name");
                chname.Value = channelname;
                xmlRoot.Attributes.Append(chname);
                XmlAttribute chdate = xmlDoc.CreateAttribute("Date");
                chdate.Value = channeldate;
                xmlRoot.Attributes.Append(chdate);
                
                foreach (HtmlNode h in aa)
                {
                    foreach (HtmlNode h1 in h.ChildNodes)
                    {
                        string b = h1.InnerText;
                        if (b.Length>6)
                        {
                            b = b.Replace(@" 剧情", "");
                            XmlElement program = xmlDoc.CreateElement("P");
                            program.InnerText = b;
                            xmlRoot.AppendChild(program);
                        }
                    }
                }




                xmlDoc.Save("./out/" + tvid + ".xml");
            }


        }

        private static void GetTVmaoToday(string tvcode)
        {
            WebClient wc = new WebClient();
            wc.DownloadFile("http://m.tvmao.com/program/BTV-BTV1-w3.html", "./out/BTV1.html");
            return;
            
            DateTime dt = DateTime.Now;
            string w="";
            switch(dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    w = "-w1";
                    break;
                case DayOfWeek.Tuesday:
                    w = "-w2";
                    break;
                case DayOfWeek.Wednesday:
                    w = "-w3";
                    break;
                case DayOfWeek.Thursday:
                    w = "-w4";
                    break;
                case DayOfWeek.Friday:
                    w = "-w5";
                    break;
                case DayOfWeek.Saturday:
                    w = "-w6";
                    break;
                case DayOfWeek.Sunday:
                    w = "-w7";
                    break;
            }
                 
                      

            HttpWebRequest httpWebRequest = null;

            //httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://m.tvmao.com/program/" + tvcode);
            httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://m.tvmao.com/program/BTV-BTV1-w7.html");
            httpWebRequest.UserAgent = @"Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920) like iPhone OS 7_0_3 Mac OS X AppleWebKit/537 (KHTML, like Gecko) Mobile Safari/537";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;";
            httpWebRequest.CookieContainer = new CookieContainer();
            HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();
            HtmlDocument document = new HtmlDocument();

            Stream responseStream = httpWebResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            String value = sReader.ReadToEnd();
            document.LoadHtml(value);
            HtmlNode rootNode = document.DocumentNode;

            string xpathstring = "//table[@class='timetable']";
            HtmlNodeCollection aa = rootNode.SelectNodes(xpathstring); 

            if (aa != null)
            {
                string innertext = aa[0].InnerHtml;
                innertext = innertext.Replace("<br>", "*");
                innertext = innertext.Replace("\r", "");
                innertext = innertext.Replace("\n", "");
                string[] strarray = innertext.Split('*');
      
                string channelname = strarray[0];
                string channeldate = strarray[1].Split(' ')[0];
                

                XmlDocument xmlDoc = new XmlDocument();
                XmlNode docNodeDeclare = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(docNodeDeclare);
                XmlElement xmlRoot = xmlDoc.CreateElement("TV");
                xmlDoc.AppendChild(xmlRoot);
                XmlAttribute chname = xmlDoc.CreateAttribute("Name");
                chname.Value = channelname;
                xmlRoot.Attributes.Append(chname);
                XmlAttribute chdate = xmlDoc.CreateAttribute("Date");
                chdate.Value = channeldate;
                xmlRoot.Attributes.Append(chdate);
              

                for (int i = 3; i < strarray.Count() - 1; i++)
                {
                    XmlElement program = xmlDoc.CreateElement("P");
                    program.InnerText = strarray[i];
                    xmlRoot.AppendChild(program);

                }
                
                xmlDoc.Save("./out/"+tvcode + ".xml");
            }
        }
    }
}
