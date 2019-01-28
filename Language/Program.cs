using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using Excel;
using Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Language
{
    class Program
    {
        public static int tttt = 0;
        //不需要的语句
        public static string[] noHaveWord = { "Log.d", "Log.D", "Log.e", "Log.E", "Log.SE", "Log.W", "Log.w", "Log.o", "Log.ow", "Log.oe", "Debug.Log", "Debug.LogError", "error", "print", "Debug.LogWarning" };

        public static string[] needChangeScrArr = { "VersionCheck.cs"};

        public static string[] noHavePre = { "level01_02preload.prefab", "Level01_prefab.prefab", "PathFind.prefab", "GM.prefab", "Level01_02_prefab.prefab", "triggers_black_pearl.prefab" };

        //存放需要读取得数据库名字
        public static string[] sqlAll;

        //需要翻译的表和列名
        public static Dictionary<string,string[]> noNeedTableRole;


        //用来判断是翻译什么语言， 1.繁体
        public static int index = 0;

        //当前应用程序所处的上一级路径
        public static string allPath = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().LastIndexOf("\\"));
        //string s2 = Environment.CurrentDirectory;

        //存储写入表格文件中的内容，key是需要翻译的内容，value是这段话出现的脚本名字 
        public static Dictionary<string, string> _alldic = new Dictionary<string, string>();

        //从表里读出来的翻译文件
        public static Dictionary<string, string> _langDic = new Dictionary<string, string>();
        //脚本内容
        public static List<object> _allConent;

        //翻译文件生成的路径名
        private static string OutPath = Directory.GetCurrentDirectory();


        public static string sqlConfig = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.txt";
        //数据库
        public static string IpAddress = "192.168.8.246";
        public static string Uid = "root";
        public static string Pwd = "db1234";
        //public static string SqlName = "poc_admin";    //Ip下随便找一个数据库的名字

        //excel这一列存放路径，展示数据库读的时候会用到
        public static int filePathIndex = 15;

        //一个数据库对应多张表和列
        public static Dictionary<string, List<string[]>> sqlTabColAll = new Dictionary<string, List<string[]>>();
        //数据库名
        public static string sqlNameForExcel = "";
        //excel15列读取出来的数据 表名和列名,数组长度固定为2
        public static List<string[]> TabColForOneSql = new List<string[]>();

        public static List<int> strError1 = new List<int>();
        public static List<int> strError2 = new List<int>();
        public static List<int> strError3 = new List<int>();
        public static List<int> strError4 = new List<int>();

        //指定要修改的文件
        public static List<string> limitLua = new List<string>();
        public static List<string> limitSrc = new List<string>();
        public static List<string> limitPrefeb = new List<string>();
        public static List<string> limitTable = new List<string>();

        //public static List<string> _allSqlTable;
        static void Main(string[] args)
        {
            Console.WriteLine("读取配置文件开始");
            ReadConfig();
            Console.WriteLine("读取配置文件完成");
            for(int i = 0; i < sqlAll.Length; i++)
            {
                Console.WriteLine(sqlAll[i]);
            }

            if (args != null)
            {             
                int argsLength = args.Length;
                if (argsLength <= 0)
                    Console.WriteLine("请输入完整的命令");
                for (int i = 0; i < argsLength; i++)
                {
                    Console.WriteLine("执行命令："+args[i]);
                    switch (args[i])
                    {
                        case "ExportKey":
                            Console.WriteLine("导出资源\n");
                            ReadAssets();
                            Console.WriteLine("##########" + tttt);
                            break;
                        case "ImportFT":
                            Console.WriteLine("导入繁体资源\n");
                            index = 1;
                            ImportLanguage();
                            break;
                        case "ImportLan1":
                            Console.WriteLine("导入语言1\n");
                            index = 2;
                            ImportLanguage();
                            break;
                        case "ImportLan2":
                            Console.WriteLine("导入语言2\n");
                            index = 3;
                            ImportLanguage();
                            break;
                        case "ImportLan3":
                            Console.WriteLine("导入语言3\n");
                            index = 4;
                            ImportLanguage();
                            break;
                        case "Limit1":
                            Console.WriteLine("指定第一列\n");
                            index = 1;
                            ChangeLimitFile();
                            break;
                        case "Limit2":
                            Console.WriteLine("指定第二列\n");
                            index = 2;
                            ChangeLimitFile();
                            break;
                        case "Limit3":
                            Console.WriteLine("指定第三列\n");
                            index = 3;
                            ChangeLimitFile();
                            break;
                        case "Limit4":
                            Console.WriteLine("指定第四列\n");
                            index = 4;
                            ChangeLimitFile();
                            break;
                        case "curPath":
                            Console.WriteLine("当前配置路径："+ sqlConfig);
                            break;
                        case "fixErrar":
                            Console.WriteLine("文档合并:");
                            MergeExcel();
                            break;
                       
                        default:
                            Console.WriteLine(args[i]+": 无效的命令");
                            break;
                    }
                }
            }
            //ReadEx(OutPath);

            Console.ReadLine();
        }

        public static void ReadConfig()
        {
            //using (System.IO.StreamReader sr = System.IO.File.OpenText("D:\\VS2017\\Language\\Language\\bin\\Debug\\config.txt"))
            using (System.IO.StreamReader sr = System.IO.File.OpenText(sqlConfig))            
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    IpAddress = o["IpAddress"].ToString();
                    Uid = o["Uid"].ToString();
                    Pwd = o["Pwd"].ToString();
                    //SqlName = o["SqlName"].ToString();

                    int len = o["SqlName"].ToArray().Length;
                    sqlAll = new String[len];
                    for (int i = 0; i < len; i++)
                    {
                        sqlAll[i] = o["SqlName"].ToArray()[i].ToString();
                    }

                    len = o["noNeedTable"].ToArray().Length;
                    int num = 0;
                    noNeedTableRole = new Dictionary<string, string[]>();
                    for (int i = 0; i < len; i++)
                    {
                        string str = o["noNeedTable"].ToArray()[i].ToString();
                        string[] strArr;
                        if (o[str] != null)
                        {
                            num = o[str].ToArray().Length;
                            strArr = new String[num];
                            for (int j = 0; j < num; j++)
                            {
                                strArr[j] = o[str].ToArray()[j].ToString(); 
                            }
                            noNeedTableRole.Add(str, strArr);
                        }
                    }             
                    
                    foreach(KeyValuePair<string,string[]> item in noNeedTableRole)
                    {
                        Console.WriteLine("key：" + item.Key);
                        for(int i = 0; i< item.Value.Length; i++)
                        {
                            Console.WriteLine("value:" + item.Value[i]);
                        }
                    }

                    int length = o["LimitLua"].ToArray().Length;
                    for(int i = 0; i < length; i++)
                    {
                        limitLua[i] = o["LimitLua"].ToArray()[i].ToString();
                    }
                    //length = o["LimitSrc"].ToArray().Length;
                    //for (int i = 0; i < length; i++)
                    //{
                    //    limitSrc[i] = o["LimitSrc"].ToArray()[i].ToString();
                    //}
                    length = o["LimitPrefab"].ToArray().Length;
                    for (int i = 0; i < length; i++)
                    {
                        limitPrefeb[i] = o["LimitPrefab"].ToArray()[i].ToString();
                    }
                }
                Console.WriteLine("当前配置路径：" + sqlConfig);

                
            }
        }

        public static void MergeExcel()
        {
            Dictionary<string, string> excel1 = new Dictionary<string, string>();   //多余的
            Dictionary<string, string> excel2 = new Dictionary<string, string>();

            Dictionary<string, string> excel3 = new Dictionary<string, string>();

            FileInfo info;
            FileStream stream;
            IExcelDataReader excelReader;

            string path = Directory.GetCurrentDirectory() + "/翻译1.xls";
            Console.WriteLine(path);

            info = new FileInfo(path);
            stream = info.Open(FileMode.Open, FileAccess.Read);
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            do
            {
                while (excelReader.Read())               //一行
                {
                    string ch = excelReader.IsDBNull(0) ? "" : excelReader.GetString(0);
                    string en = excelReader.IsDBNull(14) ? "" : excelReader.GetString(14);

                    if (ch != "" && en != "")
                    {
                        Console.WriteLine("ch:" + ch + "en:" + en);
                        excel1.Add(ch, en);
                    }                 
                 }
            } while (excelReader.NextResult());
            excelReader.Close();
            stream.Close();

            path = Directory.GetCurrentDirectory() + "/翻译2.xls";
            info = new FileInfo(path);
            stream = info.Open(FileMode.Open, FileAccess.Read);
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            do
            {
                while (excelReader.Read())               //一行
                {
                    string ch = excelReader.IsDBNull(0) ? "" : excelReader.GetString(0);
                    string en = excelReader.IsDBNull(1) ? "" : excelReader.GetString(1);

                    string add = excelReader.IsDBNull(14) ? "" : excelReader.GetString(14);
                    if (ch != "" && en != "")
                    {
                        if (excel1.ContainsKey(ch))
                        {
                            if(!add.Contains(excel1[ch]))
                                add = add + "&" + excel1[ch];
                            excel3.Add(ch, add);
                        }else
                        {
                            excel3.Add(ch, add);
                        }

                        excel2.Add(ch, en);
                    }
                }
            } while (excelReader.NextResult());
            excelReader.Close();
            stream.Close();



            //文件路径和名字
            string outPutDir = Directory.GetCurrentDirectory() + "\\翻译.xls";

            FileInfo newFile = new FileInfo(outPutDir);

            //如果已经有此文件，删除重新创建一个
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(outPutDir);
            }

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                //新建一个sheet，名字自己命名
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("翻译");

                //写入元素
                worksheet.Cells[1, 1].Value = "key";
                worksheet.Cells[1, 2].Value = "繁体";
                worksheet.Cells[1, 3].Value = "语言1";
                worksheet.Cells[1, 4].Value = "语言2";
                worksheet.Cells[1, 5].Value = "语言3";

                int i = 0;
                foreach (KeyValuePair<string, string> k in excel2)
                {
                    worksheet.Cells[i + 2, 1].Value = k.Key;
                    worksheet.Cells[i + 2, 2].Value = k.Value;
                    worksheet.Cells[i + 2, 15].Value = excel3[k.Key];
                    i++;
                }

                //保存文件
                package.Save();
            }
        }

        #region 翻译资源导出

        public static void ReadAssets()
        {
            Console.WriteLine("解析lua脚本开始");
            ReadAllLua(new DirectoryInfo(allPath));
            Console.WriteLine("解析lua脚本完成\n");

            Console.WriteLine("解析.cs脚本");
            ReadAllScr(new DirectoryInfo(allPath));
            Console.WriteLine("解析.cs脚本结束\n");

            Console.WriteLine("解析预制体开始");
            ReadAllPre(new DirectoryInfo(allPath));
            Console.WriteLine("解析预制体结束\n");

            Console.WriteLine("解析数据库表格开始");
            ReadAllSql();
            Console.WriteLine("解析数据库表格结束");

            TxtWritePre(OutPath);
        }

        //lua脚本部分
        #region
        //读所有lua
        public static void ReadAllLua(DirectoryInfo dictoryInfo)
        {
            if (!dictoryInfo.Exists) return;

            //拿到所有的脚本
            FileInfo[] fileInfos = dictoryInfo.GetFiles("*.lua.bytes", SearchOption.AllDirectories); //SearchOption.AllDirectories

            //遍历脚本
            foreach (FileInfo files in fileInfos)
            {
                //_allConent = new List<object>();
                //_chList = new List<string>();
                string path = files.FullName;
                //Debug.Log(path);
                Console.WriteLine("解析：" + files.FullName);
                Read1(files.FullName);
                //_alldic.Add(files.Name, _chList);
            }
        }

        //读所有.cs
        public static void ReadAllScr(DirectoryInfo dictoryInfo)
        {
            if (!dictoryInfo.Exists) return;

            //拿到所有的脚本
            FileInfo[] fileInfos = dictoryInfo.GetFiles("*.cs", SearchOption.AllDirectories);

            //遍历脚本
            foreach (FileInfo files in fileInfos)
            {
                for (int i = 0; i < needChangeScrArr.Length; i++)
                {
                    if (files.Name==needChangeScrArr[i])
                    {
                        //_allConent = new List<object>();
                        //_chList = new List<string>();
                        string path = files.FullName;
                        //Debug.Log(path);
                        Console.WriteLine("解析：" + files.FullName);
                        Read2(files.FullName);
                        //_alldic.Add(files.Name, _chList);
                    }
                }
            }
        }

        //读一个Lua
        static void Read1(string file)
        {
            StreamReader sr = new StreamReader(File.OpenRead(file));          //Application.dataPath + "/lua/" + file.Name 
            StringBuilder sb;
            string line = "";
            bool isNotes = false;
            while ((line = sr.ReadLine()) != null)
            {
                sb = new StringBuilder();
                //删除注释
                if (line.Trim().StartsWith("--[["))
                {
                    isNotes = !isNotes;
                    if (line.Trim().EndsWith("]]"))
                    {
                        isNotes = !isNotes;
                    }
                    continue;
                }
                if (line.Trim().EndsWith("]]"))
                {
                    isNotes = !isNotes;
                    continue;
                }
                if (isNotes)
                {
                    continue;
                }
                if (line.Trim().StartsWith("--"))
                {
                    continue;
                }
                //删除注释结束
                if (IsStartWithLog(line.Trim()))
                {
                    continue;
                }



                int num = FindWord(line, 0, "--");
                string lineEnd = "";
                if (num >= 0)
                {
                    lineEnd = line.Substring(num);
                    line = line.Substring(0, num);
                }

                string arr="";
                if (line.Contains("\""))
                {
                    arr = ReadLine2(line);
                }
                else
                {
                    arr = ReadLine(line);
                }

                //foreach (string st in arr)
                {
                    if (!_alldic.ContainsKey(arr) && !string.IsNullOrEmpty(arr) && arr != "··")
                    {
                        _alldic.Add(arr, file);
                    }
                }
            }

            sr.Close();
            sr.Dispose();
        }

        //都一个.cs
        static void Read2(string file)
        {
            StreamReader sr = new StreamReader(File.OpenRead(file));          //Application.dataPath + "/lua/" + file.Name 
            StringBuilder sb;
            string line = "";
            bool isNotes = false;
            while ((line = sr.ReadLine()) != null)
            {
                sb = new StringBuilder();
                //删除注释
                if (line.Trim().StartsWith("/*"))
                {
                    isNotes = !isNotes;
                    if (line.Trim().EndsWith("*/"))
                    {
                        isNotes = !isNotes;
                    }
                    continue;
                }
                if (line.Trim().EndsWith("*/"))
                {
                    isNotes = !isNotes;
                    continue;
                }
                if (isNotes)
                {
                    continue;
                }
                if (line.Trim().StartsWith("//"))
                {
                    continue;
                }
                //删除注释结束
                if (IsStartWithLog(line.Trim()))
                {
                    continue;
                }



                int num = FindWord(line, 0, "//");
                string lineEnd = "";
                if (num >= 0)
                {
                    lineEnd = line.Substring(num);
                    line = line.Substring(0, num);
                }

                string arr="";
                if (line.Contains("\""))
                {
                    arr = ReadLine2(line);
                }
                else
                {
                    arr = ReadLine(line);
                }

                //foreach (string st in arr)
                {
                    if (!_alldic.ContainsKey(arr) && !string.IsNullOrEmpty(arr) && arr != "··")
                    {
                        _alldic.Add(arr, file);
                    }
                }
            }
            sr.Close();
            sr.Dispose();
        }

        //读一行，筛选需要的内容
        static string ReadLine(string line)
        {
            List<string> resultArr = new List<string>();
            line = line.Trim();
            StringBuilder sb = new StringBuilder();
            StringBuilder testSb = new StringBuilder();
            bool isBegin = false;
            for (int i = 0; i < line.Length; i++)
            {
                //确保在单引号''里
                if (line[i] == '\'' && isBegin == true)
                {
                    if (IsLineNeed(sb.ToString()))
                    {
                        //resultArr.Add(sb.ToString());
                        testSb.Append(sb.ToString());
                        testSb.Append("·");
                    }
                    sb = new StringBuilder();
                    isBegin = !isBegin;
                    continue;
                }
                else if (line[i] == '\'')
                {
                    isBegin = !isBegin;
                    continue;
                }

                if (isBegin == false)
                {
                    continue;
                }
                sb.Append(line[i]);
            }
            //return testSb.ToString();
            if (testSb.ToString() != "")
            {
                if (testSb[testSb.Length - 1] == '·')
                {
                    testSb.Remove(testSb.Length - 1, 1);
                }
            }
            return testSb.ToString();
        }

        static string ReadLine2(string line)
        {
            List<string> resultArr = new List<string>();
            line = line.Trim();
            StringBuilder sb = new StringBuilder();
            StringBuilder testSb = new StringBuilder();

            bool isBegin = false;
            for (int i = 0; i < line.Length; i++)
            {

                //确保在双引号''里
                if ((line[i] == '\"' && (i == 0 || line[i - 1] != '\\')) && isBegin == true)
                {
                    if (IsLineNeed(sb.ToString()))
                    {
                        //resultArr.Add(sb.ToString());
                        testSb.Append(sb.ToString());
                        testSb.Append("·");
                    }
                    sb = new StringBuilder();
                    isBegin = !isBegin;
                    continue;
                }
                else if ((line[i] == '\"' && (i == 0 || line[i - 1] != '\\')))
                {
                    isBegin = !isBegin;
                    continue;
                }

                if (isBegin == false)
                {
                    continue;
                }
                sb.Append(line[i]);
            }

            //return testSb.ToString();
            if (testSb.ToString() != "")
            {
                if (testSb[testSb.Length - 1] == '·')
                {
                    testSb.Remove(testSb.Length - 1, 1);
                }
            }
            return testSb.ToString();
        }
        #endregion

        //预制体部分
        #region
        //读所有预制体
        public static void ReadAllPre(DirectoryInfo dictoryInfo)
        {
            if (!dictoryInfo.Exists) return;
            //_alldic = new Dictionary<string, string>();

            //拿到所有的预制体
            FileInfo[] fileInfos = dictoryInfo.GetFiles("*.prefab", SearchOption.AllDirectories);

            //遍历预制体
            foreach (FileInfo files in fileInfos)
            {
                bool isCan = true;
                for (int i = 0; i < noHavePre.Length; i++)
                {
                    if(files.Name==noHavePre[i])
                    {
                        isCan = false;
                        continue;
                    }
                }

                if (isCan)
                {
                    string path = files.FullName;
                    Console.WriteLine("解析预设：" + path);
                    ReadPre(path);
                }
            }
        }

        //读一个预制体
        static void ReadPre(string file)
        {
            //Process cmd = new Process();
            //cmd.StartInfo.FileName = "qq音乐的可执行程序文件的完整路径名";
            //cmd.StartInfo.Arguments = @"MP3的完整路径名";

            StreamReader sr = new StreamReader(File.OpenRead(file));          //Application.dataPath + "/lua/" + file.Name 
            StringBuilder sb;
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                //循环刷新
                sb = new StringBuilder();
                //找到一个"变一次相反
                bool isBegin = false;
                //m_Text代表Text组件
                //if (line.StartsWith("m_Text:"))
                if (line.Contains("\"") && line.Contains("\\u"))
                {
                    for (int i = 0; i < line.Length; i++)
                    {

                        if (line[i] == '"')
                        {
                            isBegin = !isBegin;
                            continue;
                        }
                        if (isBegin)
                            sb.Append(line[i]);
                    }

                    if (!_alldic.ContainsKey(Unicode2String(sb.ToString())) && !string.IsNullOrEmpty(sb.ToString()) && !IsLineWordLoop(Unicode2String(sb.ToString())) && IsLineNeed(Unicode2String(sb.ToString())))
                    {
                        _alldic.Add(Unicode2String(sb.ToString()), file);
                    }
                    Console.WriteLine(Unicode2String(sb.ToString()));
                }
            }
            sr.Close();
            sr.Dispose();
        }

        #endregion

        //解析数据库表格
        #region
        public static void ReadAllSql()
        {
            List<string> allSqlTable = new List<string>();

            //192.168.8.246;initial catalog=mysql;uid=root;pwd=db1234;SslMode=none;";
            string sqlUrl = "data source=" + IpAddress + ";uid=" + Uid + ";pwd=" + Pwd + ";SslMode=none;";  //连接不是本机的数据库要加  SslMode=none;
            //SqlConnection Connection = new SqlConnection(String.Format("Data Source={0};Initial Catalog=master;User ID={1};PWD={2}", IpAddress, Uid, Pwd));
            MySqlConnection conn = new MySqlConnection(sqlUrl);
            try
            {
                conn.Open();
                //string sql = "select table_name from information_schema.tables where table_schema = 'performance_schema'";
                string sql = "show databases";     //查询所有的数据库名
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader[0].ToString());
                    allSqlTable.Add(reader[0].ToString());
                }

                foreach (string sqlName in allSqlTable)
                {
                    for (int i = 0; i < sqlAll.Length; i++)
                    {
                        if (sqlName == sqlAll[i])
                        {
                            ReadSql(sqlName);
                            continue;
                        }
                    }
                }
                conn.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        //读一个数据库
        public static void ReadSql(string sqlName)
        {
            string sqlUrl = "data source=" + IpAddress + ";uid=" + Uid + ";pwd=" + Pwd + ";SslMode=none;";  //连接不是本机的数据库要加  SslMode=none;
            //SqlConnection Connection = new SqlConnection(String.Format("Data Source={0};Initial Catalog=master;User ID={1};PWD={2}", IpAddress, Uid, Pwd));
            MySqlConnection conn = new MySqlConnection(sqlUrl);

            conn.Open();
            Console.WriteLine("\n解析数据库" + sqlName);
            string sql = string.Format("select table_name from information_schema.tables where table_schema = '{0}'", sqlName);

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string tableName = reader[0].ToString();
                Console.WriteLine(reader[0].ToString());
                if(noNeedTableRole.ContainsKey(tableName))
                    ReadTable(tableName, sqlName);
            }
            conn.Close();
        }

        //读一个数据库的一个表
        public static void ReadTable(string tableName, string sqlName)
        {
            string sqlUrl = "data source=" + IpAddress + ";initial catalog=" + sqlName + ";uid=" + Uid + ";pwd=" + Pwd + ";SslMode=none;";  //连接不是本机的数据库要加  SslMode=none;
                                                                                                                                            // SqlConnection Connection = new SqlConnection(String.Format("Data Source={0};Initial Catalog=master;User ID={1};PWD={2}", IpAddress, Uid, Pwd));
            MySqlConnection conn = new MySqlConnection(sqlUrl);

            conn.Open();
            Console.WriteLine("\n解析数据库：" + sqlName + "的表：" + tableName);
            string sql = string.Format("select COLUMN_NAME from information_schema.columns where table_name='{0}' and table_schema='{1}'", tableName, sqlName);

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            string[] value = new String[0];
            if (noNeedTableRole.ContainsKey(tableName))           
                noNeedTableRole.TryGetValue(tableName, out value);                           
            
            while (reader.Read())
            {
                string roleName = reader[0].ToString();
                if (value != null && value.Length > 0)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i].Equals(roleName))
                            ReadRole(roleName, tableName, sqlName);
                    }
                }

                //ReadRole(reader[0].ToString(), tableName, sqlName);
                //ReadTable(reader[0].ToString());
            }
            conn.Close();
        }

        //读一个表的一列
        public static void ReadRole(string roleName, string tableName, string database)
        {
            //string sqlUrl = string.Format("Database='{0}';Data Source='{1}';User Id='{2}';Password='{3}';charset='utf8';pooling=true;SslMode=none;", database, IpAddress, Uid, Pwd);  //连接不是本机的数据库要加  SslMode=none;
            string sqlUrl = "Database=" + database + ";data source=" + IpAddress + ";uid=" + Uid + ";pwd=" + Pwd + ";SslMode=none;Convert Zero Datetime=True;Allow Zero Datetime=True;";
            //SqlConnection Connection = new SqlConnection(String.Format("Data Source={0};Initial Catalog=master;User ID={1};PWD={2}", IpAddress, Uid, Pwd));
            MySqlConnection conn = new MySqlConnection(sqlUrl);

            conn.Open();
            //Console.WriteLine("\n解析表：" + tableName + "的列：" + roleName);
            string sql = string.Format("select {0} from {1}", roleName, tableName);
            //string sqlUse = string.Format("use {0}", database);
            //MySqlCommand cmdUse = new MySqlCommand(sqlUse, conn);
            //cmdUse.ExecuteReader();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader[0].ToString());
                if (IsCanAdd(reader[0].ToString()))
                {
                    if (_alldic.ContainsKey(reader[0].ToString()))
                    {
                        string str = _alldic[reader[0].ToString()];
                        if (!str.Contains(database + "|" + tableName + "|" + roleName))
                        {
                            _alldic[reader[0].ToString()] = str + "&" + database + "|" + tableName + "|" + roleName;
                        }
                    }
                    else
                        //if(!_alldic.ContainsKey(reader[0].ToString()))
                        _alldic.Add(reader[0].ToString(), database + "|" + tableName + "|" + roleName);
                }
                //ReadTable(reader[0].ToString());
            }
            conn.Close();
        }
        #endregion

        //写入翻译表格文件
        public static void TxtWritePre(string path)
        {
            //文件路径和名字
            string outPutDir = path + "\\翻译.xls";

            FileInfo newFile = new FileInfo(outPutDir);

            //如果已经有此文件，删除重新创建一个
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(outPutDir);
            }

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                //新建一个sheet，名字自己命名
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("翻译");

                //写入元素
                worksheet.Cells[1, 1].Value = "key";
                worksheet.Cells[1, 2].Value = "繁体";
                worksheet.Cells[1, 3].Value = "语言1";
                worksheet.Cells[1, 4].Value = "语言2";
                worksheet.Cells[1, 5].Value = "语言3";

                int i = 0;
                foreach (KeyValuePair<string, string> k in _alldic)
                {
                    worksheet.Cells[i + 2, 1].Value = k.Key;
                    worksheet.Cells[i + 2, filePathIndex].Value = k.Value;
                    i++;
                }

                //保存文件
                package.Save();
            }
        }
        #endregion

        #region 翻译指定文件
        public static void ChangeLimitFile()
        {
            string excelPath = OutPath;

            Console.WriteLine("开始读翻译表");
            ReadEx(excelPath);
            Console.WriteLine("翻译表读取结束\n");

            foreach(string fileName in limitLua)
            {

                _allConent = new List<object>();

                Console.WriteLine(string.Format("解析{0}脚本", fileName));
                ReadLua(fileName);
                Console.WriteLine(string.Format("解析{0}脚本结束\n", fileName));

                Console.WriteLine(string.Format("写入{0}脚本", fileName));
                Write(fileName);
                Console.WriteLine(string.Format("写入{0}脚本结束", fileName));
            }
            foreach (string fileName in limitPrefeb)
            {
                {
                    _allConent = new List<object>();

                    Console.WriteLine(string.Format("解析{0}预设", fileName));
                    ReadPrefabs(fileName);
                    Console.WriteLine(string.Format("解析{0}预设结束\n", fileName));

                    Console.WriteLine(string.Format("写入{0}预设", fileName));
                    WritePrefab(fileName);
                    Console.WriteLine(string.Format("写入{0}预设结束", fileName));
                }
            }
        }
        
        #endregion

        #region 翻译资源替换
        //主方法
        static void ImportLanguage()
        {
            //excel文件路径
            string excelPath = OutPath;

            Console.WriteLine("开始读翻译表");
            ReadEx(excelPath);
            Console.WriteLine("翻译表读取结束\n");

            Console.WriteLine("写入Lua脚本开始");
            ChangeLua();
            Console.WriteLine("写入Lua脚本结束\n");

            Console.WriteLine("写入cs脚本开始");
            ChangeScr();
            Console.WriteLine("写入cs脚本结束\n");

            Console.WriteLine("写入prefab开始");
            ChangePrefab();
            Console.WriteLine("写入prefab结束\n");

            Console.WriteLine("解析数据库开始");
            ChangeSql();
            Console.WriteLine("解析数据库结束\n");

            Console.WriteLine("错误数据");
            WriterError();
            Console.WriteLine("解析错误数据结束\n");
        }

        //翻译lua
        public static void ChangeLua()
        {
            DirectoryInfo dic = new DirectoryInfo(allPath);
            //拿到所有的脚本
            FileInfo[] fileInfos = dic.GetFiles("*.lua.bytes", SearchOption.AllDirectories);

            foreach (FileInfo files in fileInfos)
            {
                _allConent = new List<object>();

                Console.WriteLine(string.Format("解析{0}脚本", files.FullName));
                ReadLua(files.FullName);
                Console.WriteLine(string.Format("解析{0}脚本结束\n", files.FullName));

                Console.WriteLine(string.Format("写入{0}脚本", files.FullName));
                Write(files.FullName);
                Console.WriteLine(string.Format("写入{0}脚本结束", files.FullName));

            }
        }

        //翻译.cs
        public static void ChangeScr()
        {
            DirectoryInfo dic = new DirectoryInfo(allPath);
            //拿到所有的脚本
            FileInfo[] fileInfos = dic.GetFiles("*.cs", SearchOption.AllDirectories);

            foreach (FileInfo files in fileInfos)
            {
                for (int i = 0; i < needChangeScrArr.Length; i++)
                {
                    _allConent = new List<object>();

                    if (needChangeScrArr[i] == files.Name)
                    {
                        Console.WriteLine(string.Format("解析{0}脚本", files.FullName));
                        ReadScr(files.FullName);
                        Console.WriteLine(string.Format("解析{0}脚本结束\n", files.FullName));

                        Console.WriteLine(string.Format("写入{0}脚本", files.FullName));
                        Write(files.FullName);
                        Console.WriteLine(string.Format("写入{0}脚本结束", files.FullName));
                    }
                }
            }
        }

        //翻译prefab
        public static void ChangePrefab()
        {
            DirectoryInfo dic = new DirectoryInfo(allPath);
            //拿到所有的预制体
            FileInfo[] fileInfos = dic.GetFiles("*.prefab", SearchOption.AllDirectories);

            foreach (FileInfo files in fileInfos)
            {
                bool isCan = true;
                for (int i = 0; i < noHavePre.Length; i++)
                {
                    if (files.Name == noHavePre[i])
                    {
                        isCan = false;
                        continue;
                    }
                }

                if (isCan)
                {
                    _allConent = new List<object>();

                    Console.WriteLine(string.Format("解析{0}预设", files.FullName));
                    ReadPrefabs(files.FullName);
                    Console.WriteLine(string.Format("解析{0}预设结束\n", files.FullName));

                    Console.WriteLine(string.Format("写入{0}预设", files.FullName));
                    WritePrefab(files.FullName);
                    Console.WriteLine(string.Format("写入{0}预设结束", files.FullName));
                }
            }
        }

        //读翻译表
        static void ReadEx(string line)
        {
            FileInfo info;
            FileStream stream;
            IExcelDataReader excelReader;

            string path = line + "/翻译.xls";
            Console.WriteLine("filepath://///////////////" + path);

            info = new FileInfo(path);
            stream = info.Open(FileMode.Open, FileAccess.Read);
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            int k = 0;
            do
            {
                while (excelReader.Read())               //一行
                {
                    k++;
                    string ch = excelReader.IsDBNull(0) ? "" : excelReader.GetString(0);
                    string en = excelReader.IsDBNull(index) ? "" : excelReader.GetString(index);

                    string filePath = excelReader.IsDBNull(filePathIndex - 1) ? "" : excelReader.GetString(filePathIndex - 1);
                    if (filePath == "")
                        continue;
                    Console.WriteLine("filepath:" + filePath);
                    Console.WriteLine(filePath);
                    if (filePath.Contains('&'))
                    {
                        string[] strPath = filePath.Split('&');
                        for (int i = 0; i < strPath.Length; i++)
                        {
                            if (strPath[i].Contains('|'))
                            {
                                //0数据库1表2列
                                string[] str = strPath[i].Split('|');

                                List<string[]> tabColumn;
                                //如果存在键 更新
                                if (sqlTabColAll.TryGetValue(str[0], out tabColumn))
                                {
                                    //string[] tabCol = { str[1], str[2] };
                                    string[] tabCol = { str[1], str[2], ch };
                                    tabColumn.Add(tabCol);
                                    sqlTabColAll[str[0]] = tabColumn;
                                }
                                //不存在则添加
                                else
                                {
                                    tabColumn = new List<string[]>();
                                    string[] tabCol = { str[1], str[2], ch };
                                    tabColumn.Add(tabCol);
                                    sqlTabColAll.Add(str[0], tabColumn);
                                }

                            }
                        }
                    }
                    else if(filePath.Contains('|'))
                    {
                        //0数据库1表2列
                        string[] str = filePath.Split('|');

                        List<string[]> tabColumn;
                        //如果存在键 更新
                        if (sqlTabColAll.TryGetValue(str[0], out tabColumn))
                        {
                            //string[] tabCol = { str[1], str[2] };
                            string[] tabCol = { str[1], str[2], ch };
                            tabColumn.Add(tabCol);
                            sqlTabColAll[str[0]] = tabColumn;
                        }
                        //不存在则添加
                        else
                        {
                            tabColumn = new List<string[]>();
                            string[] tabCol = { str[1], str[2], ch };
                            tabColumn.Add(tabCol);
                            sqlTabColAll.Add(str[0], tabColumn);
                        }
                    }

                    if (ch != "" && en != "")
                    {
                        en = DeletSpace(en, ch, k);
                        if (ch.Contains('·'))
                        {
                            string[] st1 = ch.Split('·');
                            string[] st2 = en.Split('·');
                            if (st1.Length != st2.Length)
                            {
                                if (!_langDic.ContainsKey(ch))
                                    _langDic.Add(ch, en);
                                continue;
                            }
                            for (int i = 0; i < st1.Length; i++)
                            {
                                if (!_langDic.ContainsKey(st1[i]))
                                    _langDic.Add(st1[i], st2[i]);
                            }
                        }
                        //else
                        {
                            if (!_langDic.ContainsKey(ch))
                                _langDic.Add(ch, en);
                        }
                    }
                }          
            } while (excelReader.NextResult());
            excelReader.Close();
            stream.Close();
        }

        //读lua
        static void ReadLua(string files)
        {
            //Debug.LogError(Application.dataPath);
            //Debug.LogError(files.Name);
            //File.OpenRead(Application.dataPath + "/Scripts/" + files.Name)
            StreamReader sr = new StreamReader(files);
            string line = "";
            bool isNotes = false;
            while ((line = sr.ReadLine()) != null)
            {
                //删除注释          
                if (line.Trim().StartsWith("--[["))
                {
                    isNotes = !isNotes;
                    if (line.Trim().EndsWith("]]"))
                    {
                        isNotes = !isNotes;
                    }
                    _allConent.Add(line);
                    continue;
                }
                if (line.Trim().EndsWith("]]"))
                {
                    isNotes = !isNotes;
                    _allConent.Add(line);
                    continue;
                }
                if (isNotes)
                {
                    _allConent.Add(line);
                    continue;
                }
                if (line.Trim().StartsWith("--"))
                {
                    _allConent.Add(line);
                    continue;
                }
                //删除注释结束
                if (IsStartWithLog(line.Trim()))
                {
                    _allConent.Add(line);
                    continue;
                }

                int num = FindWord(line, 0, "--");
                string lineEnd = "";
                if (num >= 0)
                {
                    lineEnd = line.Substring(num);
                    line = line.Substring(0, num);
                }
                //Debug.Log(line);
                string arr ="";
                if (line.Contains("\""))
                {
                    arr = ReadLine2(line);
                }
                else if (line.Contains("'"))
                {
                    arr = ReadLine(line);
                }
                //foreach (string st in arr)
                {
                    if (_langDic.ContainsKey(arr) && arr != "")
                    {
                        //更换脚本内容
                        if(arr.Contains('·'))
                        {
                            string[] str = arr.Split('·');
                            for(int i = 0; i < str.Length; i++)
                            {
                                line = ReplaceLine(line, str[i]);
                                Console.WriteLine("line : "+line+"  str:"+str[i]);
                            }
                        }
                        else 
                            line = ReplaceLine(line, arr);
                    }
                }
                _allConent.Add(line + lineEnd);
            }
            sr.Close();
            sr.Dispose();
        }

        //读cs
        static void ReadScr(string files)
        {
            //Debug.LogError(Application.dataPath);
            //Debug.LogError(files.Name);
            //File.OpenRead(Application.dataPath + "/Scripts/" + files.Name)
            StreamReader sr = new StreamReader(files);
            string line = "";
            bool isNotes = false;
            while ((line = sr.ReadLine()) != null)
            {
                //删除注释          
                if (line.Trim().StartsWith("/*"))
                {
                    isNotes = !isNotes;
                    if (line.Trim().EndsWith("*/"))
                    {
                        isNotes = !isNotes;
                    }
                    _allConent.Add(line);
                    continue;
                }
                if (line.Trim().EndsWith("*/"))
                {
                    isNotes = !isNotes;
                    _allConent.Add(line);
                    continue;
                }
                if (isNotes)
                {
                    _allConent.Add(line);
                    continue;
                }
                if (line.Trim().StartsWith("//"))
                {
                    _allConent.Add(line);
                    continue;
                }
                //删除注释结束
                if (IsStartWithLog(line.Trim()))
                {
                    _allConent.Add(line);
                    continue;
                }

                int num = FindWord(line, 0, "//");
                string lineEnd = "";
                if (num >= 0)
                {
                    lineEnd = line.Substring(num);
                    line = line.Substring(0, num);
                }
                //Debug.Log(line);
                string arr="";
                if (line.Contains("\""))
                {
                    arr = ReadLine2(line);
                }
                else if (line.Contains("'"))
                {
                    arr = ReadLine(line);
                }
                //foreach (string st in arr)
                {
                    if (_langDic.ContainsKey(arr) && arr != "")
                    {
                        if (_langDic.ContainsKey(arr) && arr != "")
                        {
                            //更换脚本内容
                            if (arr.Contains('·'))
                            {
                                string[] str = arr.Split('·');
                                for (int i = 0; i < str.Length; i++)
                                {
                                    line = ReplaceLine(line, str[i]);
                                }
                            }
                            else
                                line = ReplaceLine(line, arr);
                        }
                    }
                }
                _allConent.Add(line + lineEnd);
            }
            sr.Close();
            sr.Dispose();
        }


        //写脚本
        static void Write(string files)
        {
            //Application.dataPath + "/Scripts/" + files.Name;
            string path = files;
            StreamWriter sw = new StreamWriter(File.Open(path, FileMode.Create) , Encoding.UTF8);
            for (int i = 0; i < _allConent.Count; i++)
            {
                string line = (string)_allConent[i];

                sw.WriteLine(line);
            }
            sw.Close();
            sw.Dispose();
        }

        //读预制体
        public static void ReadPrefabs(string file)
        {
            StreamReader sr = new StreamReader(File.OpenRead(file));          //Application.dataPath + "/lua/" + file.Name 
            StringBuilder sb;
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                //line = line.Replace("\n", @"\n");
                //line = line.Replace("\r", @"\r");

                //循环刷新
                sb = new StringBuilder();
                //找到一个"变一次相反
                bool isBegin = false;
                //m_Text代表Text组件
                //if (line.Trim().StartsWith("m_Text:"))
                if (line.Contains("\"") && line.Contains("\\u"))
                {
                    for (int i = 0; i < line.Length; i++)
                    {

                        if (line[i] == '"')
                        {
                            isBegin = !isBegin;
                            continue;
                        }
                        if (isBegin)
                            sb.Append(line[i]);
                    }

                    if (_langDic.ContainsKey(Unicode2String(sb.ToString())) && !string.IsNullOrEmpty(sb.ToString()))
                    {
                        _allConent.Add(ReplaceLine(line, Unicode2String(sb.ToString()), true));
                    }
                    else
                        _allConent.Add(line);
                }
                else
                    _allConent.Add(line);
            }
            sr.Close();
            sr.Dispose();
        }

        //写预制体文本
        static void WritePrefab(string file)
        {
            string path = file;
            StreamWriter sw = new StreamWriter(File.Open(path, FileMode.Create));
            for (int i = 0; i < _allConent.Count; i++)
            {
                string line = (string)_allConent[i];
                //line = line.Replace(@"\n", "\n");
                //line = line.Replace(@"\r", "\r");
                sw.WriteLine(line);
            }
            sw.Close();
            sw.Dispose();
        }

        //读数据库
        static void ChangeSql()
        {
            List<string> resultList = new List<string>();
            foreach (string key in sqlTabColAll.Keys)
            {
                Console.WriteLine("打开数据库："+key);
                MysqlHelp sqlHelp = new MysqlHelp(key, IpAddress, Uid, Pwd);

                List<string[]> value;
                sqlTabColAll.TryGetValue(key, out value);
                if (value != null)
                {
                    foreach (var stArr in value)
                    {
                        //foreach (var name in _langDic)
                        //{
                        //    //if (_langDic.ContainsKey(name.Key))
                        //    {
                        //        Console.WriteLine("表名：" + stArr[0] + "列名：" + stArr[1]);

                        //        string changeValue;
                        //        _langDic.TryGetValue(name.Key, out changeValue);
                        //        Console.WriteLine("name：" + name.Key + "列名：" + changeValue);
                        //        string sql1 = "UPDATE {0} SET {1} = \"{2}\" WHERE {3} = \"{4}\"";
                        //        sqlHelp.ChangeQuery(sql1, stArr[0], stArr[1], name.Key, changeValue);
                        //    }
                        //}
                        if (_langDic.ContainsKey(stArr[2]))
                        {
                            Console.WriteLine("表名：" + stArr[0] + "列名：" + stArr[1]);

                            string changeValue;
                            _langDic.TryGetValue(stArr[2], out changeValue);
                            string oldStr = StrForChange(stArr[2]);
                            string newStr = StrForChange(changeValue);
                            Console.WriteLine("name：" + stArr[2] + "列名：" + changeValue);
                            string sql1 = "UPDATE {0} SET {1} = \'{2}\' WHERE {3} = \'{4}\'";
                            sqlHelp.ChangeQuery(sql1, stArr[0], stArr[1], oldStr, newStr);
                        }

                    }
                }
                sqlHelp.close();
            }
        }

        //符号不一致的行数
        public static void WriterError()
        {
            Console.WriteLine("[]符号缺少的行数：\n");
            foreach (int num in strError1)
            {
                Console.WriteLine(string.Format("第{0}行\n",num));
            }
            Console.WriteLine("<>符号缺少的行数：\n");
            foreach (int num in strError2)
            {
                Console.WriteLine(string.Format("第{0}行\n", num));
            }
            Console.WriteLine("{}符号缺少的行数：\n");
            foreach (int num in strError3)
            {
                Console.WriteLine(string.Format("第{0}行\n", num));
            }
            Console.WriteLine("缺少翻译标识符：\n");
            foreach (int num in strError4)
            {
                Console.WriteLine(string.Format("第{0}行\n", num));
            }
        }
        #endregion

        #region  方法
        //转义字符串的转换
        public static string StrForChange(string str)
        {
            StringBuilder sb = new StringBuilder();
            for(int i= 0; i < str.Length; i++)
            {
                if (str[i] == '\"' || str[i] == '\'' || str[i] == '\\')           
                    sb.Append('\\');
                sb.Append(str[i]);
            }
            return sb.ToString();
        }

        //中文转换为Unicode编码
        public static string String2Unicode(string source)
        {
            source = source.Replace(@"\n","\n");

            byte[] bytes = Encoding.Unicode.GetBytes(source);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        //Unicode编码转换为中文
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        //这个值是否可以加到字典里
        public static bool IsCanAdd(string word)
        {
            if (_alldic.ContainsKey(word))
            {
                tttt += 1;
            }
            //if (_alldic.ContainsKey(word) || string.IsNullOrEmpty(word) || !IsLineNeed(word))
            //    return false;
            if (string.IsNullOrEmpty(word) || !IsLineNeed(word))
                return false;
            return true;
        }

        //是否包含汉字
        public static bool IsLineNeed(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if ((line[i] >= 0x4e00 && line[i] <= 0x9fbb))
                {
                    return true;
                }
            }
            return false;
        }

        public static int FindWord(string line, int offset, string word)     //拿到line这一行“word”为止的字符长度
        {
            int idx = -1;

            offset = Math.Max(0, offset);
            bool isChars = false;
            if (line != null && line.Length >= 2)
            {
                for (int i = offset; i < line.Length; ++i)
                {
                    if (line[i] == '\"' && (i == 0 || line[i - 1] != '\\')) isChars = !isChars;        //引号干扰

                    if (isChars) continue;

                    if (string.Compare(line, i, word, 0, word.Length) == 0)
                    {
                        idx = i;
                        break;
                    }
                }
            }
            return idx;
        }

        //交换俩个字符串
        public static string ReplaceLine(string line, string word, bool isPrefab = false)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string value = "";
            _langDic.TryGetValue(word, out value);

            if (!dic.ContainsKey(word))
            {
                dic.Add(word, value);
            }
            //Console.WriteLine("value : " + value);
            //如果是预设
            if (isPrefab)
            {
                if (line.Contains("\""))
                {
                    bool isBegin = false;
                    StringBuilder testWord = new StringBuilder();
                    for (int i = 0; i < line.Length; i++)
                    {
                        //在单引号''里搜索
                        if ((line[i] == '\"' && (i == 0 || line[i - 1] != '\\')))
                        {
                            isBegin = !isBegin;
                            if (isBegin)
                            {
                                sb.Append(line[i]);
                            }
                            //单引号部分结束
                            else if (isBegin == false)
                            {
                                //如果文字部分为空，引号添加
                                if (testWord.ToString() == "")
                                {
                                    sb.Append(line[i]);
                                    continue;
                                }
                                //不为空，交换
                                if (dic.ContainsKey(Unicode2String(testWord.ToString())))
                                {
                                    string testWordValue = "";
                                    dic.TryGetValue(Unicode2String(testWord.ToString()), out testWordValue);
                                    sb.Append(String2Unicode(testWordValue));
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                                else
                                {
                                    sb.Append(testWord.ToString());
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                            }
                            continue;
                        }
                        if (isBegin == false)
                        {
                            sb.Append(line[i]);
                            continue;
                        }

                        testWord.Append(line[i]);
                    }
                }
                else
                {
                    bool isBegin = false;
                    StringBuilder testWord = new StringBuilder();
                    for (int i = 0; i < line.Length; i++)
                    {
                        //在单引号''里搜索
                        if (line[i] == '\'')
                        {
                            isBegin = !isBegin;
                            if (isBegin)
                            {
                                sb.Append(line[i]);
                            }
                            //单引号部分结束
                            else if (isBegin == false)
                            {
                                //如果文字部分为空，引号添加
                                if (testWord.ToString() == "")
                                {
                                    sb.Append(line[i]);
                                    continue;
                                }
                                //不为空，交换
                                if (dic.ContainsKey(Unicode2String(testWord.ToString())))
                                {
                                    string testWordValue = "";
                                    dic.TryGetValue(Unicode2String(testWord.ToString()), out testWordValue);
                                    sb.Append(String2Unicode(testWordValue));
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                                else
                                {
                                    sb.Append(testWord.ToString());
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                            }
                            continue;
                        }
                        if (isBegin == false)
                        {
                            sb.Append(line[i]);
                            continue;
                        }

                        testWord.Append(line[i]);
                    }
                }
            }
            else
            {
                if (line.Contains("\""))
                {
                    bool isBegin = false;
                    StringBuilder testWord = new StringBuilder();
                    for (int i = 0; i < line.Length; i++)
                    {
                        //在单引号''里搜索
                        if ((line[i] == '\"' && (i == 0 || line[i - 1] != '\\')))
                        {
                            isBegin = !isBegin;
                            if (isBegin)
                            {
                                sb.Append(line[i]);
                            }
                            //单引号部分结束
                            else if (isBegin == false)
                            {
                                //如果文字部分为空，引号添加
                                if (testWord.ToString() == "")
                                {
                                    sb.Append(line[i]);
                                    continue;
                                }
                                //不为空，交换
                                if (dic.ContainsKey(testWord.ToString()))
                                {
                                    string testWordValue = "";
                                    dic.TryGetValue(testWord.ToString(), out testWordValue);
                                    sb.Append(testWordValue);
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                                else
                                {
                                    sb.Append(testWord.ToString());
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                            }
                            continue;
                        }
                        if (isBegin == false)
                        {
                            sb.Append(line[i]);
                            continue;
                        }

                        testWord.Append(line[i]);
                    }
                }
                else
                {
                    bool isBegin = false;
                    StringBuilder testWord = new StringBuilder();
                    for (int i = 0; i < line.Length; i++)
                    {
                        //在单引号''里搜索
                        if (line[i] == '\'')
                        {
                            isBegin = !isBegin;
                            if (isBegin)
                            {
                                sb.Append(line[i]);
                            }
                            //单引号部分结束
                            else if (isBegin == false)
                            {
                                //如果文字部分为空，引号添加
                                if (testWord.ToString() == "")
                                {
                                    sb.Append(line[i]);
                                    continue;
                                }
                                //不为空，交换
                                if (dic.ContainsKey(testWord.ToString()))
                                {
                                    string testWordValue = "";
                                    dic.TryGetValue(testWord.ToString(), out testWordValue);
                                    sb.Append(testWordValue);
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                                else
                                {
                                    sb.Append(testWord.ToString());
                                    sb.Append(line[i]);
                                    testWord = new StringBuilder();
                                }
                            }
                            continue;
                        }
                        if (isBegin == false)
                        {
                            sb.Append(line[i]);
                            continue;
                        }

                        testWord.Append(line[i]);
                    }
                }
            }

            return sb.ToString();
        }

        public static bool IsStartWithLog(string line)
        {
            for(int i = 0; i < noHaveWord.Length; i++)
            {
                if (line.Trim().StartsWith(noHaveWord[i]))
                    return true;
            }
            return false;
        }

        //判断一段字的是重复没用的
        public static bool IsLineWordLoop(string line)
        {
            line = line.Trim();

            //重复次数
            int times = 0;

            List<string> stArr = new List<string>();
            for (int i = 0; i < line.Length; i++)
            {
                for (int j = 1; j < line.Length / 2 && (i+j)<line.Length; j++)
                {
                    if(!stArr.Contains(line.Substring(i,j)))
                        stArr.Add(line.Substring(i, j));
                }
            }

            foreach (string st in stArr)
            {
                StringBuilder sb = new StringBuilder();
                times = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (i % st.Length == 0)
                    {
                        if (st == sb.ToString())
                            times += 1;
                        sb = new StringBuilder();
                    }
                    if ((line[i] >= 0x4e00 && line[i] <= 0x9fbb))
                        sb.Append(line[i]);
                }
                if (times >= 3)
                    return true;
            }
            return false;           
        }

        //翻译文件转义符中间的空格问题
        public static string DeletSpace(string line , string changeLine, int num)
        {
            int sc1 = 0;  //[的个数
            int sc2 = 0; //]的个数

            int sc3 = 0; //<的个数
            int sc4 = 0; //>的个数

            int sc5 = 0; //{的个数
            int sc6 = 0; //}的个数

            int sc7 = 0; //·的个数
        
            for (int i = 0; i < changeLine.Length; i++)
            {         
                switch (changeLine[i])
                {
                    case '[':
                        sc1++;
                        break;
                    case ']':
                        sc2++;
                        break;
                    case '<':
                        sc3++;
                        break;
                    case '>':
                        sc4++;
                        break;
                    case '{':
                        sc5++;
                        break;
                    case '}':
                        sc6++;
                        break;
                    case '·':
                        sc7++;
                        break;
                    default:
                        break;
                }
            }

            int s1 = 0;  //[的个数
            int s2 = 0; //]的个数

            int s3 = 0; //<的个数
            int s4 = 0; //>的个数

            int s5 = 0; //{的个数
            int s6 = 0; //}的个数

            int s7 = 0;
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < line.Length; i++)
            {
                if (i>0 && line[i] == ' ' && line[i-1] == '\\')
                    continue;
                sb.Append(line[i]);
                switch (line[i])
                {
                    case '[':
                        s1++;
                        break;
                    case ']':
                        s2++;
                        break;
                    case '<':
                        s3++;
                        break;
                    case '>':
                        s4++;
                        break;
                    case '{':
                        s5++;
                        break;
                    case '}':
                        s6++;
                        break;
                    case '·':
                        s7++;
                        break;
                    default:
                        break;
                }             
            }
            if (s1 != s2 && (sc1!=s1||sc2 !=s2))
                strError1.Add(num);
            if (s3 != s4 && (sc3!=s3||sc4 != s4))
                strError2.Add(num);
            if (s5 != s6 && (sc5!=s5||sc6 !=s6))
                strError3.Add(num);
            if (s7 != sc6)
                strError4.Add(num);
            return sb.ToString();
        }

      

        #endregion
    }


}
class MysqlHelp
{
    private MySqlConnection conn;

    public MysqlHelp(string sqlName, string host, string user, string pwd)
    {
        //string sqlUrl = string.Format("Database='{0}';Data Source='{1}';User Id='{2}';Password='{3}';charset='utf8';pooling=true;SslMode=none;", db, host,user,pwd);
        string sqlUrl = String.Format("Database={0};data source={1};uid={2};pwd={3};SslMode=none;Convert Zero Datetime=True;Allow Zero Datetime=True;Charset=utf8", sqlName, host, user, pwd);
        this.conn = new MySqlConnection(sqlUrl);
        this.conn.Open();
    }

    public List<string> Query(string sql)
    {
        List<string> list = new List<string>();

        MySqlCommand cmd = new MySqlCommand(sql, conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine(reader[0].ToString());
            //ReadTable(reader[0].ToString(), sqlName);
            list.Add(reader[0].ToString());
        }
        cmd.Dispose();
        reader.Close();
        return list;
    }

    public void ChangeQuery(string sql1, string tableName, string RoleName, string value, string newVlaue)
    {
        //UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'
        
        //string sql = string.Format(sql1, tableName, RoleName, newVlaue, RoleName, "你好");
        //Console.WriteLine("sql1:" + sql1);
        MySqlCommand cmd = new MySqlCommand(string.Format(sql1, tableName, RoleName, newVlaue, RoleName, value), conn);  
        int num = cmd.ExecuteNonQuery();
        //Console.WriteLine(num); 
        cmd.Dispose();
        //Console.WriteLine(n);
    }

    //public void Query(string sql)
    //{
    //    MySqlCommand cmd = new MySqlCommand(sql, this.conn);
    //    MySqlDataReader reader = cmd.ExecuteReader();
    //    while (reader.Read())
    //    {
    //        List<string> line = new List<string>();
    //        for (int i = 0; i < reader.FieldCount; i++)
    //        {
    //            line.Add(reader[i].ToString());
    //            Console.WriteLine(reader[i].ToString());
    //        }
    //        //ReadTable(reader[0].ToString());
    //    }
    //    reader.Close();
    //}

    public List<List<string>> query(string sql)
    {
        List<List<string>> data = new List<List<string>>();
        MySqlCommand cmd = new MySqlCommand(sql, this.conn);

        MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            List<string> line = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                line.Add(reader[i].ToString());
                Console.WriteLine(reader[i].ToString());
            }
            data.Add(line);


            //ReadTable(reader[0].ToString());
        }
        reader.Close();
        return data;
    }
    public void close()
    {
        this.conn.Close();
    }

}

