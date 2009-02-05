/*
初始作者：李萨（iSLee / iSLeeCN）
联系方式：isleecn@gmail.com
版权信息：可自由开发、分发
版本控制：
说    明：
    这是一个初期就设计成可支持多数据库类型的类库，目的是作为现在流传甚广的微软 SqlHelper 的完全替代品，
    采用和 SqlHelper 的静态调用方法所不同的方式，需要用工厂方法创建数据操作对象，具体代码如下：
    using (iS.DbHepler db = iS.DbHelper.Create()) {
      //do something...
    }
    优点是省去了 SqlHelper 需要反复调用 Connection String 的缺点，并保持了灵活性。同时扩展了查询及更新
    功能，可以以面向对象的方式操作数据库，并自动生成数据查询语句，具体请看演示。
*/
using System;
using System.Data;
using System.Collections;
using System.Configuration;

namespace iS
{
	/// <summary>
	/// DbHelper 数据库操作接口。
	/// </summary>
	public abstract class DbHelper : IDisposable
	{

		#region 枚举数据类型

		/// <summary>
		/// 数据库类型
		/// </summary>
		public enum DbServerType
		{
			SqlServer,
			Access,
			Oracle,
			MySQL,
			ODBC
		}

		/// <summary>
		/// 搜索模式
		/// </summary>
		public enum SortMode
		{
			NumericAsc, //数字型索引
			NumericDesc,
			CharacterAsc, //字符型索引
			CharacterDesc,
			Random, //随机排序
			None //不排序
		}

		/// <summary>
		/// 数据类型
		/// </summary>
		public enum DbDataType
		{
			Boolean,
			DateTime,
			Guid,
			Int16,
			Int32,
			Int64,
			Numeric,
			String,
			Text,
			None
		}

		#endregion

		#region 获取配置

		/// <summary>
		/// 获取数据库类型。
		/// 2008.12.08
		/// </summary>
		/// <returns></returns>
		public static DbServerType ConnectionServerType
		{
			get
			{
				string type = ConfigurationManager.ConnectionStrings["ConnectionType"].ToString();
				switch (type) {
					case "SqlServer":
						return iS.DbHelper.DbServerType.SqlServer;
					case "Access":
						return iS.DbHelper.DbServerType.Access;
					case "ODBC":
						return iS.DbHelper.DbServerType.ODBC;
					case "MySQL":
						return iS.DbHelper.DbServerType.MySQL;
					case "Oracle":
						return iS.DbHelper.DbServerType.Oracle;
					default:
						return iS.DbHelper.DbServerType.SqlServer;
				}
			}
		}

		/// <summary>
		/// 获取数据库连接字符串
		/// 2008.12.08
		/// </summary>
		/// <returns></returns>
		public static string ConnectionString
		{
			get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ToString(); }
		}

		#endregion

		#region 创建数据操作对象

		public static DbHelper Create(DbServerType dbServer, string connString)
		{
			switch (dbServer) {
				case DbServerType.SqlServer: return new iSBase.Db.SqlServerBase(connString);
				case DbServerType.Access: return new iSBase.Db.MSAccessBase(connString);
				case DbServerType.Oracle: throw new Exception("iS.DbHelper 类暂不支持此接口！数据访问类未建立。");
				case DbServerType.MySQL: throw new Exception("iS.DbHelper 类暂不支持此接口！数据访问类未建立。");
				case DbServerType.ODBC: throw new Exception("iS.DbHelper 类暂不支持此接口！数据访问类未建立。");
				default:
					break;
			}
			return null;
		}

		public static DbHelper Create()
		{
			return Create(ConnectionServerType, ConnectionString);
		}

		#endregion

		#region 基础数据操作方法

		/// <summary>
		/// 设置生成的SQL语句。
		/// </summary>
		public abstract string SqlString
		{
			get;
			set;
		}

		/// <summary>
		/// 打开数据库连接。
		/// </summary>
		public abstract void Open();

		/// <summary>
		/// 关闭数据库连接。
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// 销毁数据库连接。
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// 执行SQL语句，并返回受影响的行数。
		/// </summary>
		/// <returns></returns>
		public abstract int ExecuteNonQuery();
		public abstract int ExecuteNonQuery(string SqlStr);

		/// <summary>
		/// 查询数据库并返回第一行第一列的值，适用于 Select 查询。
		/// </summary>
		/// <returns></returns>
		public abstract object ExecuteScalar();
		public abstract object ExecuteScalar(string SqlStr);

		/// <summary>
		/// 统计数据行数
		/// </summary>
		/// <returns></returns>
		public abstract int GetRecordCount();
		public abstract int GetRecordCount(string tabName);
		public abstract int GetRecordCount(string tabName, string tabWhere);

