using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LenovoCodeGenerate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        /***
 * ○ 生成类文件整体思路：定模板，设置替换点，遍历DBInfo替换，输出cs文件。类文件模板是根据自己项目代码实践而定。
 * ○ 具体算法：
 *     1、读取数据库中所有表及字段，返回DBInfo（Dictionary<{表,说明},Dictionary<{字段,说明},数据类型>>）
 *     2、遍历DBInfo，生成Entities层代码
 *     3、遍历DBInfo，生成Rules层代码
 *     4、遍历DBInfo，生成IBusiness层代码
 *     5、遍历DBInfo，生成Business层代码
 *     6、遍历DBInfo，生成Proxy层代码
 *     7、遍历DBInfo，生成Presenters层代码
 *     8、遍历DBInfo，生成I--View层代码
 *     9、遍历DBInfo，生成ViewModel层代码
 *     9、遍历DBInfo，生成RuleFactory,BaseRule,Presenter,RuleFactory代码
 ***/

        /**********************此生成器只生成最基本的增删改查方法，如需另外增加方法，请根据自己实际业务场景自行添加************************/

        #region 生成多层代码

        #region 全局变量
        static string DirSolutionPath = "";
        static string SolutionAuth = "";
        static string SolutionName = "Lenovo.CIS.Consultation"; 
        static string ProxyBaseName = "ProxyConsultationBase" ;//你当前业务模块的代理业务的基类
        static string HisModelNameSpace = "Lenovo.HIS.Entities";
        static string HisCommonNameSpace = "Lenovo.HIS.Common";
        static string HiscUtilsNameSpace = "Lenovo.HIS.cUtils";

        static string RuleNameSpace = SolutionName + ".sDBRule";//DAL层命名空间（下同）
        static string EntitiesNameSpace = SolutionName + ".Entities";
        static string IBllNameSpace = SolutionName + ".IBusiness";
        static string sBllNameSpace = SolutionName + ".sBusiness";
        static string cBllNameSpace = SolutionName + ".cBusiness";

        static string sRuleLayerPath        = Path.Combine(DirSolutionPath, RuleNameSpace );//dal层代码生成代码文件存放路径（下同）
        static string ModelLayerPath        = Path.Combine(DirSolutionPath, EntitiesNameSpace );
        static string BllLayerPath          = Path.Combine(DirSolutionPath, sBllNameSpace);
        static string IBllLayerPath         = Path.Combine(DirSolutionPath, IBllNameSpace );
        static string cProxyLayerPath       = Path.Combine(DirSolutionPath, cBllNameSpace , "FacadeProxy");
        static string cPresenterLayerPath   = Path.Combine(DirSolutionPath, cBllNameSpace , "Presenters");
        static string cViewLayerPath        = Path.Combine(DirSolutionPath, cBllNameSpace , "View");
        static string cViewModelLayerPath   = Path.Combine(DirSolutionPath, cBllNameSpace , "ViewModels");
        static char DicSplit = '≡';//分隔符，注意：代码不是因此出错，建议不要修改 
        #endregion

        #region 得到数据库中所有表及字段

        private  Dictionary<string, Dictionary<string, string>> GetDBInfo()
        {
            //Dictionary<{表,说明},Dictionary<{字段,说明},数据类型>>
            Dictionary<string, Dictionary<string, string>> dicR = new Dictionary<string, Dictionary<string, string>>();
            string getTables = " SELECT name FROM sysobjects  WHERE xtype = 'U' ";

            DataTable dt = DbHelperSQL.GetDataSet(getTables).Tables[0];
            foreach (DataRow item in dt.Rows)
            {
                string tblName = item[0].ToString();
                //"SELECT COLUMN_NAME,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + tblName+"' ";
                string getTblFields = @"SELECT 
                    表名       = case when a.colorder=1 then d.name else '' end,
                    表说明     = case when a.colorder=1 then isnull(f.value,'') else '' end,
                    字段序号   = a.colorder,
                    字段名     = a.name,
                    标识       = case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then '√'else '' end,
                    主键       = case when exists(SELECT 1 FROM sysobjects where xtype='PK' and parent_obj=a.id and name in (
                                     SELECT name FROM sysindexes WHERE indid in( SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid))) then '√' else '' end,
                    类型       = b.name,
                    占用字节数 = a.length,
                    长度       = COLUMNPROPERTY(a.id,a.name,'PRECISION'),
                    小数位数   = isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0),
                    允许空     = case when a.isnullable=1 then '√'else '' end,
                    默认值     = isnull(e.text,''),
                    字段说明   = isnull(g.[value],'')
                FROM 
                    syscolumns a
                left join 
                    systypes b 
                on 
                    a.xusertype=b.xusertype
                inner join 
                    sysobjects d 
                on 
                    a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties'
                left join 
                    syscomments e 
                on 
                    a.cdefault=e.id
                left join 
                sys.extended_properties   g 
                on 
                    a.id=G.major_id and a.colid=g.minor_id  
                left join
                sys.extended_properties f
                on 
                    d.id=f.major_id and f.minor_id=0
                where   d.name='" + tblName + "'   order by  a.id,a.colorder";
                DataTable dtTbl = DbHelperSQL.GetDataSet(getTblFields).Tables[0]; //DbHelperSQL.Query(getTblFields).Tables[0];
                Dictionary<string, string> dicItem = new Dictionary<string, string>();
                foreach (DataRow tbl in dtTbl.Rows)
                {
                    if (tbl[1].ToString() != "")
                        tblName += DicSplit + tbl[1].ToString();
                    string COLUMN_NAME = tbl[3].ToString() + DicSplit + tbl[12].ToString();
                    string DATA_TYPE = tbl[6].ToString();
                    dicItem.Add(COLUMN_NAME, DATA_TYPE);
                }
                dicR.Add(tblName, dicItem);
            }
            return dicR;
        }

        private  Dictionary<string, Dictionary<string, string>> GetOracleBInfo()
        {
            OracleSqlHelper oracleSqlHelper = new OracleSqlHelper();
            Dictionary<string, Dictionary<string, string>> dicR = new Dictionary<string, Dictionary<string, string>>(); 
            DataTable dt =oracleSqlHelper.GetTableList(this.txtTabLike1.Text.ToString(), this.txtTabLike2.Text.ToString());
            foreach (DataRow item in dt.Rows)
            {
                string tblName = item["TABLE_NAME"].ToString(); 

                DataTable dtTbl = oracleSqlHelper.GetTabType(tblName); //DbHelperSQL.Query(getTblFields).Tables[0];
                Dictionary<string, string> dicItem = new Dictionary<string, string>();
                foreach (DataRow tbl in dtTbl.Rows)
                {
                    //if (item["COMMENTS"].ToString() != "")
                    //    tblName += DicSplit + item["COMMENTS"].ToString().SplitSpecialChar();
                    string COLUMN_NAME = tbl["COLUMN_NAME"].ToString() + DicSplit + tbl["COMMENTS"].ToString().SplitSpecialChar();
                    string DATA_TYPE = tbl["DATA_TYPE"].ToString();
                    dicItem.Add(COLUMN_NAME, DATA_TYPE);
                }
                tblName = tblName + DicSplit + item["COMMENTS"].ToString().SplitSpecialChar() + DicSplit + item["PKCOLUMN"].ToString();
                dicR.Add(tblName, dicItem);
            }
            return dicR;
        }
        #endregion

        #region 遍历生成 Entity        层代码
        private static void EntityFactory(Dictionary<string, Dictionary<string, string>> dic)
        {
            foreach (var item in dic)
            {
                #region 类模板
                StringBuilder sb = new StringBuilder();
                sb.Append("  /**************************************************  \r\n");
                sb.Append("   ** Company  ：  联想智慧医疗                        \r\n");
                sb.Append("   ** ClassName：  【表】                                \r\n");
                sb.Append("   ** Ver      ：  V1.0.0.0                            \r\n");
                sb.Append("   ** Desc     ：  【表职责】                           \r\n");
                sb.Append("   ** auth     ：  【开发者名字】                              \r\n");
                sb.Append("   ** date     ： 【时间戳】                           \r\n");
                sb.Append("  **************************************************/  \r\n");
                sb.Append("  using System;                                        \r\n");
                sb.Append("  using System.Text;                                   \r\n");
                sb.Append("  using 【HIS公共命名空间】;                             \r\n");
                sb.Append("  using System.Collections.Generic;                    \r\n");
                sb.Append("  using System.Linq;                                   \r\n");
                sb.Append("  using System.Runtime.Serialization;                  \r\n");
                sb.Append("                                                       \r\n");
                sb.Append("  namespace 【命名空间】                               \r\n");
                sb.Append("  {                                                    \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        ///【表职责】                                       \r\n"); 
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        [Serializable]                                    \r\n ");
                sb.Append("        [DataContract]                                    \r\n ");
                sb.Append("        public partial class 【类名称】                   \r\n ");
                sb.Append("        {                                                \r\n");
                sb.Append("                                                       \r\n");
                sb.Append("            public 【类名称】()                              \r\n");
                sb.Append("            {                                           \r\n ");
                sb.Append("            }                                           \r\n ");
                sb.Append("         【属性部分】                                  \r\n ");
                sb.Append("     }                                                \r\n ");
                sb.Append("  }                                                   \r\n ");

                #endregion

                #region 属性部分
                StringBuilder propPart = new StringBuilder();
              
                foreach (var field in item.Value)
                {
                    string[] key = field.Key.Split(DicSplit);
                    string type = ChangeToCSharpType(field.Value.ToString());//Dictionary<{表,说明},Dictionary<{字段,说明},数据类型>>
                    string tfName = key[0];
                    string fName = key[0];
                    string fRemark = key.Length == 2 ? key[1] : "";
                    //string first = field.Key.Substring(0, 1);//第一个字母
                    //fName = fName.Substring(1, fName.Length - 1);//不含第一个字母
                    //string _f = first.ToLower() + fName;

                    string pF = fName.SplitUnderLine();
                    propPart.Append("                                                 \r\n");
                    propPart.Append("        ///<summary>                             \r\n");
              propPart.AppendFormat("        ///{0}                                   \r\n", fRemark);
                    propPart.Append("        ///<summary>                             \r\n");
                    propPart.Append("        [DataMember]                             \r\n");
              propPart.AppendFormat("        [DataField(\"{0}\")]                     \r\n", tfName);
              propPart.AppendFormat("        public {0} {1}                         \r\n", type, pF);
                    propPart.Append("        {                                        \r\n");
                    propPart.Append("              get;                               \r\n");
                    propPart.Append("              set;                               \r\n");
                    propPart.Append("        }                                        \r\n");
                }
                #endregion

                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                        .Replace("【类名称】", tblName.SplitUnderLine())
                        .Replace("【时间戳】", DateTime.Now.ToString())
                        .Replace("【命名空间】", EntitiesNameSpace)
                        .Replace("【表】", tblName + "表实体类")
                        .Replace("【表职责】", tblWork.SplitSpecialChar())
                        .Replace("【属性部分】", propPart.ToString())
                        .Replace("【开发者名字】",SolutionAuth  )
                        .Replace("【HIS公共命名空间】", HisCommonNameSpace);
                CreateTxt( Path.Combine(ModelLayerPath , tblName.SplitUnderLine() + ".cs"), ModelLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 Rule          层代码
        private static void RuleFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                string tblName = item.Key.Split(DicSplit)[0];
                string keyName = item.Key.Split(DicSplit)[2];
                string tblWork = item.Key.Split(DicSplit)[1];
                StringBuilder sb = new StringBuilder();
                #region 类模板
                sb.Append("/**************************************************                                  \r\n  ");
                sb.Append(" ** ClassName ： 【类名称】                                                          \r\n  ");
                sb.Append(" ** Ver       ：  V1.0.0.0                                                           \r\n  ");
                sb.Append(" ** Desc      ：  用于【表】数据持久化                                               \r\n  ");
                sb.Append(" ** auth      ：  【开发者名字】                                                             \r\n  ");
                sb.Append(" ** date      ： 【时间戳】                                                          \r\n  ");
                sb.Append("****************************************************/                                \r\n  ");
                sb.Append("using System.Collections.Generic;                                                    \r\n  ");
                sb.Append("using System.Text;                                                                   \r\n  ");
                sb.AppendFormat("using {0};                                                                           \r\n  ", EntitiesNameSpace);
                sb.AppendFormat("using {0};                                                                           \r\n  ", HisModelNameSpace);
                sb.Append("                                                                                     \r\n  ");
                sb.Append("namespace 【命名空间】                                                               \r\n  ");
                sb.Append("{                                                                                    \r\n  ");
                sb.AppendFormat("    public class 【业务分类】ConcreteRule : 【业务分类】Rule                             \r\n  ");
                sb.Append("    {                                                                                \r\n  ");
                sb.AppendFormat("        public 【业务分类】ConcreteRule(EnumDBType _EnumDBType): base(_EnumDBType)     \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("    }                                                                                \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    ///【表职责】                                                                    \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    public class 【业务分类】Rule                                                      \r\n  ");
                sb.Append("    {                                                                                \r\n  ");
                sb.Append("        public static EnumDBType m_DBType = EnumDBType.Oracle;                       \r\n  ");
                sb.Append("        public 【业务分类】Rule(EnumDBType dbType)                                     \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            m_DBType = dbType;                                                       \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.AppendFormat("        #region {0} FieldsList                                                       \r\n  ", tblName);
                sb.AppendFormat("        const string m_tbFields = @{0};                                              \r\n  ", GetFieldsList(tblName, item.Value));
                sb.AppendFormat("        #endregion FieldsList                                                        \r\n  ");
                sb.Append("        #region  select by Condition                                                 \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 返回查询SQL语句                                                          \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        public string GetByCondition【业务分类】List(【类名称】 model)               \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            string _sql = string.Format(\"SELECT {0} FROM 【表】 where 1=1 \",m_tbFields); \r\n  ");
                sb.Append("            return _sql;                                                             \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("        #endregion                                                                   \r\n  ");
                sb.Append("        #region select All                                                           \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 返回查询所有SQL语句                                                      \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        public string GetAll【业务分类】List()                                       \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            string _sql = string.Format(\"SELECT {0} FROM 【表】  \",m_tbFields);    \r\n  ");
                sb.Append("            return _sql;                                                             \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("        #endregion                                                                   \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        #region delete                                                               \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 返回删除SQL语句                                                          \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        public List<string> Delete【业务分类】 (List<【类名称】> models)             \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            【当前表Delete】                                                         \r\n  ");
                sb.Append("            return _sqlList;                                                         \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("        #endregion                                                                   \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        #region insert                                                               \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 返回新增SQL语句                                                          \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        public List<string> Insert【业务分类】(List<【类名称】> models)              \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            【当前表Insert】                                                         \r\n  ");
                sb.Append("            return _sqlList;                                                         \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("        #endregion                                                                   \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        #region update                                                               \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 返回更新SQL语句                                                          \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        public List<string> Update【业务分类】(List<【类名称】> models)              \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            【当前表Update】                                                         \r\n  ");
                sb.Append("            return _sqlList;                                                         \r\n  ");
                sb.Append("        }                                                                            \r\n  ");
                sb.Append("        #endregion                                                                   \r\n  ");
                sb.Append("    }                                                                                \r\n  ");
                sb.Append("}                                                                                    \r\n  ");
                #endregion
                string insertSQL = GetInsertSQL(tblName, item.Value);
                string updateSQL = GetUpdateSQL(tblName, item.Value, keyName);
                string deleteSQL = GetDeleteSQL(tblName, item.Value, keyName);
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", RuleNameSpace)
                            .Replace("【开发者名字】", SolutionAuth)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【当前表Delete】", deleteSQL)
                            .Replace("【当前表Insert】", insertSQL)
                            .Replace("【当前表Update】", updateSQL);
                CreateTxt(Path.Combine(sRuleLayerPath , tblName.SplitFirstUnderLine() + "Rule.cs"), sRuleLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 IBusiness     层代码

        private static void IBusinessFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region 类模板
                sb.Append("/**************************************************            						\r\n  ");
                sb.Append("** ClassName ： I【业务分类】Business                                     						\r\n  ");
                sb.Append("** Ver       ： V1.0.0.0                                       						\r\n  ");
                sb.Append("** Desc      ： 用于【表】业务接口约束定义                     						\r\n  ");
                sb.Append("** auth      ： 【开发者名字】                                         						\r\n  ");
                sb.Append("** Desc      ： 【表职责】                                     						\r\n  ");
                sb.Append("** date      ： 【时间戳】                                     						\r\n  ");
                sb.Append("****************************************************/          						\r\n  ");
                sb.Append("using 【实体类命名空间】;                                      						\r\n  ");
                sb.Append("using System.Collections.Generic;                              						\r\n  ");
                sb.Append("using System.Linq;                                             						\r\n  ");
                sb.Append("using System.Text;                                             						\r\n  ");
                sb.Append("using System.ServiceModel;                                     						\r\n  ");
                sb.Append("                                                               						\r\n  ");
                sb.Append("namespace 【命名空间】                                         						\r\n  ");
                sb.Append("{                                                              						\r\n  ");
                sb.Append("    ///<summary>                                               						\r\n  ");
                sb.Append("    ///用于【表】业务接口约束定义                              						\r\n  ");
                sb.Append("    ///【表】说明：【表职责】                                  						\r\n  ");
                sb.Append("    ///<summary>                                               						\r\n  ");
                sb.Append("    [ServiceContract]                                          						\r\n  ");
                sb.Append("    public interface I【业务分类】Business                       						\r\n  ");
                sb.Append("    {                                                          						\r\n  ");
                sb.Append("        #region select by Condition                            						\r\n  ");
                sb.Append("                                                               						\r\n  ");
                sb.Append("        ///<summary>                                         						\r\n  ");
                sb.Append("        /// 根据条件返回结果集                                 						\r\n  ");
                sb.Append("        ///<summary>                                        							\r\n  ");
                sb.Append("        [OperationContract]                                    						\r\n  ");
                sb.Append("        List<【类名称】> GetByCondition【业务分类】List(【类名称】 model);	\r\n  ");
                sb.Append("                                                          							\r\n  ");
                sb.Append("        #endregion                                        							\r\n  ");
                sb.Append("        #region select All                                							\r\n  ");
                sb.Append("                                                          							\r\n  ");
                sb.Append("        ///<summary>                                      							\r\n  ");
                sb.Append("        /// 查询所有表数据                                							\r\n  ");
                sb.Append("        ///<summary>                                      							\r\n  ");
                sb.Append("        [OperationContract]                               							\r\n  ");
                sb.Append("        List<【类名称】> GetAll【业务分类】List(); 							\r\n  ");
                sb.Append("                                                          							\r\n  ");
                sb.Append("        #endregion                                        							\r\n  ");
                sb.Append("                                                          							\r\n  ");
                sb.Append("        #region delete                                    							\r\n  ");
                sb.Append("        ///<summary>                                      							\r\n  ");
                sb.Append("        /// 根据条件删除记录，初始默认ID                  							\r\n  ");
                sb.Append("        ///<summary>                                      							\r\n  ");
                sb.Append("        [OperationContract]                                                 			\r\n  ");
                sb.Append("        int Delete【业务分类】(List<【类名称】> models);             			\r\n  ");
                sb.Append("                                                                            			\r\n  ");
                sb.Append("        #endregion                                                          			\r\n  ");
                sb.Append("                                                                            			\r\n  ");
                sb.Append("        #region insert                                                      			\r\n  ");
                sb.Append("        ///<summary>                               									\r\n  ");
                sb.Append("        /// 新增                                   									\r\n  ");
                sb.Append("        ///<summary>                               									\r\n  ");
                sb.Append("        [OperationContract]                                                 			\r\n  ");
                sb.Append("        int Insert【业务分类】(List<【类名称】> models);             			\r\n  ");
                sb.Append("                                                                            			\r\n  ");
                sb.Append("        #endregion                                                          			\r\n  ");
                sb.Append("                                                                            			\r\n  ");
                sb.Append("        #region update                                                      			\r\n  ");
                sb.Append("        ///<summary>                                       							\r\n  ");
                sb.Append("        /// 根据条件更新记录，初始默认ID                   							\r\n  ");
                sb.Append("        ///<summary>                                       							\r\n  ");
                sb.Append("        [OperationContract]                                                 			\r\n  ");
                sb.Append("        int Update【业务分类】(List<【类名称】> models);             			\r\n  ");
                sb.Append("                                                                            			\r\n  ");
                sb.Append("        #endregion                                                          			\r\n  ");
                sb.Append("    }                                                                       			\r\n  ");
                sb.Append("}                                                                           			\r\n  ");
                #endregion

                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", IBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                        .Replace("【开发者名字】", SolutionAuth)
                            .Replace("【实体类命名空间】", EntitiesNameSpace);
                CreateTxt(Path.Combine(IBllLayerPath , "I" + tblName.SplitFirstUnderLine() + "Business.cs"), IBllLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 Business      层代码
        private static void BusinessFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region 类模板
                sb.Append("/**************************************************                           \r\n  ");
                sb.Append("** Company  ：   联想智慧医疗                        \r\n");
                sb.Append("** ClassName：   【业务分类】Business                                                      \r\n  ");
                sb.Append("** Ver      ：   V1.0.0.0                                                       \r\n  ");
                sb.Append("** Desc     ：   用于【表】表业务操作                                            \r\n ");
                sb.Append("** auth     ：   【开发者名字】                                                        \r\n  ");
                sb.Append("** date     ：   【时间戳】                                                      \r\n");
                sb.Append("****************************************************/                         \r\n  ");
                sb.Append("using System;                                                                 \r\n");
                sb.Append("using System.Data;                                                                 \r\n");
                sb.Append("using System.Collections.Generic;                                             \r\n");
                sb.Append("using 【接口命名空间】;                                                       \r\n");
                sb.Append("using 【实体类命名空间】;                                                     \r\n");
                sb.Append("using 【HIS公共命名空间】;                                                    \r\n");
                sb.Append("                                                                              \r\n  ");
                sb.Append("namespace 【命名空间】                                                            \r\n  ");
                sb.Append("{                                                                             \r\n  ");
                sb.Append("    ///<summary>                                                                      \r\n");
                sb.Append("    ///  用于【表】表业务操作                                                         \r\n");
                sb.Append("    ///  【表】说明：【表职责】                                                       \r\n");
                sb.Append("    ///<summary>                                                         \r\n");
                sb.Append("    public class 【业务分类】Business : I【业务分类】Business                     \r\n  ");
                sb.Append("    {                                                                         \r\n  ");
                sb.Append("        #region select by Condition                                           \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        /// 根据条件返回结果集                                                \r\n");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        public List<【类名称】> GetByCondition【业务分类】List(【类名称】 model)              \r\n ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            var _sql = DBHelper.Get【业务分类】Rule().GetByCondition【业务分类】List(model);   \r\n  ");
                sb.Append("            DataTable _dt = DBHelper.Instance.Query(_sql).Tables[0];                 \r\n  ");
                sb.Append("            var _List = FuncTableHelper.DataTableToListByAttr<【类名称】>(_dt);      \r\n  ");
                sb.Append("            return _List;                                                            \r\n  ");
                sb.Append("        }                                                                     \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("        #region select All                                                       \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 查询所有表数据                                     \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public List<【类名称】> GetAll【业务分类】List()                                \r\n ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            var _sql = DBHelper.Get【业务分类】Rule().GetAll【业务分类】List();   \r\n  ");
                sb.Append("            DataTable _dt = DBHelper.Instance.Query(_sql).Tables[0];                 \r\n  ");
                sb.Append("            var _List = FuncTableHelper.DataTableToListByAttr<【类名称】>(_dt);      \r\n  ");
                sb.Append("            return _List;                                                            \r\n  ");
                sb.Append("        }                                                                     \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region delete                                                        \r\n  ");
                sb.Append("        ///<summary>                                                           \r\n");
                sb.Append("        /// 根据条件删除记录，初始条件默认ID                                   \r\n");
                sb.Append("        ///<summary>                                                           \r\n");
                sb.Append("        public int Delete【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            var _sql = DBHelper.Get【业务分类】Rule().Delete【业务分类】(models);     \r\n  ");
                sb.Append("            int result = DBHelper.Instance.ExecSqlTran(_sql);                                \r\n  ");
                sb.Append("            return result;                                                                   \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region insert                                                        \r\n  ");
                sb.Append("        ///<summary>                                               \r\n");
                sb.Append("        /// 新增                                                   \r\n");
                sb.Append("        ///<summary>                                               \r\n");
                sb.Append("        public int Insert【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            var _sql = DBHelper.Get【业务分类】Rule().Insert【业务分类】(models);     \r\n  ");
                sb.Append("            int result = DBHelper.Instance.ExecSqlTran(_sql);                                \r\n  ");
                sb.Append("            return result;                                                                   \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region update                                                        \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 根据条件更新记录，初始条件默认ID                                  \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public int Update【业务分类】(List<【类名称】> models)                           \r\n ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            var _sql = DBHelper.Get【业务分类】Rule().Update【业务分类】(models);     \r\n  ");
                sb.Append("            int result = DBHelper.Instance.ExecSqlTran(_sql);                                \r\n  ");
                sb.Append("            return result;                                                                   \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("    }                                                                         \r\n  ");
                /************************  BaseBusiness   分割线 *******************/

                sb.Append("    public partial class BaseBusiness                                         \r\n  ");
                sb.Append("    {                                                                         \r\n  ");
                sb.Append("         private 【业务分类】Business m_【业务分类】Business = new 【业务分类】Business();\r\n  ");
                sb.Append("        #region select by Condition                                           \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        /// 根据条件返回结果集                                                \r\n");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        public List<【类名称】> GetByCondition【业务分类】List(【类名称】 model)             \r\n ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            return m_【业务分类】Business.GetByCondition【业务分类】List(model);     \r\n  ");
                sb.Append("        }                                                                     \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("        #region select All                                                       \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 查询所有表数据                                     \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public List<【类名称】> GetAll【业务分类】List()                           \r\n  ");
                sb.Append("        {                                                                          \r\n  ");
                sb.Append("            return m_【业务分类】Business.GetAll【业务分类】List();           \r\n  ");
                sb.Append("        }                                                                          \r\n  ");
                sb.Append("        #endregion                                                                 \r\n  ");
                sb.Append("                                                                                   \r\n  ");
                sb.Append("        #region delete                                                             \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        /// 根据条件删除记录，初始条件默认ID                                       \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        public int Delete【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return m_【业务分类】Business.Delete【业务分类】(models);     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region insert                                                        \r\n  ");
                sb.Append("        ///<summary>                                               \r\n");
                sb.Append("        /// 新增                                                   \r\n");
                sb.Append("        ///<summary>                                               \r\n");
                sb.Append("        public int Insert【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return m_【业务分类】Business.Insert【业务分类】(models);     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region update                                                        \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 根据条件更新记录，初始条件默认ID                                  \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public int Update【业务分类】(List<【类名称】> models)                           \r\n ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return m_【业务分类】Business.Update【业务分类】(models);     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("    }                                                                         \r\n  ");

                sb.Append("}                                                                             \r\n  ");
                #endregion
                string tblName = item.Key.Split(DicSplit)[0];
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", sBllNameSpace)
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【接口命名空间】", IBllNameSpace)
                            .Replace("【HIS公共命名空间】", HisCommonNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【开发者名字】", SolutionAuth);
                CreateTxt(Path.Combine(BllLayerPath , tblName.SplitFirstUnderLine() + "Business.cs"), BllLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 Proxy         层代码
        private static void ProxyFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region 类模板
                sb.Append("/**************************************************                           \r\n  ");
                sb.Append("** Company  ：   联想智慧医疗                                                   \r\n");
                sb.Append("** ClassName：   【类名称】                                                      \r\n  ");
                sb.Append("** Ver      ：   V1.0.0.0                                                       \r\n  ");
                sb.Append("** Desc     ：   用于【表】表客户端代理业务操作                                 \r\n ");
                sb.Append("** auth     ：   【开发者名字】                                                         \r\n  ");
                sb.Append("** date     ：   【时间戳】                                                      \r\n");
                sb.Append("****************************************************/                         \r\n  ");
                sb.Append("using System;                                                                 \r\n");
                sb.Append("using System.Collections.Generic;                                             \r\n");
                sb.Append("using 【接口命名空间】;                                                       \r\n");
                sb.Append("using 【实体类命名空间】;                                                     \r\n");
                sb.Append("using 【HIScUtils命名空间】;                                                  \r\n");
                sb.Append("                                                                              \r\n  ");
                sb.Append("namespace 【命名空间】                                                            \r\n  ");
                sb.Append("{                                                                             \r\n  ");
                sb.Append("    ///<summary>                                                                      \r\n");
                sb.Append("    ///  用于【表】表客户端代理业务操作                                                         \r\n");
                sb.Append("    ///  【表】说明：【表职责】                                                       \r\n");
                sb.Append("    ///<summary>                                                         \r\n");
                sb.Append("    public class Proxy【业务分类】Business :【代理业务基类】, I【业务分类】Business  \r\n  ");
                sb.Append("    {                                                                         \r\n  ");
                sb.Append("        #region select by Condition                                           \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        /// 根据条件返回结果集                                                \r\n");
                sb.Append("        ///<summary>                                                          \r\n");
                sb.Append("        public List<【类名称】> GetByCondition【业务分类】List(【类名称】 model)             \r\n ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            return FuncWcfProxy.BaseWork(this, proxy => proxy.Channel.GetByCondition【业务分类】List(model)); \r\n  ");
                sb.Append("        }                                                                     \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("        #region select All                                                       \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 查询所有表数据                                     \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public List<【类名称】> GetAll【业务分类】List()                           \r\n  ");
                sb.Append("        {                                                                          \r\n  ");
                sb.Append("            return FuncWcfProxy.BaseWork(this, proxy => proxy.Channel.GetAll【业务分类】List());           \r\n  ");
                sb.Append("        }                                                                          \r\n  ");
                sb.Append("        #endregion                                                                 \r\n  ");
                sb.Append("                                                                                   \r\n  ");
                sb.Append("        #region delete                                                             \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        /// 根据条件删除记录，初始条件默认ID                                       \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        public int Delete【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return FuncWcfProxy.BaseWork(this, proxy => proxy.Channel.Delete【业务分类】(models));     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region insert                                                        \r\n  ");
                sb.Append("        ///<summary>                                               \r\n");
                sb.Append("        /// 新增                                                   \r\n");
                sb.Append("        ///<summary>                                               \r\n"); 
                sb.Append("        public int Insert【业务分类】(List<【类名称】> models)                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return FuncWcfProxy.BaseWork(this, proxy => proxy.Channel.Insert【业务分类】(models));     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region update                                                        \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 根据条件更新记录，初始条件默认ID                                  \r\n");
                sb.Append("        ///<summary>                                         \r\n"); 
                sb.Append("        public int Update【业务分类】(List<【类名称】> models)                           \r\n ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return FuncWcfProxy.BaseWork(this, proxy => proxy.Channel.Update【业务分类】(models));     \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("    }                                                                         \r\n  ");
                sb.Append("}                                                                             \r\n  ");
                #endregion
                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【代理业务基类】", ProxyBaseName)
                            .Replace("【HIScUtils命名空间】", HiscUtilsNameSpace)
                            .Replace("【接口命名空间】", IBllNameSpace)
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", cBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                CreateTxt(Path.Combine(cProxyLayerPath , "Proxy" + tblName.SplitFirstUnderLine() + "Business.cs"), cProxyLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 Presenter     层代码
        private static void PresenterFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region 类模板
                sb.Append("/**************************************************                           \r\n  ");
                sb.Append("** Company  ：   联想智慧医疗                                                  \r\n");
                sb.Append("** ClassName：   【类名称】                                                      \r\n  ");
                sb.Append("** Ver      ：   V1.0.0.0                                                       \r\n  ");
                sb.Append("** Desc     ：   用于【表】表客户端Presenter操作                                 \r\n ");
                sb.Append("** auth     ：   【开发者名字】                                                         \r\n  ");
                sb.Append("** date     ：   【时间戳】                                                      \r\n");
                sb.Append("****************************************************/                         \r\n  ");
                sb.Append("using System;                                                                 \r\n");
                sb.Append("using System.Collections.Generic;                                             \r\n");
                sb.Append("using 【实体类命名空间】;                                                     \r\n");
                sb.Append("                                                                              \r\n  ");
                sb.Append("namespace 【命名空间】                                                            \r\n  ");
                sb.Append("{                                                                             \r\n  ");
                sb.Append("    ///<summary>                                                                      \r\n");
                sb.Append("    ///  用于【表】表客户端Presenter操作                                                         \r\n");
                sb.Append("    ///  【表】说明：【表职责】                                                       \r\n");
                sb.Append("    ///<summary>                                                                     \r\n");
                sb.Append("    public class 【业务分类】Presenter : Presenter<I【业务分类】View>                    \r\n  ");
                sb.Append("    {                                                                                   \r\n  ");
                sb.Append("        public 【业务分类】Presenter(I【业务分类】View view): base(view)                \r\n  ");
                sb.Append("        {                                                                               \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("        }                                                                               \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("        public override void OnViewEvent()                                              \r\n  ");
                sb.Append("        {                                                                               \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("        }                                                                               \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("        public override void OnViewLoaded()                                              \r\n  ");
                sb.Append("        {                                                                                \r\n  ");
                sb.Append("             View.m_Select【业务分类】Condition = new 【类名称】();                      \r\n  ");
                sb.Append("             View.m_【业务分类】s = new List<【类名称】>();                             \r\n  ");
                sb.Append("             View.m_Delete【业务分类】s = new List<【类名称】>();                        \r\n  ");
                sb.Append("             View.m_Insert【业务分类】s = new List<【类名称】>();                        \r\n  ");
                sb.Append("             View.m_Update【业务分类】s = new List<【类名称】>();                        \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("                                                                                        \r\n  ");
                sb.Append("        }                                                                               \r\n  ");
                sb.Append("        #region GetByCondition【业务分类】List                                          \r\n  ");
                sb.Append("        ///<summary>                                                                    \r\n");
                sb.Append("        /// 根据条件返回结果集                                                          \r\n");
                sb.Append("        ///<summary>                                                                    \r\n");
                sb.Append("        public List<【类名称】> GetByCondition【业务分类】List()                    \r\n ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            return FacadeProxy.【业务分类】BusinessProxy().GetByCondition【业务分类】List(View.m_Select【业务分类】Condition);\r\n  ");
                sb.Append("        }                                                                     \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("        #region select All                                                       \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 查询所有表数据                                     \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public List<【类名称】> GetAll【业务分类】List()                           \r\n  ");
                sb.Append("        {                                                                          \r\n  ");
                sb.Append("            return FacadeProxy.【业务分类】BusinessProxy().GetAll【业务分类】List();\r\n  ");
                sb.Append("        }                                                                          \r\n  ");
                sb.Append("        #endregion                                                                 \r\n  ");
                sb.Append("                                                                                   \r\n  ");
                sb.Append("        #region delete                                                             \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        /// 根据条件删除记录，初始条件默认ID                                       \r\n  ");
                sb.Append("        ///<summary>                                                               \r\n  ");
                sb.Append("        public int Delete【业务分类】()                                            \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return FacadeProxy.【业务分类】BusinessProxy().Delete【业务分类】(View.m_Delete【业务分类】s);       \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region insert                                                        \r\n  ");
                sb.Append("        ///<summary>                                                              \r\n");
                sb.Append("        /// 新增                                                                  \r\n");
                sb.Append("        ///<summary>                                                              \r\n");
                sb.Append("        public int Insert【业务分类】()                           \r\n  ");
                sb.Append("        {                                                                                    \r\n  ");
                sb.Append("            return FacadeProxy.【业务分类】BusinessProxy().Insert【业务分类】(View.m_Insert【业务分类】s);       \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #region update                                                        \r\n  ");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        /// 根据条件更新记录，初始条件默认ID                                  \r\n");
                sb.Append("        ///<summary>                                         \r\n");
                sb.Append("        public int Update【业务分类】()                           \r\n ");
                sb.Append("        {                                                                                                      \r\n  ");
                sb.Append("            return FacadeProxy.【业务分类】BusinessProxy().Update【业务分类】(View.m_Update【业务分类】s);       \r\n  ");
                sb.Append("        }                                                                                    \r\n  ");
                sb.Append("                                                                              \r\n  ");
                sb.Append("        #endregion                                                            \r\n  ");
                sb.Append("    }                                                                         \r\n  ");
                sb.Append("}                                                                             \r\n  ");
                #endregion
                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【代理业务基类】", ProxyBaseName)
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", cBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                CreateTxt(Path.Combine(cPresenterLayerPath , tblName.SplitFirstUnderLine() + "Presenter.cs"), cPresenterLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 I-----View    层代码
        private static void IInterfaceViewFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region  类模板
                sb.Append("/**************************************************                                  \r\n  ");
                sb.Append("** Company  ：   联想智慧医疗                                                       \r\n  ");
                sb.Append("** ClassName：   【类名称】                                                          \r\n  ");
                sb.Append("** Ver      ：   V1.0.0.0                                                           \r\n  ");
                sb.Append("** Desc     ：   用于【表】表客户端接口                                             \r\n  ");
                sb.Append("** auth     ：   【开发者名字】                                                             \r\n  ");
                sb.Append("** date     ：   【时间戳】                                                          \r\n  ");
                sb.Append("****************************************************/                                \r\n  ");
                sb.Append("using System.ComponentModel;                                                         \r\n  ");
                sb.Append("using System.Collections.Generic;                                                    \r\n  ");
                sb.Append("using 【实体类命名空间】;                                                            \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("namespace 【命名空间】                                                               \r\n  ");
                sb.Append("{                                                                                    \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    ///  用于【表】表客户端接口定义约束                                              \r\n  ");
                sb.Append("    ///  【表】说明：【表职责】                                                      \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    public interface I【业务分类】View                                                 \r\n  ");
                sb.Append("    {                                                                                \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 查询条件参数                                                             \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        【类名称】 m_Select【业务分类】Condition { get; set; }                                   \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 查询结果集                                                               \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        BindingList<【业务分类】ViewModel> m_【业务分类】sViewModel { get; set; }    \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 查询结果集                                                               \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        List<【类名称】> m_【业务分类】s { get; set; }                             \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 新增记录参数                                                             \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        List<【类名称】 > m_Insert【业务分类】s { get; set; }                        \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 更新记录参数                                                             \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        List<【类名称】> m_Update【业务分类】s { get; set; }                         \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        /// 删除记录参数                                                             \r\n  ");
                sb.Append("        ///<summary>                                                                 \r\n  ");
                sb.Append("        List<【类名称】> m_Delete【业务分类】s { get; set; }                         \r\n  ");
                sb.Append("    }                                                                                \r\n  ");
                sb.Append("}                                                                                    \r\n  ");
                #endregion
                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", cBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                CreateTxt(Path.Combine(cViewLayerPath , "I" + tblName.SplitFirstUnderLine() + "View.cs"), cViewLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 ViewModel     层代码
        private static void ViewModelFactory(Dictionary<string, Dictionary<string, string>> dic)
        {

            foreach (var item in dic)
            {
                StringBuilder sb = new StringBuilder();
                #region  类模板
                sb.Append("/**************************************************                                  \r\n  ");
                sb.Append("** Company  ：  联想智慧医疗                                                       \r\n  ");
                sb.Append("** ClassName：  【类名称】                                                          \r\n  ");
                sb.Append("** Ver      ：  V1.0.0.0                                                           \r\n  ");
                sb.Append("** Desc     ：  用于【表】表客户端接口                                             \r\n  ");
                sb.Append("** auth     ：  【开发者名字】                                                             \r\n  ");
                sb.Append("** date     ：  【时间戳】                                                          \r\n  ");
                sb.Append("****************************************************/                                \r\n  ");
                sb.Append("using System;                                                                        \r\n  ");
                sb.Append("using System.Runtime.Serialization;                                                  \r\n  ");
                sb.Append("using 【实体类命名空间】;                                                            \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("namespace 【命名空间】                                                               \r\n  ");
                sb.Append("{                                                                                    \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    ///  用于【表】表客户端接口定义约束                                              \r\n  ");
                sb.Append("    ///  【表】说明：【表职责】                                                      \r\n  ");
                sb.Append("    ///<summary>                                                                     \r\n  ");
                sb.Append("    [Serializable]                                                                   \r\n  ");
                sb.Append("    [DataContract]                                                                   \r\n  ");
                sb.Append("    public class 【业务分类】ViewModel : 【类名称】,ICheckable                         \r\n  ");
                sb.Append("    {                                                                                \r\n  ");
                sb.Append("        /// <summary> 										                        \r\n  ");
                sb.Append("        /// 是否选择                                                                 \r\n  ");
                sb.Append("        /// </summary>                                                               \r\n  ");
                sb.Append("        [DataMember]                                                                 \r\n  ");
                sb.Append("        public bool IsPicking { set; get; }                                          \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        /// <summary>                                                                \r\n  ");
                sb.Append("        /// 是否修改                                                                 \r\n  ");
                sb.Append("        /// </summary>                                                               \r\n  ");
                sb.Append("        [DataMember]                                                                 \r\n  ");
                sb.Append("        public bool IsChange { set; get; }                                           \r\n  ");
                sb.Append("                                                                                     \r\n  ");
                sb.Append("        /// <summary>                                                                \r\n  ");
                sb.Append("        /// 唯一标识                                                                 \r\n  ");
                sb.Append("        /// </summary>                                                               \r\n  ");
                sb.Append("        public dynamic UniqueID                                                      \r\n  ");
                sb.Append("        {                                                                            \r\n  ");
                sb.Append("            get                                                                      \r\n  ");
                sb.Append("            {                                                                        \r\n  ");
                sb.Append("                return this.【主键名称】;                                                      \r\n  ");
                sb.Append("            }                                                                        \r\n  ");
                sb.Append("        }													                        \r\n  ");
                sb.Append("    }                                                                                \r\n  ");
                sb.Append("}                                                                                    \r\n  ");
                #endregion
                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                string r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", cBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【主键名称】", tableInfo[2].SplitUnderLine())
                            .Replace("【开发者名字】", SolutionAuth);
                CreateTxt(Path.Combine(cViewModelLayerPath , tblName.SplitFirstUnderLine() + "ViewModel.cs"), cViewModelLayerPath, r);
            }

        }
        #endregion

        #region 遍历生成 RuleFactoryFactory   层代码
        private static void RuleFactoryFactory(Dictionary<string, Dictionary<string, string>> dic)
        {
            StringBuilder sb = new StringBuilder();
            string r = "";

            sb.Append("/**************************************************                                  \r\n  ");
            sb.Append("** Company  ：  联想智慧医疗                                                       \r\n  ");
            sb.Append("** ClassName：  RuleFactory                                                          \r\n  ");
            sb.Append("** Ver      ：  V1.0.0.0                                                           \r\n  ");
            sb.Append("** Desc     ：  RuleFactory 抽象                                                     \r\n  ");
            sb.Append("** auth     ：  【开发者名字】                                                             \r\n  ");
            sb.Append("** date     ：  【时间戳】                                                          \r\n  ");
            sb.Append("****************************************************/                                \r\n  ");
            sb.Append("using 【HIS实体命名空间】;                                                                           \r\n  ");
            sb.Append("using System.Reflection;                                                                             \r\n  ");
            sb.Append("                                                                                                     \r\n  ");
            sb.Append("namespace 【命名空间】                                                                             \r\n  ");
            sb.Append("{                                                                                                    \r\n  ");
            sb.Append("    public abstract class RuleFactory                                                                \r\n  ");
            sb.Append("    {                                                                                                \r\n  ");
            sb.Append("        /// <summary>                                                                                \r\n  ");
            sb.Append("        /// rule对象创建                                                                             \r\n  ");
            sb.Append("        /// </summary>                                                                               \r\n  ");
            sb.Append("        /// <returns></returns>                                                                      \r\n  ");
            sb.Append("        public static RuleFactory CreateInterface()                                                  \r\n  ");
            sb.Append("        {                                                                                            \r\n  ");
            sb.Append("            string _rule = System.Configuration.ConfigurationManager.AppSettings[\"RuleName\"];        \r\n  ");
            sb.Append("            RuleFactory _interface;                                                                  \r\n  ");
            sb.Append("            string _dllPath = \"\";                                                                    \r\n  ");
            sb.Append("            string _className = \"\";                                                                  \r\n  ");
            sb.Append("            if (_rule == \"Base\" || _rule == \"\")                                                      \r\n  ");
            sb.Append("            {                                                                                        \r\n  ");
            sb.Append("                _dllPath = \"【命名空间】\";                                                         \r\n  ");
            sb.Append("            }                                                                                        \r\n  ");
            sb.Append("            else                                                                                     \r\n  ");
            sb.Append("            {                                                                                        \r\n  ");
            sb.Append("                _dllPath = \"【命名空间】\" + \".\" + _rule;                                         \r\n  ");
            sb.Append("            }                                                                                        \r\n  ");
            sb.Append("            _className = _dllPath + \".BaseRule\";                                                     \r\n  ");
            sb.Append("            _interface = (RuleFactory)Assembly.Load(_dllPath).CreateInstance(_className);            \r\n  ");
            sb.Append("            return _interface;                                                                       \r\n  ");
            sb.Append("        }                                                                                            \r\n  ");
            foreach (var item in dic)
            {
                #region  类模板
                sb.Append("        /// <summary>						                                                  \r\n  ");
                sb.Append("        /// 【业务分类】Rule 工厂抽象定义                                                            \r\n  ");
                sb.Append("        /// 【表职责】                                                                         \r\n  ");
                sb.Append("        /// </summary>                                                                          \r\n  ");
                sb.Append("        /// <param name=\"dbType\"></param>                                                     \r\n  ");
                sb.Append("        /// <returns></returns>                                                                 \r\n  ");
                sb.Append("        public abstract 【业务分类】Rule Create【业务分类】Rule(EnumDBType dbType);                  \r\n  ");
                #endregion

                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", RuleNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【HIS实体命名空间】", HisModelNameSpace)
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                sb.Clear();
                sb.Append(r); 
            }
            sb.Append("    }                                                                                                   \r\n  ");
            sb.Append("}                                                                                                       \r\n  ");
            r = sb.ToString();
            CreateTxt(Path.Combine(sRuleLayerPath , "RuleFactory.cs"), sRuleLayerPath, r);

        }
        #endregion

        #region 遍历生成 BaseRule      层代码
        private static void BaseRuleFactory(Dictionary<string, Dictionary<string, string>> dic)
        {
            StringBuilder sb = new StringBuilder();
            string r = "";

            sb.Append("/**************************************************                                  \r\n  ");
            sb.Append("** Company  ：  联想智慧医疗                                                       \r\n  ");
            sb.Append("** ClassName：  BaseRule                                                          \r\n  ");
            sb.Append("** Ver      ：  V1.0.0.0                                                           \r\n  ");
            sb.Append("** Desc     ：  BaseRule                                                           \r\n  ");
            sb.Append("** auth     ：  【开发者名字】                                                             \r\n  ");
            sb.Append("** date     ：  【时间戳】                                                          \r\n  ");
            sb.Append("****************************************************/                                \r\n  ");
            sb.Append("using 【HIS实体命名空间】;                                                       \r\n  ");
            sb.Append("                                                                                 \r\n  ");
            sb.Append("namespace Lenovo.CIS.Consultation.sDBRule                                        \r\n  ");
            sb.Append("{                                                                                \r\n  ");
            sb.Append("    /// <summary>                                                                \r\n  ");
            sb.Append("    /// 实现                                                                     \r\n  ");
            sb.Append("    /// </summary>                                                               \r\n  ");
            sb.Append("    public class BaseRule : RuleFactory                                          \r\n  ");
            sb.Append("    {                                                                            \r\n  ");
            sb.Append("                                                                                 \r\n  ");
            foreach (var item in dic)
            {
                #region  类模板
                sb.Append("        /// <summary>						                                                  \r\n  ");
                sb.Append("        /// 【业务分类】Rule 抽象 实现                                                            \r\n  ");
                sb.Append("        /// 【表职责】                                                                         \r\n  ");
                sb.Append("        /// </summary>                                                                          \r\n  ");
                sb.Append("        /// <param name=\"dbType\"></param>                                                     \r\n  ");
                sb.Append("        /// <returns></returns>                                                                 \r\n  ");
                sb.Append("        public override 【业务分类】Rule Create【业务分类】Rule(EnumDBType dbType)               \r\n  ");
                sb.Append("        {                                                                        \r\n  ");
                sb.Append("            return new 【业务分类】ConcreteRule(dbType);                                 \r\n  ");
                sb.Append("        }                                                                        \r\n  ");
                #endregion

                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", RuleNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【HIS实体命名空间】", HisModelNameSpace)
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                sb.Clear();
                sb.Append(r);
            }
            sb.Append("    }                                                                                                   \r\n  ");
            sb.Append("}                                                                                                       \r\n  ");
            r = sb.ToString();
            CreateTxt(Path.Combine(sRuleLayerPath , "BaseRule.cs"), sRuleLayerPath, r);

        }
        #endregion

        #region 遍历生成 PresenterBase 层代码
        private static void PresenterBaseFactory(Dictionary<string, Dictionary<string, string>> dic)
        {
            StringBuilder sb = new StringBuilder();
            string r = "";

            sb.Append("/**************************************************                                  \r\n  ");
            sb.Append("** Company  ：  联想智慧医疗                                                       \r\n  ");
            sb.Append("** ClassName：  PresenterBase                                                          \r\n  ");
            sb.Append("** Ver      ：  V1.0.0.0                                                           \r\n  ");
            sb.Append("** Desc     ：  PresenterBase                                                       \r\n  ");
            sb.Append("** auth     ：  【开发者名字】                                                             \r\n  ");
            sb.Append("** date     ：  【时间戳】                                                          \r\n  ");
            sb.Append("****************************************************/                                \r\n  ");

            sb.Append("namespace Lenovo.CIS.Consultation.cBusiness		\r\n  ");
            sb.Append("{                                                \r\n  ");
            sb.Append("    /// <summary>                                \r\n  ");
            sb.Append("    /// Presenter基础类                          \r\n  ");
            sb.Append("    /// </summary>                               \r\n  ");
            sb.Append("    /// <typeparam name=\"IView\"></typeparam>   \r\n  ");
            sb.Append("    public class Presenter<IView>                \r\n  ");
            sb.Append("    {                                            \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        public IView View { get; set; }          \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        public Presenter(IView view)             \r\n  ");
            sb.Append("        {                                        \r\n  ");
            sb.Append("            this.View = view;                    \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("            this.OnViewEvent();                  \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("            this.OnViewLoaded();                 \r\n  ");
            sb.Append("        }                                        \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        virtual public void OnViewLoaded()       \r\n  ");
            sb.Append("        {                                        \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        }                                        \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        virtual public void OnViewEvent()        \r\n  ");
            sb.Append("        {                                        \r\n  ");
            sb.Append("                                                 \r\n  ");
            sb.Append("        }                                        \r\n  ");
            sb.Append("    }                                            \r\n  ");
            sb.Append("}                                                \r\n  ");
            r = sb.ToString()
                        .Replace("【时间戳】", DateTime.Now.ToString())
                        .Replace("【开发者名字】", SolutionAuth);
            CreateTxt(Path.Combine(cPresenterLayerPath , "Presenter.cs"), cPresenterLayerPath, r);

        }
        #endregion

        #region 遍历生成 RuleFactory   层代码
        private static void FacadeProxyFactory(Dictionary<string, Dictionary<string, string>> dic)
        {
            StringBuilder sb = new StringBuilder();
            string r = "";

            sb.Append("/**************************************************                                  \r\n  ");
            sb.Append("** Company  ：  联想智慧医疗                                                       \r\n  ");
            sb.Append("** ClassName：  FacadeProxyFactory                                                          \r\n  ");
            sb.Append("** Ver      ：  V1.0.0.0                                                           \r\n  ");
            sb.Append("** Desc     ：  FacadeProxyFactory 代理类总代理                                             \r\n  ");
            sb.Append("** auth     ：  【开发者名字】                                                             \r\n  ");
            sb.Append("** date     ：  【时间戳】                                                          \r\n  ");
            sb.Append("****************************************************/                                \r\n  ");
            sb.Append("                                                                                                     \r\n  ");
            sb.Append("namespace 【命名空间】                                                                             \r\n  ");
            sb.Append("{                                                                                                    \r\n  ");
            sb.Append("    /// <summary>                                                                                \r\n  ");
            sb.Append("    /// 公共代理                                                                                  \r\n  ");
            sb.Append("    /// </summary>                                                                               \r\n  ");
            sb.Append("    public class FacedeProxy                                                                         \r\n  ");
            sb.Append("    {                                                                                                \r\n  ");
            foreach (var item in dic)
            {
                #region  类模板
                sb.Append("        /// <summary>						                                                   \r\n  ");
                sb.Append("        /// 【类名称】 生成代理对象                                                             \r\n  ");
                sb.Append("        /// 【表职责】                                                                          \r\n  ");
                sb.Append("        /// </summary>                                                                          \r\n  ");
                sb.Append("        /// <param name=\"dbType\"></param>                                                     \r\n  ");
                sb.Append("        /// <returns>【类名称】代理</returns>                                                   \r\n  ");
                sb.Append("        public static Proxy【业务分类】Business 【业务分类】BusinessProxy()                     \r\n  ");
                sb.Append("        {                                                                                       \r\n  ");
                sb.Append("            return new Proxy【业务分类】Business();                                                 \r\n  ");
                sb.Append("        }                                                                                       \r\n  ");
                #endregion

                string[] tableInfo = item.Key.Split(DicSplit);
                string tblName = tableInfo[0];
                string tblWork = tableInfo.Length == 3 ? tableInfo[1] : "";
                r = sb.ToString()
                            .Replace("【类名称】", tblName.SplitUnderLine())
                            .Replace("【业务分类】", tblName.SplitFirstUnderLine())
                            .Replace("【时间戳】", DateTime.Now.ToString())
                            .Replace("【命名空间】", cBllNameSpace)
                            .Replace("【表】", tblName)
                            .Replace("【表职责】", tblWork.SplitSpecialChar())
                            .Replace("【HIS实体命名空间】", HisModelNameSpace)
                            .Replace("【实体类命名空间】", EntitiesNameSpace)
                            .Replace("【开发者名字】", SolutionAuth);
                sb.Clear();
                sb.Append(r);
            }
            sb.Append("    }                                                                                                   \r\n  ");
            sb.Append("}                                                                                                       \r\n  ");
            r = sb.ToString();
            CreateTxt(Path.Combine(cProxyLayerPath , "FacadeProxy.cs"), cProxyLayerPath, r);

        }
        #endregion

        #region 其他

        //获取表字段集合
        public static string GetFieldsList(string tableName, Dictionary<string, string> filedDic)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\"       \r\n");
            int i = 0;
            foreach (var item in filedDic)
            {
                string[] key = item.Key.Split(DicSplit);
                string filedName = key[0];
                string splitChar = ",";
                if (i + 1 == filedDic.Count)
                    splitChar = "";
                sb.AppendFormat("        {0} {1}                   \r\n", filedName, splitChar);
                i++;
            }
            sb.AppendFormat("        \"       ");
            return sb.ToString();
        }
        //为某个表生成delete语句
        public static string GetDeleteSQL(string tableName, Dictionary<string, string> filedDic,string keyName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("            List<string> _sqlList = new List<string>();        \r\n");
            sb.AppendFormat("            foreach(var model in models)        \r\n");
            sb.Append("            {                                \r\n");
            sb.AppendFormat("                StringBuilder sql = new StringBuilder();       \r\n");
            sb.AppendFormat("                sql.AppendFormat(\" DELETE FROM {0} WHERE {1}={2}  \",model.{3}); \r\n", tableName, keyName, "{0}", keyName.SplitUnderLine());
            sb.Append("                _sqlList.Add(sql.ToString());                               \r\n");
            sb.Append("            }                                \r\n");
            return sb.ToString();
        }
        //为某个表生成insert语句
        public static string GetInsertSQL(string tableName, Dictionary<string, string> filedDic)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("            List<string> _sqlList = new List<string>();        \r\n");
            sb.AppendFormat("            foreach(var model in models)        \r\n");
            sb.Append("            {      \r\n");
            sb.AppendFormat("               StringBuilder sql = new StringBuilder();           \r\n");
            sb.AppendFormat("               #region sql                                       \r\n");
            sb.AppendFormat("               sql.Append(\"INSERT INTO {0}  \");                \r\n", tableName);
            sb.AppendFormat("               sql.Append(\" ( \"); \r\n");
            sb.Append("               sql.Append( m_tbFields);                              \r\n");
            sb.AppendFormat("               sql.Append(\" ) \"); \r\n");
            sb.AppendFormat("               sql.Append(\" VALUES  ( \");                      \r\n");
            int b = 0;
            foreach (var item in filedDic)
            {
                string[] key = item.Key.Split(DicSplit);
                string filedName = key[0];
                string splitChar = ",";
                if (b + 1 == filedDic.Count)
                    splitChar = "";
                if (CheckNumberType(item.Value)) { sb.AppendFormat("               sql.AppendFormat(\"    {0} {1} \",model.{2});     \r\n", "{0}", splitChar, filedName.SplitUnderLine()); }
                else if (CheckDataTimeType(item.Value))
                {
                    sb.AppendFormat("               sql.AppendFormat(\"     to_date('{0}','yyyy-mm-dd hh24:mi:ss') {1} \",model.{2});     \r\n", "{0}", splitChar, filedName.SplitUnderLine());
                }
                else 
                sb.AppendFormat("               sql.AppendFormat(\"    '{0}' {1} \",model.{2});     \r\n", "{0}", splitChar, filedName.SplitUnderLine());
                b++;
            }
            sb.AppendFormat("               sql.Append(\" ) \");                             \r\n");
            sb.AppendFormat("               _sqlList.Add(sql.ToString());                    \r\n"); 
            sb.AppendFormat("               #endregion sql                                   \r\n");
            sb.Append("               }      \r\n");
            return sb.ToString();
        }

        //为某个表生成update语句
        public static string GetUpdateSQL(string tableName, Dictionary<string, string> filedDic,string keyName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("            List<string> _sqlList = new List<string>();            \r\n");
            sb.AppendFormat("            foreach(var model in models)                           \r\n");
                  sb.Append("            {                                                      \r\n");
            sb.AppendFormat("                StringBuilder sql = new StringBuilder();           \r\n");
            sb.AppendFormat("                #region sql                                        \r\n");
            sb.AppendFormat("                sql.Append(\" UPDATE {0} SET   \");                \r\n", tableName);
            int i = 0;
            foreach (var item in filedDic)
            {
                string[] key = item.Key.Split(DicSplit);
                string filedName = key[0];
                string splitChar = ",";
                if (i + 1 == filedDic.Count)
                    splitChar = "";
                if (filedName != keyName) {
                    if (CheckNumberType(item.Value))
                    {
                        sb.AppendFormat("                sql.AppendFormat(\"   {0} = {1} {2} \",model.{3}); \r\n", filedName, "{0}", splitChar, filedName.SplitUnderLine());
                    }
                    else if (CheckDataTimeType(item.Value))
                    {
                        sb.AppendFormat("                sql.AppendFormat(\"   {0} = to_date('{1}','yyyy-mm-dd hh24:mi:ss') {2} \",model.{3}); \r\n", filedName, "{0}", splitChar, filedName.SplitUnderLine());
                    }
                    else
                    sb.AppendFormat("                sql.AppendFormat(\"   {0} = '{1}' {2} \",model.{3}); \r\n", filedName, "{0}", splitChar, filedName.SplitUnderLine());
                }
                i++;
            }
      sb.AppendFormat("                sql.AppendFormat(\" Where {0}={1}   \",model.{2}); \r\n", keyName, "{0}", keyName.SplitUnderLine());
            sb.Append("                _sqlList.Add(sql.ToString());                               \r\n");
      sb.AppendFormat("                #endregion sql                                       \r\n");
            sb.Append("                }                                \r\n");

            return sb.ToString();
        }

        //生成cs文件
        public static void CreateTxt( string filePath, string folderPath, string fileContent)
        {
            if (!Directory.Exists(folderPath))//如果不存在就创建文件夹
                Directory.CreateDirectory(folderPath);
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.Write(fileContent);
            sw.Close();
            fs.Close();
        }

        // 数据库中与C#中的数据类型对照
        private static string ChangeToCSharpType(string type)
        {
            string reval = string.Empty;
            switch (type.ToLower())
            {
                case "int":
                    reval = "int?";
                    break;
                case "text":
                    reval = "string";
                    break;
                case "bigint":
                    reval = "int?";
                    break;
                case "binary":
                    reval = "byte[]";
                    break;
                case "bit":
                    reval = "bool";
                    break;
                case "char":
                    reval = "string";
                    break;
                case "datetime":
                    reval = "DateTime?";
                    break;
                case "decimal":
                    reval = "decimal?";
                    break;
                case "float":
                    reval = "double?";
                    break;
                case "image":
                    reval = "byte[]";
                    break;
                case "money":
                    reval = "decimal?";
                    break;
                case "nchar":
                    reval = "string";
                    break;
                case "ntext":
                    reval = "string";
                    break;
                //case "numeric":
                //    reval = "decimal?";
                //    break;
                case "numeric":
                    reval = "long?";
                    break;
                case "number":
                    reval = "int?";
                    break;
                case "nvarchar":
                    reval = "string";
                    break;
                case "real":
                    reval = "single";
                    break;
                case "smalldatetime":
                    reval = "DateTime?";
                    break;
                case "smallint":
                    reval = "int?";
                    break;
                case "smallmoney":
                    reval = "decimal?";
                    break;
                case "timestamp":
                    reval = "DateTime?";
                    break;
                case "tinyint":
                    reval = "byte";
                    break;
                case "uniqueidentifier":
                    reval = "System.Guid";
                    break;
                case "varbinary":
                    reval = "byte[]";
                    break;
                case "varchar":
                    reval = "string";
                    break;
                case "Variant":
                    reval = "Object";
                    break;
                case "varchar2":
                    reval = "string";
                    break;
                case "clob":
                    reval = "string";
                    break;
                case "nclob":
                    reval = "string";
                    break;
                case "blob":
                    reval = "string";
                    break;
                case "date":
                    reval = "DateTime?";
                    break;
                default:
                    reval = "string";
                    break;
            }
            return reval;
        }
       /// <summary>
       /// 如果是是number一类的 返回true
       /// </summary>
       /// <param name="type"></param>
       /// <returns></returns>
        private static bool CheckNumberType(string type)
        {
            bool? reval =null;
            switch (type.ToLower())
            {
                case "int":
                    reval = true;
                    break;
                case "text":
                    reval = false;
                    break;
                case "bigint":
                    reval = true;
                    break;
                case "binary":
                    reval = false;
                    break;
                case "bit":
                    reval = false;
                    break;
                case "char":
                    reval = false;
                    break;
                case "datetime":
                    reval = false;
                    break;
                case "decimal":
                    reval = true;
                    break;
                case "float":
                    reval = true;
                    break;
                case "image":
                    reval = false;
                    break;
                case "money":
                    reval = true;
                    break;
                case "nchar":
                    reval = false;
                    break;
                case "ntext":
                    reval = false;
                    break;
                //case "numeric":
                //    reval = "decimal?";
                //    break;
                case "numeric":
                    reval = true;
                    break;
                case "number":
                    reval = true;
                    break;
                case "nvarchar":
                    reval = false;
                    break;
                case "real":
                    reval = false;
                    break;
                case "smalldatetime":
                    reval = false;
                    break;
                case "smallint":
                    reval = true;
                    break;
                case "smallmoney":
                    reval = true;
                    break;
                case "timestamp":
                    reval = false;
                    break;
                case "tinyint":
                    reval = false;
                    break;
                case "uniqueidentifier":
                    reval = false;
                    break;
                case "varbinary":
                    reval = false;
                    break;
                case "varchar":
                    reval = false;
                    break;
                case "Variant":
                    reval = false;
                    break;
                case "varchar2":
                    reval = false;
                    break;
                case "clob":
                    reval = false;
                    break;
                case "nclob":
                    reval = false;
                    break;
                case "blob":
                    reval = false;
                    break;
                case "date":
                    reval = false;
                    break;
                default:
                    reval = false;
                    break;
            }
            return reval.Value;
        }

        /// <summary>
        /// 如果是是Datetime一类的 返回true
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool CheckDataTimeType(string type)
        {
            bool? reval = null;
            switch (type.ToLower())
            {
                case "timestamp":
                    reval = true;
                    break;
                case "date":
                    reval = true;
                    break;
                case "smalldatetime":
                    reval = true;
                    break;
                case "datetime":
                    reval = true;
                    break;
                default:
                    reval = false;
                    break;
            }
            return reval.Value;
        }
        #endregion

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            SolutionName = txtSolutionName.Text.ToString();
            ProxyBaseName = txtProxy.Text.ToString();
            SolutionAuth = txtAuthName.Text.ToString();

            DirSolutionPath = Path.Combine( txtDisk.Text, SolutionName);
            Init();
            if (Directory.Exists(DirSolutionPath)) { Directory.Delete(DirSolutionPath, true); }
            else Directory.CreateDirectory(DirSolutionPath);
            Dictionary<string, Dictionary<string, string>> r = new Dictionary<string, Dictionary<string, string>>();
            if (rdioSqlserver.Checked)
                r = GetDBInfo();
            else if(rdioOracle.Checked)
                r = GetOracleBInfo();
            EntityFactory(r);
            RuleFactory(r);
            IBusinessFactory(r);
            BusinessFactory(r);
            ProxyFactory(r);
            PresenterFactory(r);
            IInterfaceViewFactory(r);
            ViewModelFactory(r);
            //RuleFactoryFactory(r);
            //BaseRuleFactory(r);
            //PresenterBaseFactory(r);
            //FacadeProxyFactory(r);
            MessageBox.Show("生成完毕！");
        }
        //初始化变量
        public void Init()
        {
             RuleNameSpace = SolutionName + ".sDBRule";//DAL层命名空间（下同）
             EntitiesNameSpace = SolutionName + ".Entities";
             IBllNameSpace = SolutionName + ".IBusiness";
             sBllNameSpace = SolutionName + ".sBusiness";
             cBllNameSpace = SolutionName + ".cBusiness";

             sRuleLayerPath = Path.Combine(DirSolutionPath, RuleNameSpace);//dal层代码生成代码文件存放路径（下同）
             ModelLayerPath = Path.Combine(DirSolutionPath, EntitiesNameSpace);
             BllLayerPath = Path.Combine(DirSolutionPath, sBllNameSpace);
             IBllLayerPath = Path.Combine(DirSolutionPath, IBllNameSpace);
             cProxyLayerPath = Path.Combine(DirSolutionPath, cBllNameSpace, "FacadeProxy");
             cPresenterLayerPath = Path.Combine(DirSolutionPath, cBllNameSpace, "Presenters");
             cViewLayerPath = Path.Combine(DirSolutionPath, cBllNameSpace, "View");
             cViewModelLayerPath = Path.Combine(DirSolutionPath, cBllNameSpace, "ViewModels");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SolutionName = txtSolutionName.Text.ToString();
            ProxyBaseName = txtProxy.Text.ToString();
            SolutionAuth = txtAuthName.Text.ToString();

            DirSolutionPath = Path.Combine(txtDisk.Text, SolutionName);
            Init();
            if (Directory.Exists(DirSolutionPath)) { Directory.Delete(DirSolutionPath, true); }
            Dictionary<string, Dictionary<string, string>> r = new Dictionary<string, Dictionary<string, string>>();
            if (rdioSqlserver.Checked)
                r = GetDBInfo();
            else if (rdioOracle.Checked)
                r = GetOracleBInfo();
            //EntityFactory(r);
            //RuleFactory(r);
            //IBusinessFactory(r);
            //BusinessFactory(r);
            //ProxyFactory(r);
            //PresenterFactory(r);
            //IInterfaceViewFactory(r);
            //ViewModelFactory(r);
            RuleFactoryFactory(r);
            BaseRuleFactory(r);
            PresenterBaseFactory(r);
            FacadeProxyFactory(r);
            MessageBox.Show("生成完毕！");
        }
    }

    public static class ObjectEx
    {

        /// <summary>
        /// 将传输进来的字符串转换 主要是oracle 的列名和表名
        /// 比如：CONSUL_ID转换成ConsulId
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static string SplitUnderLine(this string fName)
        {
            var fieldsParts = fName.Split('_');
            string newFieldName = "";
            if (fieldsParts.Count() > 1)
                foreach (var fieldNamePart in fieldsParts)
                {
                    string ss = fieldNamePart.Substring(0, 1).ToUpper() + fieldNamePart.Substring(1, fieldNamePart.Length - 1).ToLower();
                    newFieldName = newFieldName + ss;
                }
            else
            {
                string ss = fName.Substring(0, 1).ToUpper() + fName.Substring(1, fName.Length - 1).ToLower();
                newFieldName = newFieldName + ss;
            }
            return newFieldName;
        }

        /// <summary>
        /// 将传输进来的字符串转换 主要是去掉前缀 
        /// 比如：HD_CONSUL_APPLY转换成ConsulApply
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static string SplitFirstUnderLine(this string fName)
        {
            var fieldsParts = fName.Split('_');
            string newFieldName = "";
            if (fieldsParts.Count() > 1)
            {
                int i = 0;
                foreach (var fieldNamePart in fieldsParts)
                {
                    if (i != 0)
                    {
                        string ss = fieldNamePart.Substring(0, 1).ToUpper() + fieldNamePart.Substring(1, fieldNamePart.Length - 1).ToLower();
                        newFieldName = newFieldName + ss;

                    }
                    i++;
                }
            }
            else
            {
                string ss = fName.Substring(0, 1).ToUpper() + fName.Substring(1, fName.Length - 1).ToLower();
                newFieldName = newFieldName + ss;
            }
            return newFieldName;
        }
        /// <summary>
        /// 去除字符串中的空格，回车，换行符，制表符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitSpecialChar(this string str)
        {
            return str.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
        }

    }

}
