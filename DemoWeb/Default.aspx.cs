using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
		if (!Page.IsPostBack) {
			BindData();
		}
    }

	/// <summary>
	/// 绑定数据。
	/// </summary>
	private void BindData()
	{
		using (iS.DbHelper db = iS.DbHelper.Create()) {
			db.SetTableName = "T_User";
			db.SetPrimaryKey = "F_ID";
			db.SetFieldNames = "F_ID, F_Name";
			db.CreateSqlSelectString(iS.DbHelper.SortMode.NumericAsc);
			this.list.DataSource = db.GetDataSet();
			this.list.DataBind();
		}
	}
	
	/// <summary>
	/// 新增数据，并返回ID值。
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnAdd_Click(object sender, EventArgs e)
	{
		if (Page.IsValid) {
			using (iS.DbHelper db = iS.DbHelper.Create()) {
				//启动事务
				db.BeginTransaction();
				//执行过程
				int rows = db.ExecuteNonQuery("Insert T_User (F_Name) Values ('" + this.TextBox1.Text + "')");
				int id = Convert.ToInt32(db.ExecuteScalar("Select @@identity"));
				//提交事务
				if (db.Commit()) {
					this.result.Text = string.Format("此次执行插入了{0}条数据，返回ID值为{1}！", rows.ToString(), id.ToString());
				} else {
					//失败回滚事务
					db.Rollback();
					this.result.Text = "数据更新失败，请检查web.config中的ConnectionString是否配置正确！";
				}
			}
			//刷新列表
			BindData();
		}
	}
}