		/// <summary>
		/// 查询数据库并返回 SqlDataReader 对象，只读向前，使用前必须手动打开数据库连接！
		/// </summary>
		/// <returns></returns>
		public abstract IDataReader GetDataReader();
		public abstract IDataReader GetDataReader(string SqlStr);

		/// <summary>
		/// 查询数据库并返回 DataSet 对象。
		/// </summary>
		/// <returns></returns>
		public abstract DataSet GetDataSet();
		public abstract DataSet GetDataSet(string SqlStr);

		#endregion

		#region 扩展的数据操作方法

		/// <summary>
		/// 设置 Sql 语句需要操作的数据表名称。
		/// </summary>
		private string tableName = String.Empty;
		public string SetTableName
		{
			set { tableName = value; }
		}
		public string GetTableName
		{
			get { return tableName; }
		}

		/// <summary>
		/// 设置 Sql 语句作为索引的主键，需要数字型字段且不能重复，在分页查询中将作为第二排序字段。
		/// </summary>
		private string primaryKey = null;
		public string SetPrimaryKey
		{
			set { primaryKey = value; }
		}
		public string GetPrimaryKey
		{
			get { return primaryKey; }
		}

		/// <summary>
		/// 设置 Sql 语句需要操作的字段。
		/// </summary>
		private string fieldNames = "*";
		public string SetFieldNames
		{
			set { fieldNames = value; }
		}
		public string GetFieldNames
		{
			get { return fieldNames; }
		}

		/// <summary>
		/// 设置 Sql 语句返回数据的行数，仅在 Select 时有效。
		/// </summary>
		private int pageSize = 10;
		public int SetPageSize
		{
			set { pageSize = value; }
		}
		public int GetPageSize
		{
			get { return pageSize; }
		}

		/// <summary>
		/// 设置 Sql 语句的执行条件。
		/// </summary>
		private string where = String.Empty;
		public string SetWhere
		{
			set { where = value; }
		}
		public string GetWhere
		{
			get { return where; }
		}

		/// <summary>
		/// 设置 Sql 语句返回数据的排序方式，仅在 Select 时有效。
		/// </summary>
		private string orderBy = String.Empty;
		public string SetOrderBy
		{
			set { orderBy = value; }
		}
		public string GetOrderBy
		{
			get { return orderBy; }
		}

		/// <summary>
		/// 设置 Sql 语句需要更新或插入的值，仅在更新或插入数据时才需要设置。
		/// </summary>
		public Hashtable Values = new Hashtable();

		/// <summary>
		/// 生成查询数据的 Sql 语句，必要设置：SetTableName，可选设置：SetWhere、SetOrderBy、SetPageSize默认值10、SetFieldNames默认值“*”。
		/// </summary>
		/// <returns></returns>
		public abstract void CreateSqlSelectString(SortMode sortMode);

		/// <summary>
		/// 生成快速分页查询数据的 Sql 语句，必要设置：SetTableName、SetPrimaryKey，可选设置：SetOrder、SetWhere、SetPageSize默认值10、SetFieldNames默认值“*”。生成的SQL语句将储存在SqlString字符串中。
		/// </summary>
		/// <param name="currentPage">当前页码</param>
		/// <param name="orderBy">主键排序方式。</param>
		/// <returns></returns>
		public abstract void CreateSqlSelectString(int currentPage, SortMode sortMode);

		/// <summary>
		/// 设置执行过程的值。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		public abstract void Add(string key, object val, DbDataType type);

		/// <summary>
		/// 获取新增数据行的 Sql 语句，必要设置：SetTableName、SetFieldNames、Values。
		/// </summary>
		/// <returns></returns>
		public abstract int Insert();

		/// <summary>
		/// 获取更新数据的 Sql 语句，必要设置：SetTableName、SetFieldNames，可选设置：SetWhere。
		/// </summary>
		/// <returns></returns>
		public abstract int Update();

		/// <summary>
		/// 获取并执行删除数据的 Sql 语句，必要设置：SetTableName、SetWhere。
		/// </summary>
		/// <returns></returns>
		public abstract int Delete();

		#endregion

		#region 事务处理

		/// <summary>
		/// 开始一个新的事务，此操作会将 DbHelper 转化为事务模式。
		/// </summary>
		public abstract void BeginTransaction();

		/// <summary>
		/// 提交事务。
		/// </summary>
		/// <returns></returns>
		public abstract bool Commit();

		/// <summary>
		/// 回滚事务。
		/// </summary>
		/// <returns></returns>
		public abstract bool Rollback();

		#endregion

		#region IDisposable 成员

		void IDisposable.Dispose()
		{
			this.Dispose();
		}

		#endregion
	}
}
