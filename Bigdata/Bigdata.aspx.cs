using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bigdata
{
    public partial class Bigdata : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                JObject jObject = new JObject();
                jObject.Add("result", "0");
                jObject.Add("error_msg", "请用GET请求！");
                Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None, null));
                return;
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("result", "0");


                String cmd = Request.QueryString["cmd"];
                if(cmd==null || cmd=="")
                {
                    jObject.Add("error_msg", "缺少参数(cmd)！");
                    Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None,null));
                    return;
                }

                String appkey = Request.QueryString["appkey"];
                if (appkey == null || appkey == "")
                {
                    jObject.Add("error_msg", "缺少参数(appkey)！");
                    Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None, null));
                    return;
                }

                //（若此参数不传，默认当 天 0 点 0 分 0 秒）
                String start_time = Request.QueryString["start_time"]; 
                if (start_time == null)
                {
                    start_time = "";
                }
                else
                {
                    if (start_time != "")
                    {
                        try
                        {
                            String dataString = start_time.Substring(0, 4) + 
                                "-" + start_time.Substring(4, 2) + 
                                "-" + start_time.Substring(6, 2) + 
                                " " + start_time.Substring(8, 2) + 
                                ":" + start_time.Substring(10, 2) + 
                                ":" + start_time.Substring(12, 2);
                            DateTime.Parse(dataString);
                            start_time = dataString;
                        }
                        catch
                        {
                            jObject.Add("error_msg", "时间（start_time）格式错误！");
                            Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None, null));
                            return;
                        }
                    }
                }

                String end_time = Request.QueryString["end_time"];
                if (end_time == null)
                {
                    end_time = "";
                }
                else
                {
                    if (end_time != "")
                    {
                        try
                        {
                            String dataString = end_time.Substring(0, 4) +
                                                           "-" + end_time.Substring(4, 2) +
                                                           "-" + end_time.Substring(6, 2) +
                                                           " " + end_time.Substring(8, 2) +
                                                           ":" + end_time.Substring(10, 2) +
                                                           ":" + end_time.Substring(12, 2);
                            DateTime.Parse(dataString);
                            end_time = dataString;
                        }
                        catch
                        {
                            jObject.Add("error_msg", "时间（end_time）格式错误！");
                            Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None, null));
                            return;
                        }
                    }
                }

                //命令转换成小写
                cmd = cmd.ToLower();
                switch (cmd)
                {
                    case "camecountinfo":
                        Response.Write(CameCountInfo.GetDta(start_time, end_time));
                        return;
                    case "borroworderinfo":
                        Response.Write(BorrowOrderInfo.GetDta(start_time, end_time));
                        return;
                    case "borrowbackcountinfo":
                        Response.Write(BorrowBackCountInfo.GetDta(start_time, end_time));
                        return;
                    default:
                        break;
                }

                jObject.Add("error_msg", "指令错误！");
                Response.Write(jObject.ToString(Newtonsoft.Json.Formatting.None, null));
                return;

            }

            // http://202.115.129.156:8081/Bigdata.aspx?cmd=CameCountInfo&appkey=yonghuxuke&start_time=20171011000000&end_time=20171011000000   
            // http://202.115.129.156:8081/Bigdata.aspx?cmd=BorrowOrderInfo&appkey=yonghuxuke&start_time=20171010000000&end_time=20171011000000   
            // http://202.115.129.156:8081/Bigdata.aspx?cmd=BorrowBackCountInfo&appkey=yonghuxuke&start_time=20171010000000&end_time=20171011000000   
        }
    }
}