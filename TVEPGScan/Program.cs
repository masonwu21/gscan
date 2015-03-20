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

                    GetEPGToday(xn.Attributes["id"].Value);
                }

            } 
        }

        private static void GetEPGToday(string tvcode)
        {
            HttpWebRequest httpWebRequest = null;

            httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://wap.tvmao.com/program.jsp?c=" + tvcode);
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

            string xpathstring = "//div[@class='c']";
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
