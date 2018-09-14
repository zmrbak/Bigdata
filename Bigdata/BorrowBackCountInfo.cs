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
    /// 借还书系统
    /// 接口用途：获取指定时间段内的借出图书数量和归还图书数量
    /// </summary>
    public static class BorrowBackCountInfo
    {
        public static String GetDta(String start_time, String end_time)
        {
            String result = "1";
            String error_msg = "";
            String borrow_count = "";
            String back_count = "";

            string connStr = String.Format("User Id={0};Password={1};Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={2})(PORT={3})))(CONNECT_DATA=(SERVICE_NAME={4})))",
              ConfigurationManager.AppSettings["OracleUserId"].ToString(),
              ConfigurationManager.AppSettings["OraclePassword"].ToString(),
              ConfigurationManager.AppSettings["OracleHost"].ToString(),
              ConfigurationManager.AppSettings["OraclePort"].ToString(),
              ConfigurationManager.AppSettings["OracleServiceName"].ToString()
              );

            if (start_time == "")
            {
                start_time = DateTime.Now.ToString("yyyyMMdd") + " 00:00:00";
            }

            if (end_time == "")
            {
                end_time = DateTime.Now.ToString("yyyy-mm-dd HH-MM-ss");
            }

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    //借出的图书数量
                    cmd.CommandText =
                        @" SELECT count(*) 借阅量 FROM 流通日志" +
                        @" where " +
                        @" 操作类型 like 'J'" +
                        @" AND 处理时间  BETWEEN to_date('" + start_time + "','yyyy-mm-dd hh24:mi:ss') AND  to_date('" + end_time + "','yyyy-mm-dd hh24:mi:ss')";
                    object J = cmd.ExecuteScalar();
                    borrow_count = J.ToString();

                    cmd.Parameters.Clear();
                    cmd.CommandText =
                        @" SELECT count(*) 借阅量 FROM 流通日志" +
                        @" where " +
                        @" 操作类型 like 'H'" +
                        @" AND 处理时间  BETWEEN to_date('" + start_time + "','yyyy-mm-dd hh24:mi:ss') AND  to_date('" + end_time + "','yyyy-mm-dd hh24:mi:ss')";
                    object H = cmd.ExecuteScalar();
                    back_count = H.ToString();
                }
            }



            JObject jObject = new JObject();
            //result=1 表示接口调用成功，获取的信息放入 msg 节点中
            jObject.Add("result", result);
            //错误信息提示语，例如：鉴权失败，服务器故障等
            jObject.Add("error_msg", error_msg);
            //借出图书的数量
            jObject.Add("borrow_count", borrow_count);
            //还入图书的数量
            jObject.Add("back_count", back_count);
            return jObject.ToString(Newtonsoft.Json.Formatting.None, null);
        }
    }
}