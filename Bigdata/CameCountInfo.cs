using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bigdata
{
    /// <summary>
    /// 到馆统计数据接口规范
    /// 接口用途：获取指定时间段之内的到馆人数和在馆人数（也可以说是指定时间段内的在馆人数和离开人数）
    /// </summary>
    public static class CameCountInfo
    {
        public static String GetDta(String start_time,String end_time)
        {
            String result = "1";
            String error_msg = "";
            String count_by_here = "";
            String count_by_all = "";

            string connStr = String.Format("User Id={0};Password={1};Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={2})(PORT={3})))(CONNECT_DATA=(SERVICE_NAME={4})))",
                ConfigurationManager.AppSettings["OracleUserId"].ToString(),
                ConfigurationManager.AppSettings["OraclePassword"].ToString(),
                ConfigurationManager.AppSettings["OracleHost"].ToString(),
                ConfigurationManager.AppSettings["OraclePort"].ToString(),
                ConfigurationManager.AppSettings["OracleServiceName"].ToString()
                );

            if(start_time=="")
            {
                start_time = DateTime.Now.ToString("yyyyMMdd")+" 00:00:00";
            }

            if(end_time=="")
            {
                end_time = DateTime.Now.ToString("yyyy-mm-dd HH-MM-ss");
            }

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT count(*) FROM  阅览日志 WHERE 登到时间 BETWEEN to_date('"+ start_time + "','yyyy-mm-dd hh24:mi:ss') AND  to_date('" + end_time + "','yyyy-mm-dd hh24:mi:ss')";
                    //cmd.Parameters.AddRange(parameters);
                    object CameCount=cmd.ExecuteScalar();
                    count_by_here = CameCount.ToString();
                    count_by_all = count_by_here;
                }
            }

            JObject jObject = new JObject();
            //result=1 表示接口调用成功，获取的信息放入 msg 节点中
            jObject.Add("result", result);
            //错误信息提示语，例如：鉴权失败，服务器故障等
            jObject.Add("error_msg", error_msg);
            //当前时间段内的在馆人数，若无法提供此参数，请返回数字 0
            jObject.Add("count_by_here", count_by_here);
            //当前时间段内的到过馆的总人次
            jObject.Add("count_by_all", count_by_all);
            return jObject.ToString(Newtonsoft.Json.Formatting.None, null);
        }
    }
}