//-----------------------------------------------------------------------
// <copyright file="ConditionBase.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------


using System.ComponentModel;

namespace AntServiceStack.Manager.Model.Condition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    /// <summary>
    /// 分页查询
    /// </summary>
    public class ConditionBase
    {
        private int pageSize;


        [Description("条数,默认10")]
        public int PageSize
        {
            get { return pageSize <= 0 ? 10 : pageSize; }
            set { pageSize = value; }
        }

        private int pageIndex ;

        [Description("页数,默认1")]
        public int PageIndex
        {
            get { return pageIndex <= 0 ?1 : pageIndex; }
            set { pageIndex = value; }
        }

        private string orderBy;

        [Description("排序字段")]
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        private string orderSequence;


        [Description("asc | desc")]
        public string OrderSequence
        {
            get { return string.IsNullOrEmpty(orderSequence)?"asc": orderSequence; }
            set { orderSequence = value; }
        }
    }
}