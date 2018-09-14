using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bigdata
{
    /// <summary>
    /// 借阅排行榜
    /// 接口用途： 获取指定时间段内的排名前 10 的图书信息
    /// </summary>
    public static class BorrowOrderInfo
    {
        public static String GetDta(String start_time, String end_time)
        {
            String result = "1";
            String error_msg = "";
            String msg = "";
            String author = "";
            String book_name = "";
            String isbn = "";
            String read_url = "";
            String pubdate = "";
            String publisher = "";
            String summary = "";
            String cover_url = "";
            String bnum = "";

            string connStr = String.Format("User Id={0};Password={1};Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={2})(PORT={3})))(CONNECT_DATA=(SERVICE_NAME={4})))",
              ConfigurationManager.AppSettings["OracleUserId"].ToString(),
              ConfigurationManager.AppSettings["OraclePassword"].ToString(),
              ConfigurationManager.AppSettings["OracleHost"].ToString(),
              ConfigurationManager.AppSettings["OraclePort"].ToString(),
              ConfigurationManager.AppSettings["OracleServiceName"].ToString()
              );

            if (start_time == "")
            {
                start_time = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            }

            if (end_time == "")
            {
                end_time = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            }

            JArray jArray = new JArray();

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT * FROM(
SELECT sum(借阅次数) as 借阅总次数,题名 FROM 
(
SELECT c.索书号, count(*) as 借阅次数, c.题名, c.责任者 
FROM 流通日志 a, 读者库 b, 馆藏书目库 c, 馆藏典藏库 d
WHERE
     a.处理时间 BETWEEN to_date('" + start_time + @"','yyyy-mm-dd hh24:mi:ss') AND  to_date('" + end_time + @"','yyyy-mm-dd hh24:mi:ss')
     AND a.操作类型='J'
     AND a.读者条码=b.读者条码
     AND a.条形码 IN (SELECT 条形码 FROM 馆藏典藏库 WHERE 索书号 LIKE 'I%')
     AND a.条形码=d.条形码 
     AND d.主键码=c.主键码
     group by c.索书号,c.题名,c.责任者 
     order by 题名 desc
) 
group by  题名
ORDER BY 借阅总次数 desc
)
WHERE rownum <=10
";
                    List<Tuple<String, String>> bookList = new List<Tuple<string, string>>();
                    OracleDataReader oracleDataReader = cmd.ExecuteReader();
                    while(oracleDataReader.Read())
                    {
                        bookList.Add(new Tuple<string, string>(oracleDataReader["借阅总次数"].ToString().Trim(), oracleDataReader["题名"].ToString().Trim()));
                    }
                    oracleDataReader.Close();

                    //查询每一本书
                    foreach (Tuple<string, string> item in bookList)
                    {
                        cmd.CommandText = "SELECT * FROM 馆藏书目库 WHERE 题名 LIKE '"+ item.Item2.ToString() + "'";
                        oracleDataReader = cmd.ExecuteReader();


                        if (oracleDataReader.Read())
                        {
                            JObject jObjectMsg = new JObject();
                            //作者
                            jObjectMsg.Add("author", oracleDataReader["责任者"].ToString().Trim());
                            //书名
                            jObjectMsg.Add("book_name", item.Item2.ToString());
                            //图书编号
                            jObjectMsg.Add("isbn", oracleDataReader["标准编码"].ToString().Trim());
                            //扫描可在线阅读的 url
                            jObjectMsg.Add("read_url", read_url);
                            //出版时间
                            jObjectMsg.Add("pubdate", oracleDataReader["出版日期"].ToString().Trim());
                            //出版社
                            jObjectMsg.Add("publisher", oracleDataReader["出版者"].ToString().Trim());
                            //附注
                            jObjectMsg.Add("summary", oracleDataReader["summarys"].ToString().Trim());
                            //图书封面
                            jObjectMsg.Add("cover_url", oracleDataReader["coverpath"].ToString().Trim());
                            //借阅次数数量，以整数为单位
                            jObjectMsg.Add("bnum", item.Item1.ToString());
                            jArray.Add(jObjectMsg);
                        }
                    }
                }
            }



            JObject jObject = new JObject();
            //result=1 表示接口调用成功，获取的信息放入 msg 节点中
            jObject.Add("result", result);
            //错误信息提示语，例如：鉴权失败，服务器故障等
            jObject.Add("error_msg", error_msg);
            //Msg 为多条图书信息
            jObject.Add("msg", jArray);

           
            

            return jObject.ToString(Newtonsoft.Json.Formatting.None, null);
        }
    }
}