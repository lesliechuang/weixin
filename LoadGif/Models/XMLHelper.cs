using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace LoadGif.Models
{
    public static class XMLHelper
    {
        //解释一下(string)requestHT["Event"]，通过下面的方法可以将微信传递过来的XML转换成Hashtable，获取对应的参数
        // 将xml文件转换成Hashtable
        //使用方法：Hashtable requestHT = WeixinServer.ParseXml(xml);
        public static Hashtable ParseXml(this string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode bodyNode = xmlDocument.ChildNodes[0];
            Hashtable ht = new Hashtable();
            if (bodyNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode xn in bodyNode.ChildNodes)
                {
                    ht.Add(xn.Name, xn.InnerText);
                }
            }
            return ht;
        }
    }
}