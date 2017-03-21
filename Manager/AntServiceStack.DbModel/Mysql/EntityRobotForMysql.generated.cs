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
		/// �����
		/// </summary>
		public ITable<Service> Services { get { return this.Get<Service>(); } }

		private readonly IDataContext con;

		public ITable<T> Get<T>()
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
	/// �����
	/// </summary>
	[Table(Comment="�����", Name="services")]
	public partial class Service : BaseEntity
	{
		#region Column

		/// <summary>
		/// ����
		/// </summary>
		[Column("Tid",                 DataType=DataType.Int64,    Comment="����"), PrimaryKey, Identity]
		public long Tid { get; set; } // bigint(20)

		/// <summary>
		/// ������ʱ��
		/// </summary>
		[Column("DataChange_LastTime", DataType=DataType.DateTime, Comment="������ʱ��"), NotNull]
		public DateTime DataChangeLastTime // datetime
		{
			get { return _DataChangeLastTime; }
			set { _DataChangeLastTime = value; }
		}

		/// <summary>
		/// ��������
		/// </summary>
		[Column("ServiceName",         DataType=DataType.VarChar,  Length=50, Comment="��������"),    Nullable]
		public string ServiceName { get; set; } // varchar(50)

		/// <summary>
		/// �����ռ�
		/// </summary>
		[Column("Namespace",           DataType=DataType.VarChar,  Length=200, Comment="�����ռ�"),    Nullable]
		public string Namespace { get; set; } // varchar(200)

		/// <summary>
		/// ����
		/// </summary>
		[Column("Domain",              DataType=DataType.VarChar,  Length=50, Comment="����"),    Nullable]
		public string Domain { get; set; } // varchar(50)

		/// <summary>
		/// ״̬
		/// </summary>
		[Column("Status",              DataType=DataType.Int32,    Comment="״̬"), NotNull]
		public int Status { get; set; } // int(11)

		/// <summary>
		/// ά����Ա
		/// </summary>
		[Column("BusinessOwner",       DataType=DataType.VarChar,  Length=50, Comment="ά����Ա"),    Nullable]
		public string BusinessOwner { get; set; } // varchar(50)

		/// <summary>
		/// ������Ա
		/// </summary>
		[Column("TechOwner",           DataType=DataType.VarChar,  Length=50, Comment="������Ա"),    Nullable]
		public string TechOwner { get; set; } // varchar(50)

		/// <summary>
		/// �������� 0����NET 1����JAVA
		/// </summary>
		[Column("Type",                DataType=DataType.Int32,    Comment="�������� 0����NET 1����JAVA"), NotNull]
		public int Type { get; set; } // int(11)

		/// <summary>
		/// ������ע������
		/// </summary>
		[Column("FullName",            DataType=DataType.VarChar,  Length=100, Comment="������ע������"),    Nullable]
		public string FullName { get; set; } // varchar(100)

		#endregion

		#region Field

		private DateTime _DataChangeLastTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

		#endregion
	}

	public static partial class TableExtensions
	{
		public static Service FindByBk(this ITable<Service> table, long Tid)
		{
			return table.FirstOrDefault(t =>
				t.Tid == Tid);
		}

		public static async Task<Service> FindByBkAsync(this ITable<Service> table, long Tid)
		{
			return await table.FirstOrDefaultAsync(t =>
				t.Tid == Tid);
		}
	}
}
