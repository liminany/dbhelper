/*
初始作者：李萨（iSLee / iSLeeCN）
联系方式：isleecn@gmail.com
版权信息：可自由开发、分发
说    明：
*/
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace iSBase.Db
{
	/// <summary>
	/// 这是一个基类，用于初始化数据连接，包含了通用的数据库操作方法。
	/// </summary>
	public class SqlServerBase : iS.DbHelper
	{
		#region 设置参数及类型

		//定义数据源
		protected SqlConnection conn = null;
		protected SqlCommand cmd = null;

		private string _SqlString = String.Empty; //最终生成的SQL语句
		private ArrayList _Values = new ArrayList(); //储存值的数据表

		/// <summary>
		/// 转换数据类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public SqlDbType GetDbType(DbDataType type)
		{
			switch (type) {
				case DbDataType.Boolean: return SqlDbType.Bit;
				case DbDataType.DateTime: return SqlDbType.DateTime;
				case DbDataType.Guid: return SqlDbType.UniqueIdentifier;
				case DbDataType.Int16: return SqlDbType.SmallInt;
				case DbDataType.Int32: return SqlDbType.Int;
				case DbDataType.Int64: return SqlDbType.BigInt;
				case DbDataType.Numeric: return SqlDbType.Decimal;
				case DbDataType.String: return SqlDbType.NVarChar;
				case DbDataType.Text: return SqlDbType.NText;
				case DbDataType.None: return SqlDbType.NVarChar;
				default: return SqlDbType.NVarChar;
			}
		}

		#endregion

		#region 基础数据操作方法

		/// <summary>
		/// 获取或设置生成的SQL语句。
		/// </summary>
		public override string SqlString
		{
			get { return _SqlString; }
			set { _SqlString = value; }
		}

		public SqlServerBase(string connString)
		{
			//连接数据源
			this.conn =  new SqlConnection(connString);
			this.cmd = conn.CreateCommand();
		}

		/// <summary>
		/// 打开数据库连接。
		/// </summary>
		public override void Open()
		{
			if (conn.State == System.Data.ConnectionState.Closed) {
				conn.Open();
			} else if (conn.State == System.Data.ConnectionState.Broken) {
				conn.Close();
				conn.Open();
			}
		}

		/// <summary>
		/// 关闭数据库连接。
		/// </summary>
		public override void Close()
		{
			if (conn != null) {
				if (conn.State != ConnectionState.Closed) { conn.Close(); }
			}
		}

		/// <summary>
		/// 销毁数据库连接。
		/// </summary>
		public override void Dispose()
		{
			if (conn != null) {
				if (conn.State != ConnectionState.Closed) { conn.Close(); }
				conn.Dispose();
			}
			//释放内存
			this._SqlString = String.Empty;
			this._Values.Clear();
		}

		/// <summary>
		/// 执行SQL语句，并返回受影响的行数。
		/// </summary>
		/// <returns></returns>
		public override int ExecuteNonQuery()
		{
			try {
				this.Open();
				this.cmd.CommandText = this.SqlString;
				return (int)this.cmd.ExecuteNonQuery();
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				//this.Close();
			}
		}

		/// <summary>
		/// 执行SQL语句，并返回受影响的行数。
		/// </summary>
		/// <param name="SqlStr">传入SQL语句</param>
		/// <returns></returns>
		public override int ExecuteNonQuery(string SqlStr)
		{
			try {
				this.Open();
				this.cmd.CommandText = SqlStr;
				return (int)this.cmd.ExecuteNonQuery();
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				//this.Close();
			}
		}

		/// <summary>
		/// 查询数据库并返回第一行第一列的值，适用于 Select 查询。
		/// </summary>
		/// <returns></returns>
		public override object ExecuteScalar()
		{
			try {
				this.Open();
				this.cmd.CommandText = this.SqlString;
				return this.cmd.ExecuteScalar();
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				//this.Close();
			}
		}

		/// <summary>
		/// 查询数据库并返回第一行第一列的值，适用于 Select 查询。
		/// </summary>
		/// <param name="SqlStr">传入SQL语句</param>
		/// <returns></returns>
		public override object ExecuteScalar(string SqlStr)
		{
			try {
				this.Open();
				this.cmd.CommandText = SqlStr;
				return this.cmd.ExecuteScalar();
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				//this.Close();
			}
		}

		/// <summary>
		/// 统计数据行数
		/// </summary>
		/// <returns></returns>
		public override int GetRecordCount()
		{
			string SqlStr = "SELECT Count(*) FROM " + this.GetWhere;
			if (this.GetWhere.Length > 0) { SqlStr += " WHERE " + this.GetWhere; }
			this.SqlString = SqlStr;
			return (int)this.ExecuteScalar();
		}

		/// <summary>
		/// 统计数据行数。
		/// 2008.12.09
		/// </summary>
		/// <param name="tabName">表名称</param>
		/// <param name="tabWhere">查询条件</param>
		/// <returns></returns>
		public override int GetRecordCount(string tabName)
		{
			string SqlStr = "SELECT Count(*) FROM " + tabName;
			this.SqlString = SqlStr;
			return (int)this.ExecuteScalar();
		}

		/// <summary>
		/// 统计数据行数。
		/// 2008.04.21
		/// </summary>
		/// <param name="tabName">表名称</param>
		/// <param name="tabWhere">查询条件</param>
		/// <returns></returns>
		public override int GetRecordCount(string tabName, string tabWhere)
		{
			string SqlStr = "SELECT Count(*) FROM " + tabName;
			if (tabWhere.Length > 0) { SqlStr += " WHERE " + tabWhere; }
			this.SqlString = SqlStr;
			return (int)this.ExecuteScalar();
		}

		/// <summary>
		/// 查询数据库并返回 SqlDataReader 对象，只读向前，使用后需要手动关闭连接！
		/// 2008.04.21
		/// </summary>
		/// <returns></returns>
		public override IDataReader GetDataReader()
		{
			try {
				this.Open();
				this.cmd.CommandText = this.SqlString;
				IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				return dr;
			} catch (SqlException ex) {
				throw (ex);
			}
		}

		/// <summary>
		/// 查询数据库并返回 SqlDataReader 对象，只读向前，使用后需要手动关闭连接！
		/// 2008.04.21
		/// </summary>
		/// <param name="SqlStr">传入SQL语句</param>
		/// <returns></returns>
		public override IDataReader GetDataReader(string SqlStr)
		{
			try {
				this.Open();
				this.cmd.CommandText = SqlStr;
				IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				return dr;
			} catch (SqlException ex) {
				throw (ex);
			}
		}

		/// <summary>
		/// 查询数据库并返回 DataSet 对象，只读向前。
		/// 2008.04.21
		/// </summary>
		/// <returns></returns>
		public override DataSet GetDataSet()
		{
			DataSet ds = new DataSet();
			SqlDataAdapter da = new SqlDataAdapter();
			try {
				this.Open();
				this.cmd.CommandText = this.SqlString;
				da.SelectCommand = this.cmd;
				da.Fill(ds);
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				this.Close();
			}
			return ds;
		}

		/// <summary>
		/// 查询数据库并返回 DataSet 对象，只读向前。
		/// 2008.04.21
		/// </summary>
		/// <param name="SqlStr">传入SQL语句</param>
		/// <returns></returns>
		public override DataSet GetDataSet(string SqlStr)
		{
			DataSet ds = new DataSet();
			SqlDataAdapter da = new SqlDataAdapter();
			try {
				this.Open();
				this.cmd.CommandText = SqlStr;
				da.SelectCommand = this.cmd;
				da.Fill(ds);
			} catch (SqlException ex) {
				throw (ex);
			} finally {
				this.Close();
			}
			return ds;
		}

		/// <summary>
		/// 生成查询 Sql 语句。
		/// 2008.04.21
		/// </summary>
		/// <param name="sortMode">排序模式</param>
		public override void CreateSqlSelectString(SortMode sortMode)
		{
			string SqlStr = "SELECT ";
			if (this.GetPageSize > 0) {
				SqlStr += string.Format("TOP {0} {1}", this.GetPageSize.ToString(), this.GetFieldNames);
			} else {
				SqlStr += this.GetFieldNames;
			}
			SqlStr += string.Format(" FROM {0}", this.GetTableName);
			if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
			switch (sortMode) {
				case SortMode.CharacterAsc:
					if (this.GetOrderBy.Length > 0)
						SqlStr += string.Format(" ORDER BY {0}, {1}", this.GetOrderBy, this.GetPrimaryKey);
					else
						SqlStr += string.Format(" ORDER BY {0}", this.GetPrimaryKey);
					break;

				case SortMode.CharacterDesc:
					if (this.GetOrderBy.Length > 0)
						SqlStr += string.Format(" ORDER BY {0} DESC, {1} DESC", this.GetOrderBy, this.GetPrimaryKey);
					else
						SqlStr += string.Format(" ORDER BY {0} DESC", this.GetPrimaryKey);
					break;

				case SortMode.NumericAsc:
					if (this.GetOrderBy.Length > 0)
						SqlStr += string.Format(" ORDER BY {0}, {1}", this.GetOrderBy, this.GetPrimaryKey);
					else
						SqlStr += string.Format(" ORDER BY {0}", this.GetPrimaryKey);
					break;

				case SortMode.NumericDesc:
					if (this.GetOrderBy.Length > 0)
						SqlStr += string.Format(" ORDER BY {0} DESC, {1} DESC", this.GetOrderBy, this.GetPrimaryKey);
					else
						SqlStr += string.Format(" ORDER BY {0} DESC", this.GetPrimaryKey);
					break;

				case SortMode.Random:
					if (this.GetOrderBy.Length > 0)
						SqlStr += string.Format(" ORDER BY {0}, NewID()", this.GetOrderBy);
					else
						SqlStr += " ORDER BY NewID()";
					break;

				case SortMode.None:
					this.SetOrderBy = String.Empty;
					break;
			}
			this.SqlString = SqlStr;
		}

		/// <summary>
		/// 生成分页查询的 Sql 语句。
		/// 2008.05.07
		/// </summary>
		/// <param name="currentPage">当前页码</param>
		/// <param name="sortMode">排序模式</param>
		public override void CreateSqlSelectString(int currentPage, SortMode sortMode)
		{
			if (currentPage > 1) {
				string SqlStr = "SELECT ";
				SqlStr += string.Format("TOP {0} {1} FROM {2} WHERE", this.GetPageSize.ToString(), this.GetFieldNames, this.GetTableName);
				if (this.GetWhere.Length > 0) { SqlStr += string.Format(" {0} and", this.GetWhere); }

				switch (sortMode) {
					case SortMode.CharacterAsc:
						SqlStr += string.Format(" {0} not in (SELECT TOP {1} {0} FROM {2}", this.GetPrimaryKey, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
						if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
						if (this.GetOrderBy.Length > 0)
							SqlStr += string.Format(" ORDER BY {0}, {1}) ORDER BY {0}, {1}", this.GetOrderBy, this.GetPrimaryKey);
						else
							SqlStr += string.Format(" ORDER BY {0}) ORDER BY {0}", this.GetPrimaryKey);
						this.SqlString = SqlStr;
						break;

					case SortMode.CharacterDesc:
						SqlStr += string.Format(" {0} not in (SELECT TOP {1} {0} FROM {2}", this.GetPrimaryKey, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
						if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
						if (this.GetOrderBy.Length > 0)
							SqlStr += string.Format(" ORDER BY {0} DESC, {1} DESC) ORDER BY {0} DESC, {1} DESC", this.GetOrderBy, this.GetPrimaryKey);
						else
							SqlStr += string.Format(" ORDER BY {0} DESC) ORDER BY {0} DESC", this.GetPrimaryKey);
						this.SqlString = SqlStr;
						break;

					case SortMode.NumericAsc:
						if (this.GetOrderBy.Length > 0) {
							SqlStr += string.Format(" {0} > (SELECT Max({0}) FROM (SELECT TOP {1} {0} FROM {2}", this.GetOrderBy, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
							if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
							SqlStr += string.Format(" ORDER BY {0}, {1}) As TmpTable) ORDER BY {0}, {1}", this.GetOrderBy, this.GetPrimaryKey);
						} else {
							SqlStr += string.Format(" {0} > (SELECT Max({0}) FROM (SELECT TOP {1} {0} FROM {2}", this.GetPrimaryKey, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
							if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
							SqlStr += string.Format(" ORDER BY {0}) As TmpTable) ORDER BY {0}", this.GetPrimaryKey);
						}
						this.SqlString = SqlStr;
						break;

					case SortMode.NumericDesc:
						if (this.GetOrderBy.Length > 0) {
							SqlStr += string.Format(" {0} < (SELECT Min({0}) FROM (SELECT TOP {1} {0} FROM {2}", this.GetOrderBy, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
							if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
							SqlStr += string.Format(" ORDER BY {0} DESC, {1} DESC) As TmpTable) ORDER BY {0} DESC, {1} DESC", this.GetOrderBy, this.GetPrimaryKey);
						} else {
							SqlStr += string.Format(" {0} < (SELECT Min({0}) FROM (SELECT TOP {1} {0} FROM {2}", this.GetPrimaryKey, currentPage * this.GetPageSize - this.GetPageSize, this.GetTableName);
							if (this.GetWhere.Length > 0) { SqlStr += string.Format(" WHERE {0}", this.GetWhere); }
							SqlStr += string.Format(" ORDER BY {0} DESC) As TmpTable) ORDER BY {0} DESC", this.GetPrimaryKey);
						}
						this.SqlString = SqlStr;
						break;

					case SortMode.Random:
						this.CreateSqlSelectString(sortMode);
						break;

					case SortMode.None:
						this.CreateSqlSelectString(sortMode);
						break;

					default:
						this.SqlString = SqlStr;
						break;

				}
			} else {
				this.CreateSqlSelectString(sortMode);
			}
		}

		/// <summary>
		/// 生成删除 Sql 语句并执行。
		/// 2008.12.09
		/// </summary>
		/// <returns></returns>
		public override int Delete()
		{
			if (this.GetWhere.Length > 0) {
				this.SqlString = "DELETE FROM " + this.GetTableName + " WHERE " + this.GetWhere;
				return this.ExecuteNonQuery();
			} else {
				throw new Exception("为了避免错误删除全表数据，必须使用 SetWhere 设置查询条件，若确实需要删除全表数据可使用“1=1”。");
			}
		}

		/// <summary>
		/// 将值加入数据表。
		/// 2008.05.05
		/// </summary>
		/// <param name="key">数据键名称</param>
		/// <param name="val">值</param>
		/// <param name="type">值类型</param>
		public override void Add(string key, object val, DbDataType type)
		{
			ArrayList dr = new ArrayList();
			dr.Add(key);
			dr.Add(val);
			dr.Add(type);
			this._Values.Add(dr);
		}

		/// <summary>
		/// 将数据插入数据库，需要设置 SetTableName 的值。
		/// 2008.05.07
		/// </summary>
		public override int Insert()
		{
			if (this._Values.Count > 0) {
				//组合 Sql 语句
				bool firstRow = true;
				string tmpFieldStr = String.Empty, tmpValueStr = String.Empty;
				foreach (ArrayList dr in this._Values) {
					if (dr[0].ToString() != this.GetPrimaryKey) {
						if (firstRow) {
							tmpFieldStr = dr[0].ToString();
							tmpValueStr = "@" + dr[0].ToString();
							firstRow = false;
						} else {
							tmpFieldStr += "," + dr[0].ToString();
							tmpValueStr += ",@" + dr[0].ToString();
						}
					}
				}

				//传入存储过程变量
				foreach (ArrayList dr in this._Values) {
					DbDataType type = (DbDataType)dr[2];
					this.cmd.Parameters.Add("@" + dr[0].ToString(), this.GetDbType(type));
					this.cmd.Parameters["@" + dr[0].ToString()].Value = dr[1];
				}

				//储存 Sql 语句
				this.SqlString = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", this.GetTableName, tmpFieldStr, tmpValueStr);

				//执行结果
				return this.ExecuteNonQuery();
			} else {
				throw new Exception("在 Values 表中没有读取到数据，您需要使用 Add(string key, object val, DbDataType type) 方法添加数据。");
			}
		}

		/// <summary>
		/// 将数据更新至数据库，需要设置 SetTableName 及 SetWhere 的值。
		/// 2008.05.07
		/// </summary>
		public override int Update()
		{
			if (this._Values.Count > 0) {
				if (this.GetWhere.Length > 0) {
					//组合 Sql 语句
					bool firstRow = true;
					string tmpFieldStr = String.Empty;
					foreach (ArrayList dr in this._Values) {
						if (dr[0].ToString() != this.GetPrimaryKey) {
							if (firstRow) {
								tmpFieldStr = dr[0].ToString() + "=@" + dr[0].ToString();
								firstRow = false;
							} else {
								tmpFieldStr += "," + dr[0].ToString() + "=@" + dr[0].ToString();
							}
						}
					}

					//传入存储过程变量
					foreach (ArrayList dr in this._Values) {
						DbDataType type = (DbDataType)dr[2];
						this.cmd.Parameters.Add("@" + dr[0].ToString(), this.GetDbType(type));
						this.cmd.Parameters["@" + dr[0].ToString()].Value = dr[1];
					}

					//储存 Sql 语句
					this.SqlString = string.Format("UPDATE {0} SET {1} WHERE {2}", this.GetTableName, tmpFieldStr, this.GetWhere);

					//执行结果
					return this.ExecuteNonQuery();
				} else {
					throw new Exception("为了避免错误覆盖全表数据，必须设置 SetWhere 查询条件，若需要更新全表数据可使用“1=1”。");
				}
			} else {
				throw new Exception("在 Values 表中没有读取到数据，您需要使用 Add(string key, object val, DbDataType type) 方法添加数据。");
			}
		}

		#endregion

		#region 事务处理

		/// <summary>
		/// 开始一个新的事务，此操作会将 DbHelper 转化为事务模式。
		/// 2009.09.09
		/// </summary>
		public override void BeginTransaction()
		{
			this.Open();
			this.cmd.Transaction = this.conn.BeginTransaction();
		}

		/// <summary>
		/// 提交事务。
		/// 2009.09.09
		/// </summary>
		/// <returns></returns>
		public override bool Commit()
		{
			try {
				this.cmd.Transaction.Commit();
				return true;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// 回滚事务。
		/// 2009.09.09
		/// </summary>
		/// <returns></returns>
		public override bool Rollback()
		{
			try {
				this.cmd.Transaction.Rollback();
				return true;
			} catch {
				return false;
			}
		}

		#endregion

	}
}
