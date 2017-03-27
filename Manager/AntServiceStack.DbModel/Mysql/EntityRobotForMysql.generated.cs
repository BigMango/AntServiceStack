using System;
using System.Linq;
using System.Threading.Tasks;

using AntData.ORM;
using AntData.ORM.Linq;
using AntData.ORM.Mapping;

namespace AntServiceStack.DbModel
{
	/// <summary>
	/// Database       : antsoa
	/// Data Source    : 127.0.0.1
	/// Server Version : 5.6.26-log
	/// </summary>
	public partial class Entitys : IEntity
	{
		/// <summary>
		/// 服务节点
		/// </summary>
		public IQueryable<Node>    Nodes    { get { return this.Get<Node>(); } }
		/// <summary>
		/// 服务表
		/// </summary>
		public IQueryable<Service> Services { get { return this.Get<Service>(); } }

		private readonly IDataContext con;

		public IQueryable<T> Get<T>()
			 where T : class
		{
			return this.con.GetTable<T>();
		}

		public Entitys(IDataContext con)
		{
			this.con = con;
		}
	}

	/// <summary>
	/// 服务节点
	/// </summary>
	[Table(Comment="服务节点", Name="nodes")]
	public partial class Node : BaseEntity
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                 DataType=DataType.Int64,    Comment="主键"), PrimaryKey, Identity]
		public long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChange_LastTime", DataType=DataType.DateTime, Comment="最后更新时间"), NotNull]
		public DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 地址
		/// </summary>
		[Column("Url",                 DataType=DataType.VarChar,  Length=100, Comment="地址"),    Nullable]
		public string Url { get; set; } // varchar(100)

		/// <summary>
		/// 说明
		/// </summary>
		[Column("Description",         DataType=DataType.VarChar,  Length=200, Comment="说明"),    Nullable]
		public string Description { get; set; } // varchar(200)

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",            DataType=DataType.Boolean,  Comment="是否可用"), NotNull]
		public bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 服务全名称
		/// </summary>
		[Column("ServiceFullName",     DataType=DataType.VarChar,  Length=100, Comment="服务全名称"),    Nullable]
		public string ServiceFullName { get; set; } // varchar(100)

		/// <summary>
		/// 服务类型(0自注册 1consu)
		/// </summary>
		[Column("Type",                DataType=DataType.Int32,    Comment="服务类型(0自注册 1consu)"), NotNull]
		public int Type { get; set; } // int(1)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	/// <summary>
	/// 服务表
	/// </summary>
	[Table(Comment="服务表", Name="services")]
	public partial class Service : BaseEntity
	{
		#region Column

		/// <summary>
		/// 主键
		/// </summary>
		[Column("Tid",                 DataType=DataType.Int64,    Comment="主键"), PrimaryKey, Identity]
		public long Tid { get; set; } // bigint(20)

		/// <summary>
		/// 最后更新时间
		/// </summary>
		[Column("DataChange_LastTime", DataType=DataType.DateTime, Comment="最后更新时间"), NotNull]
		public DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// 服务名称
		/// </summary>
		[Column("ServiceName",         DataType=DataType.VarChar,  Length=50, Comment="服务名称"),    Nullable]
		public string ServiceName { get; set; } // varchar(50)

		/// <summary>
		/// 命名空间
		/// </summary>
		[Column("Namespace",           DataType=DataType.VarChar,  Length=200, Comment="命名空间"),    Nullable]
		public string Namespace { get; set; } // varchar(200)

		/// <summary>
		/// 部门
		/// </summary>
		[Column("Domain",              DataType=DataType.VarChar,  Length=50, Comment="部门"),    Nullable]
		public string Domain { get; set; } // varchar(50)

		/// <summary>
		/// 状态
		/// </summary>
		[Column("Status",              DataType=DataType.Int32,    Comment="状态"), NotNull]
		public int Status { get; set; } // int(11)

		/// <summary>
		/// 维护人员
		/// </summary>
		[Column("BusinessOwner",       DataType=DataType.VarChar,  Length=50, Comment="维护人员"),    Nullable]
		public string BusinessOwner { get; set; } // varchar(50)

		/// <summary>
		/// 开发人员
		/// </summary>
		[Column("TechOwner",           DataType=DataType.VarChar,  Length=50, Comment="开发人员"),    Nullable]
		public string TechOwner { get; set; } // varchar(50)

		/// <summary>
		/// 服务类型 0代表NET 1代表JAVA
		/// </summary>
		[Column("Type",                DataType=DataType.Int32,    Comment="服务类型 0代表NET 1代表JAVA"), NotNull]
		public int Type { get; set; } // int(11)

		/// <summary>
		/// 服务发现注册名称
		/// </summary>
		[Column("FullName",            DataType=DataType.VarChar,  Length=100, Comment="服务发现注册名称"),    Nullable]
		public string FullName { get; set; } // varchar(100)

		/// <summary>
		/// 是否可用
		/// </summary>
		[Column("IsActive",            DataType=DataType.Boolean,  Comment="是否可用"), NotNull]
		public bool IsActive { get; set; } // tinyint(1)

		/// <summary>
		/// 描述
		/// </summary>
		[Column("Description",         DataType=DataType.VarChar,  Length=200, Comment="描述"),    Nullable]
		public string Description { get; set; } // varchar(200)

		/// <summary>
		/// 产线
		/// </summary>
		[Column("SubDomain",           DataType=DataType.VarChar,  Length=50, Comment="产线"),    Nullable]
		public string SubDomain { get; set; } // varchar(50)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	public static partial class TableExtensions
	{
		public static Node FindByBk(this IQueryable<Node> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Node> FindByBkAsync(this IQueryable<Node> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}

		public static Service FindByBk(this IQueryable<Service> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Service> FindByBkAsync(this IQueryable<Service> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}
	}
}
