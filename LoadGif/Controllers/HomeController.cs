using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using LoadGif.Models;
using System.Net;
using System.Text;
using Weather;
using Weather.Model;
using System.Resources;
using System.Globalization;
using Face.Model;

namespace LoadGif.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string Token = "maolegemi";
        private static readonly string appId = "wxb3d178324e572048";
        private static readonly string secret = "5e3f018fb1561c82146f3d2e934ee8e3";
        private string flag = "";

        public ActionResult Index()
        {
            if (Request.HttpMethod.ToLower() == "get")
            {
                string echoStr = Request.QueryString["echoStr"];
                if (CheckSignature())
                {
                    if (!string.IsNullOrEmpty(echoStr))
                    {
                        return Content(echoStr);
                    }
                }
            }
            else if (Request.HttpMethod.ToLower() == "post")
            {
                using (StreamReader reader = new StreamReader(Request.InputStream))
                {
                    string xml = reader.ReadToEnd();
                    string s = DualRequest(xml);
                    if (s != "")
                    {
                        Response.Write(s);
                        Response.End();
                    }

                }

                //System.IO.Stream s = System.Web.HttpContext.Current.Request.InputStream;

                //byte[] b = new byte[s.Length];

                //s.Read(b, 0, (int)s.Length);

                //string postStr = System.Text.Encoding.UTF8.GetString(b);

                //if (!string.IsNullOrEmpty(postStr))
                //{

                //    ResponseMsg(postStr);

                //    Response.Write(ResponseMsg(postStr));

                //    Response.End();

                //}

            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "你的应用程序说明页。";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "你的联系方式页。";

            return View();
        }

        #region 私有方法

        /// <summary>

        /// 返回信息结果(微信信息返回)

        /// </summary>

        /// <param name="weixinXML"></param>
        private string ResponseMsg(string weixinXML)
        {
            string resxml = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(weixinXML);//读取XML字符串
                XmlElement rootElement = doc.DocumentElement;

                XmlNode MsgType = rootElement.SelectSingleNode("MsgType");//获取字符串中的消息类型

                if (MsgType.InnerText == "text")//如果消息类型为文本消息
                {
                    var model = new
                    {
                        ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText,
                        FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText,
                        CreateTime = rootElement.SelectSingleNode("CreateTime").InnerText,
                        MsgType = MsgType.InnerText,
                        Content = rootElement.SelectSingleNode("Content").InnerText,
                        MsgId = rootElement.SelectSingleNode("MsgId").InnerText
                    };
                    if (model.Content.Contains("图"))
                    {
                        resxml += "<xml><ToUserName><![CDATA[" + model.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + model.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><ArticleCount>1</ArticleCount><Articles><item><Title><![CDATA[欢迎您的光临！]]></Title><Description><![CDATA[非常感谢您的关注！]]></Description><PicUrl><![CDATA[http://h.hiphotos.baidu.com/image/w%3D1366%3Bcrop%3D0%2C0%2C1366%2C768/sign=86dfb525a044ad342ebf8384e6943797/42166d224f4a20a451df2b0f91529822730ed030.jpg]]></PicUrl><Url><![CDATA[http://www.baidu.com/]]></Url></item></Articles><FuncFlag>0</FuncFlag></xml>";

                    }
                    else if (model.Content.Contains("?") || model.Content.Contains("？"))
                    {
                        resxml += "<xml><ToUserName><![CDATA[" + model.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + model.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[欢迎光临---网中网" + DateTime.Now.ToString() + "]]></Content><FuncFlag>0</FuncFlag></xml>";

                    }
                    else
                    {
                        resxml += "<xml><ToUserName><![CDATA[" + model.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + model.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[亲，感谢您对我的关注，有事请留言，我会及时回复你的哦。]]></Content><FuncFlag>0</FuncFlag></xml>";

                    }
                }//如果是其余的消息类型
                else
                {
                    var model = new
                    {
                        ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText,
                        FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText,
                        CreateTime = rootElement.SelectSingleNode("CreateTime").InnerText,
                    };
                    resxml += "<xml><ToUserName><![CDATA[" + model.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + model.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[亲，感谢您对我的关注，有事请留言，我会及时回复你的哦。" + MsgType.InnerText + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                    // Response.Write(resxml);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //Response.End(); 
            return resxml;
            ///这里写你的返回信息代码

        }


        /// <summary>
        /// 处理请求消息
        /// </summary>
        private string DualRequest(string xml)
        {
            string responseXML = "";
            string resultStr = "";
            string basePath = "\\resources";

            //创建文件夹
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + basePath))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + basePath);
            }

            using (StreamWriter wirter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + basePath + "\\1.txt"))
            {
                try
                {
                    wirter.WriteLine(xml);

                    Hashtable ht = xml.ParseXml();

                    string fromUser = ht["FromUserName"].ToString();
                    string toUser = ht["ToUserName"].ToString();
                    int nowTime = ConvertDateTimeInt(DateTime.Now);
                    string type = ht["MsgType"].ToString();

                    if (ht["MsgType"].ToString() == "text")
                    {
                        if (ht["Content"].ToString() == "图")
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[news]]></MsgType>
                    <ArticleCount>1</ArticleCount>
                    <Articles>
                    <item>
                    <Title><![CDATA[{3}]]></Title> 
                    <Description><![CDATA[{4}]]></Description>
                    <PicUrl><![CDATA[{5}]]></PicUrl>
                    <Url><![CDATA[{6}]]></Url>
                    </item>
                    </Articles>
                    </xml> ", fromUser, toUser, nowTime, "图文1", "图文1的描述", "http://www.cloudscool.com/CloudsCoolWeb/WeiXinToken/Images/d1.jpg", "www.baidu.com");
                        }
                        else if (ht["Content"].ToString() == "音频")
                        {
                            responseXML = string.Format(@"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                        <MsgType><![CDATA[music]]></MsgType>
                        <Music>
                        <Title><![CDATA[{3}]]></Title>
                        <Description><![CDATA[{4}]]></Description>
                        <MusicUrl><![CDATA[{5}]]></MusicUrl>
                        <HQMusicUrl><![CDATA[{6}]]></HQMusicUrl>
                        </Music>
                        </xml>", fromUser, toUser, nowTime, "音频", "回复的音频", "http://www.cloudscool.com/CloudsCoolWeb/WeiXinToken/Music/1.mp3", "http://www.cloudscool.com/CloudsCoolWeb/WeiXinToken/Music/1.mp3");
                        }
                        else if (ht["Content"].ToString() == "视频")
                        {
                            responseXML = string.Format(@"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                        <MsgType><![CDATA[video]]></MsgType>
                        <Video>
                        <MediaId><![CDATA[{3}]]></MediaId>
                        <Title><![CDATA[{4}]]></Title>
                        <Description><![CDATA[{5}]]></Description>
                        </Video> 
                        </xml>", fromUser, toUser, nowTime, "", "视频", "回复的视频");
                        }
                        else if (ht["Content"].ToString() == "发送")
                        {

                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret="+secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{
                                                    ""touser"":"""+fromUser+ @""",
                                                    ""msgtype"":""text"",
                                                    ""text"":
                                                    {
                                                         ""content"":""发送消息""
                                                    }
                                                }";

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            //responseJason = HttpUtility.UrlEncode(responseJason, encoding);
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=" + token.access_token);
                            request.Method = "POST";
                            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request.ContentLength = data.Length;
                            Stream stream = request.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            resultStr += "https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=" + token.access_token + "\n";
                            resultStr += "***************************\n";
                            resultStr += responseJason;

                            //                        responseXML = string.Format(@"<xml>
                            //                    <ToUserName><![CDATA[{0}]]></ToUserName>
                            //                    <FromUserName><![CDATA[{1}]]></FromUserName>
                            //                    <CreateTime>{2}</CreateTime>
                            //                    <MsgType><![CDATA[text]]></MsgType>
                            //                    <Content><![CDATA[{3}]]></Content>
                            //                    <FuncFlag>0</FuncFlag>
                            //                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "回复消息：" + responseStr); 
                        }
                        else if (ht["Content"].ToString() == "我的信息")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/user/info?access_token=" + token.access_token + "&openid=" + fromUser + "&lang=zh_CN");
                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }
                            resultStr += "***************************\n";
                            resultStr += responseStr;

                            responseXML = string.Format(@"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{3}]]></Content>
                            <FuncFlag>0</FuncFlag>
                            </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "回复消息：" + responseStr);
                        }
                        else if (ht["Content"].ToString().StartsWith("创建分组") && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            var array = ht["Content"].ToString().Split(':');
                            string groupName = array[1];
                            if (groupName != "")
                            {
                                HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                                string responseStr = "";
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    responseStr = reader.ReadToEnd();
                                }

                                string responseJason = @"{""group"":{""name"":"""+groupName+@"""}}";

                                Encoding encoding = Encoding.GetEncoding("gb2312");
                                byte[] data = Encoding.UTF8.GetBytes(responseJason);

                                AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();
                                HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/groups/create?access_token="+token.access_token);
                                request1.Method = "POST";
                                request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                                request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                                request1.ContentLength = data.Length;
                                Stream stream = request1.GetRequestStream();
                                stream.Write(data, 0, data.Length);
                                stream.Close();

                                HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                                using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                                {
                                    responseStr = reader.ReadToEnd();
                                }

                                responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "成功创建分组：" + responseStr);
                            }
                        }
                        else if (ht["Content"].ToString() == "所有分组")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/groups/get?access_token=" + token.access_token);
                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "分组：" + responseStr);
                        }
                        else if (ht["Content"].ToString().StartsWith("移动到:") && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            var array = ht["Content"].ToString().Split(':');
                            string groupName = array[1];

                            if (groupName != "")
                            {
                                HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                                string responseStr = "";
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    responseStr = reader.ReadToEnd();
                                }

                                AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                                string responseJason = @"{""openid"":"""+fromUser+@""",""to_groupid"":" + groupName + "}";

                                Encoding encoding = Encoding.GetEncoding("gb2312");
                                byte[] data = Encoding.UTF8.GetBytes(responseJason);

                                HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/groups/members/update?access_token=" + token.access_token);
                                request1.Method = "POST";
                                request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                                request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                                request1.ContentLength = data.Length;
                                Stream stream = request1.GetRequestStream();
                                stream.Write(data, 0, data.Length);
                                stream.Close();

                                HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                                using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                                {
                                    responseStr = reader.ReadToEnd();
                                }

                                responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), responseStr);
                            }
                        }
                        else if (ht["Content"].ToString() == "我在的分组")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            string responseJason = @"{""openid"":"""+fromUser+@"""}";
                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/groups/getid?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "分组："+responseStr);
                        }
                        else if (ht["Content"].ToString().StartsWith("分组改名") && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            string s = ht["Content"].ToString();
                            s = s.Substring(s.IndexOf('(') + 1, s.Length - s.IndexOf('(') - 2);
                            var array = s.Split(':');

                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }
                            
                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{""group"":{""id"":"+array[0]+@",""name"":"""+array[1]+@"""}}";
                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/groups/update?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "修改分组名：" + responseStr);
                        }
                        else if (ht["Content"].ToString() == "创建菜单" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{
                             ""button"":[
                             {	
                                  ""type"":""click"",
                                  ""name"":""天气"",
                                  ""key"":""V1001_Weather""
                              },
                              {
                                   ""type"":""click"",
                                   ""name"":""歌手简介"",
                                   ""key"":""V1001_TODAY_SINGER""
                              },
                              {
                                   ""name"":""菜单"",
                                   ""sub_button"":[
                                   {	
                                       ""type"":""view"",
                                       ""name"":""搜索"",
                                       ""url"":""http://www.soso.com/""
                                    },
                                    {
                                       ""type"":""view"",
                                       ""name"":""视频"",
                                       ""url"":""http://v.qq.com/""
                                    },
                                    {
                                       ""type"":""click"",
                                       ""name"":""赞一下我们"",
                                       ""key"":""V1001_GOOD""
                                    }]
                               }]
                         }";

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/menu/create?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "创建菜单返回结果：" + responseStr);
                        }
                        else if (ht["Content"].ToString() == "查询菜单")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/menu/get?access_token=" + token.access_token);
                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "查询菜单返回结果：" + responseStr);
                        }
                        else if (ht["Content"].ToString() == "删除菜单" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/menu/delete?access_token=" + token.access_token);
                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "删除菜单返回结果：" + responseStr);
                        }
                        else if (ht["Content"].ToString() == "上传文件1" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

//                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
//                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
//                            {
//                                responseStr = reader.ReadToEnd();
//                            }

//                            responseXML = string.Format(@"<xml>
//                                <ToUserName><![CDATA[{0}]]></ToUserName>
//                                <FromUserName><![CDATA[{1}]]></FromUserName>
//                                <CreateTime>{2}</CreateTime>
//                                <MsgType><![CDATA[text]]></MsgType>
//                                <Content><![CDATA[{3}]]></Content>
//                                <FuncFlag>0</FuncFlag>
//                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "上传文件：" + responseStr);

                        }
                        else if (ht["Content"].ToString() == "上传图文" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{
   ""articles"": [
		 {
                        ""thumb_media_id"":""MYXB-jam7yeXAgRu43LfTPCQrtD0k44cmn1Qv-1VRPqiqNOb7xdL00B3CH7WJjok"",
                        ""author"":""author1"",
			 ""title"":""Happy Day1"",
			 ""content_source_url"":""www.qq.com"",
			 ""content"":""content1"",
			 ""digest"":""digest1"",
                        ""show_cover_pic"":""1""
		 },
		 {
                        ""thumb_media_id"":""MYXB-jam7yeXAgRu43LfTPCQrtD0k44cmn1Qv-1VRPqiqNOb7xdL00B3CH7WJjok"",
                        ""author"":""author2"",
			 ""title"":""Happy Day2"",
			 ""content_source_url"":""www.qq.com"",
			 ""content"":""content2"",
			 ""digest"":""digest2"",
                        ""show_cover_pic"":""0""
		 },
		 {
                        ""thumb_media_id"":""MYXB-jam7yeXAgRu43LfTPCQrtD0k44cmn1Qv-1VRPqiqNOb7xdL00B3CH7WJjok"",
                        ""author"":""author3"",
			 ""title"":""Happy Day3"",
			 ""content_source_url"":""www.qq.com"",
			 ""content"":""content3"",
			 ""digest"":""digest3"",
                        ""show_cover_pic"":""1""
		 }
   ]
}";

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/media/uploadnews?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "上传图文返回结果：" + responseStr);

                        }
                        else if (ht["Content"].ToString() == "群发图文" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{
   ""filter"":{
      ""group_id"":""0""
   },
   ""mpnews"":{
      ""media_id"":""TBcnunWYUFc3VEqrpKM2i1tGd92HnoTIzUgH1wzd8fbJBZL5i79JPAG-L8OIVe21""
   },
    ""msgtype"":""mpnews""
}";

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/message/mass/sendall?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "群发返回结果：" + responseStr);
                        }
                        else if (ht["Content"].ToString() == "二维码" && fromUser == "o7PrPt7h1JQMbeiNOzNDkqHy0O-M")
                        {
                            HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                            string responseStr = "";
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                            string responseJason = @"{""action_name"": ""QR_LIMIT_SCENE"", ""action_info"": {""scene"": {""scene_id"": 1}}}";

                            Encoding encoding = Encoding.GetEncoding("gb2312");
                            byte[] data = Encoding.UTF8.GetBytes(responseJason);

                            HttpWebRequest request1 = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token=" + token.access_token);
                            request1.Method = "POST";
                            request1.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.1.4322)";
                            request1.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
                            request1.ContentLength = data.Length;
                            Stream stream = request1.GetRequestStream();
                            stream.Write(data, 0, data.Length);
                            stream.Close();

                            HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                            {
                                responseStr = reader.ReadToEnd();
                            }

                            responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "生成二维码返回结果：" + responseStr);
                        }
                        else if (ht["Content"].ToString().Trim().EndsWith("的天气"))
                        {
                            string name = ht["Content"].ToString().Substring(0, ht["Content"].ToString().IndexOf("的天气"));
                            responseXML = SearchCityWeather(name, fromUser, toUser, ConvertDateTimeInt(DateTime.Now));
                        }
                        else
                        {
                            if (flag == "")
                            {
                                responseXML = string.Format(@"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{3}]]></Content>
                            <FuncFlag>0</FuncFlag>
                            </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "回复消息：" + ht["Content"].ToString());
                            }
                            else if (flag == "weather")
                            {
                                responseXML = SearchCityWeather(ht["Content"].ToString().Trim(), fromUser, toUser, ConvertDateTimeInt(DateTime.Now));
                            }

                        }

                    }
                    else if (ht["MsgType"].ToString() == "image")
                    {

                        Face.Face face = new Face.Face();
                        string image = HttpUtility.UrlEncode(ht["PicUrl"].ToString());
                        string jason = face.FaceAnalysis(image);
                        FaceAnalysis faceAnalysis = jason.UserializeJSONToObject<FaceAnalysis>();

                        if (faceAnalysis.face.Count > 0)
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[news]]></MsgType>
                    <ArticleCount>1</ArticleCount>
                    <Articles>
                    <item>
                    <Title><![CDATA[{3}]]></Title> 
                    <Description><![CDATA[{4}]]></Description>
                    <PicUrl><![CDATA[{5}]]></PicUrl>
                    <Url><![CDATA[{6}]]></Url>
                    </item>
                    </Articles>
                    </xml> ", fromUser, toUser, nowTime, "", "年龄：" + faceAnalysis.face[0].attribute.age.value + "  误差：" + faceAnalysis.face[0].attribute.age.range + "  性别：" + faceAnalysis.face[0].attribute.gender.value + "  可信度：" + faceAnalysis.face[0].attribute.gender.confidence + "%  人种：" + faceAnalysis.face[0].attribute.race.value + "  可信度：" + faceAnalysis.face[0].attribute.race.confidence + "%  微笑度：" + faceAnalysis.face[0].attribute.smiling.value + "%", ht["PicUrl"].ToString(), "");
                        }
                        else
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
                    <Content><![CDATA[{3}]]></Content>
                    <FuncFlag>0</FuncFlag>
                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "No Face Find");
                        }
                        
                    }
                    else if (ht["MsgType"].ToString() == "video")
                    {
                        responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[video]]></MsgType>
                    <Video>
                    <MediaId><![CDATA[{3}]]></MediaId>
                    <Title><![CDATA[{4}]]></Title>
                    <Description><![CDATA[{5}]]></Description>
                    </Video> 
                    </xml>", fromUser, toUser, nowTime, ht["MediaId"].ToString(), "视频", "回复的视频");
                    }
                    else if (ht["MsgType"].ToString() == "event")
                    {
                        if (ht["Event"].ToString() == "subscribe")
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
                    <Content><![CDATA[{3}]]></Content>
                    <FuncFlag>0</FuncFlag>
                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "欢迎订阅喵了个咪");
                        }
                        else if (ht["Event"].ToString() == "unsubscribe")
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
                    <Content><![CDATA[{3}]]></Content>
                    <FuncFlag>0</FuncFlag>
                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "取消订阅");
                        }
                        else if (ht["Event"].ToString() == "CLICK")
                        {
//                            responseXML = string.Format(@"<xml>
//                    <ToUserName><![CDATA[{0}]]></ToUserName>
//                    <FromUserName><![CDATA[{1}]]></FromUserName>
//                    <CreateTime>{2}</CreateTime>
//                    <MsgType><![CDATA[text]]></MsgType>
//                    <Content><![CDATA[{3}]]></Content>
//                    <FuncFlag>0</FuncFlag>
//                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "点击事件：" + ht["EventKey"].ToString());

                            if (ht["EventKey"].ToString() == "V1001_Weather")
                            {
                                responseXML = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "输入地区");
                                flag = "weather";
                            }

                        }
                        else if (ht["Event"].ToString() == "SCAN")
                        {
                            responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
                    <Content><![CDATA[{3}]]></Content>
                    <FuncFlag>0</FuncFlag>
                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "扫描二维码事件：(EventKey:" + ht["EventKey"].ToString() + ",TICKET:" + ht["Ticket"].ToString() + ")");
                        }

                    }
                    else if (ht["MsgType"].ToString() == "location")
                    {
                        //地理位置xml
                        //<xml><ToUserName><![CDATA[gh_93fb5feeafe9]]></ToUserName>
                        //<FromUserName><![CDATA[oTC7-sskFgNunEi5Ewk_yYKFJzAc]]></FromUserName>
                        //<CreateTime>1403836036</CreateTime>
                        //<MsgType><![CDATA[location]]></MsgType>
                        //<Location_X>39.984482</Location_X>
                        //<Location_Y>116.312996</Location_Y>
                        //<Scale>16</Scale>
                        //<Label><![CDATA[海淀区海淀路54号]]></Label>
                        //<MsgId>6029429863766789043</MsgId>
                        //</xml>

                        responseXML = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
                    <Content><![CDATA[{3}]]></Content>
                    <FuncFlag>0</FuncFlag>
                    </xml>", ht["FromUserName"].ToString(), ht["ToUserName"].ToString(), ConvertDateTimeInt(DateTime.Now), "当前位置（纬度：" + ht["Location_X"].ToString() + "，经度：" + ht["Location_Y"].ToString() + "，精度：" + ht["Scale"].ToString() + ",地址：" + ht["Label"].ToString() + "）");
                    }

                    wirter.WriteLine("-------------------------------------");
                    wirter.WriteLine(responseXML);
                    wirter.WriteLine("-------------------------------------");
                    wirter.WriteLine(resultStr);

                    wirter.Close();
                }
                catch (Exception ex)
                {
                    wirter.WriteLine("-------------------------------------");
                    wirter.WriteLine(ex.Message);
                }

            }

            return responseXML;
        }


        //查询天气
        private string SearchCityWeather(string name, string fromUser, string toUser, int nowTime)
        { 
            string result = "";

            Weather.Weather weather = new Weather.Weather();
            List<string> weatherResult = weather.GetCityWeather(name, Server.MapPath("~/App_Data/City.xml"));
            if (weatherResult.Count>0)
            {
                CityWeather todyWeather = weatherResult[0].UserializeJSONToObject<CityWeather>();
                CityWeather currntWeather = weatherResult[1].UserializeJSONToObject<CityWeather>();

                result = string.Format(@"<xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[news]]></MsgType>
                    <ArticleCount>3</ArticleCount>
                    <Articles>
                    <item>
                    <Title><![CDATA[{3}]]></Title> 
                    <Description><![CDATA[{4}]]></Description>
                    <PicUrl><![CDATA[{5}]]></PicUrl>
                    <Url><![CDATA[{6}]]></Url>
                    </item>
                    <item>
                    <Title><![CDATA[{7}]]></Title> 
                    <Description><![CDATA[{8}]]></Description>
                    <PicUrl><![CDATA[{9}]]></PicUrl>
                    <Url><![CDATA[{10}]]></Url>
                    </item>
                    <item>
                    <Title><![CDATA[{11}]]></Title> 
                    <Description><![CDATA[{12}]]></Description>
                    <PicUrl><![CDATA[{13}]]></PicUrl>
                    <Url><![CDATA[{14}]]></Url>
                    </item>
                    </Articles>
                    </xml> ",
                    fromUser, toUser, nowTime, "【" + name + "】" + "天气：" + todyWeather.weatherinfo.weather + "  温度：" + currntWeather.weatherinfo.temp + "℃  湿度：" + currntWeather.weatherinfo.SD + "  风速：" + currntWeather.weatherinfo.WD + currntWeather.weatherinfo.WS , "", "", "",
                    "白天：" + todyWeather.weatherinfo.temp1, "", "http://www.weather.com.cn/m2/i/icon_weather/29x20/" + todyWeather.weatherinfo.img1, "",
                    "夜晚：" + todyWeather.weatherinfo.temp2, "", "http://www.weather.com.cn/m2/i/icon_weather/29x20/" + todyWeather.weatherinfo.img2, "");
            }
            else
            {
                result = string.Format(@"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                <FuncFlag>0</FuncFlag>
                                </xml>", fromUser, toUser, nowTime, "无法查询" + name + "当前天气");
            }

            return result;
        }

        /// <summary>

        /// datetime转换为unixtime

        /// </summary>

        /// <param name="time"></param>

        /// <returns></returns>

        private int ConvertDateTimeInt(System.DateTime time)
        {

            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            return (int)(time - startTime).TotalSeconds;

        }

        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        private bool CheckSignature()
        {
            string signature = Request.QueryString["signature"];
            string timestamp = Request.QueryString["timestamp"];
            string nonce = Request.QueryString["nonce"];
            string[] ArrTmp = { Token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void Valid()
        {
            string echoStr = Request.QueryString["echoStr"];
            if (CheckSignature())
            {
                if (!string.IsNullOrEmpty(echoStr))
                {
                    Response.Write(echoStr);
                    Response.End();
                }
            }
        }

        private void Upload1()
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                string responseStr = "";
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseStr = reader.ReadToEnd();
                }

                AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();
                //string path = Server.MapPath("~/Images/d1.jpg");
                //WebClient wx_upload = new WebClient();
                //byte[] result = wx_upload.UploadFile(new Uri(String.Format("http://file.api.weixin.qq.com/cgi-bin/media/upload?access_token={0}&type={1}", token.access_token, "image")), path);
                //string resultjson = Encoding.Default.GetString(result);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }


        //40005
        private void Upload()
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + secret);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                string responseStr = "";
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseStr = reader.ReadToEnd();
                }

                AccessToken token = responseStr.UserializeJSONToObject<AccessToken>();

                string path = Server.MapPath("~/Images/d1.jpg");

                FileStream fStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                //时间戳,用于结束标志
                string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");
                //表头
                //string filePartHeader = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" + "Content-Type: application/octet-stream\r\n\r\n", "media", "a.jpg");

                StringBuilder sb = new StringBuilder();
                sb.Append("--");
                sb.Append(strBoundary);
                sb.Append("\r\n");
                sb.Append("Content-Disposition: form-data; name=\"");
                sb.Append("file");
                sb.Append("\"; filename=\"");
                sb.Append("a");
                sb.Append("\"");
                sb.Append("\r\n");
                sb.Append("Content-Type: ");
                sb.Append("application/octet-stream");
                sb.Append("\r\n");
                sb.Append("\r\n");
                string strPostHeader = sb.ToString();
                var headerbytes = Encoding.UTF8.GetBytes(strPostHeader);
                // 边界符  
                //var beginBoundary = Encoding.ASCII.GetBytes("--" + strBoundary + "\r\n");
                // 最后的结束符  
                var endBoundary = Encoding.ASCII.GetBytes("--" + strBoundary + "--\r\n");
                HttpWebRequest request1 = WebRequest.CreateHttp("http://file.api.weixin.qq.com/cgi-bin/media/upload?access_token=" + token.access_token + "&type=image");
                request1.Method = "POST";
                request1.Timeout = 300000;
                request1.ContentType = "multipart/form-data; boundary=" + strBoundary;
                request1.ContentLength = fStream.Length + headerbytes.Length + endBoundary.Length;
                request1.AllowWriteStreamBuffering = false;
                request1.KeepAlive = true;

                Stream stream = request1.GetRequestStream();
                //stream.Write(beginBoundary, 0, beginBoundary.Length);
                stream.Write(headerbytes, 0, headerbytes.Length);

                int byteReadCount;
                var buffer = new byte[1024];
                while ((byteReadCount = fStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    stream.Write(buffer, 0, byteReadCount);
                }

                //添加尾部的时间戳 
                stream.Write(endBoundary, 0, endBoundary.Length);
                
                stream.Close();

                HttpWebResponse response1 = request1.GetResponse() as HttpWebResponse;
                using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                {
                    responseStr = reader.ReadToEnd();
                }
            }
            catch (Exception ex) 
            {
                string s = ex.Message;
            }
        }

        private void DownLoad()
        {
            string ticket =Server.UrlEncode("gQGh8DoAAAAAAAAAASxodHRwOi8vd2VpeGluLnFxLmNvbS9xL0xFeXJJMm5rbEVqdHBUaVFlbUFvAAIEvcm0UwMEAAAAAA==");

            WebClient wx_upload = new WebClient();
            wx_upload.DownloadFile(new Uri("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=" + ticket), "C:/qrcode.png");
        }

        #endregion
    }
}
