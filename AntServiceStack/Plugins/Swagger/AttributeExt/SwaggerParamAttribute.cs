//-----------------------------------------------------------------------
// <copyright file="ParamAttribute.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------
namespace AntServiceStackSwagger.AttributeExt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

  

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class SwaggerParamAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public SwaggerParamAttribute(string name) : this(name, string.Empty, false)
        {
        }
        public SwaggerParamAttribute(string name, string desc)
          : this(name, desc,false)
        {
        }

        public SwaggerParamAttribute(string name, string desc,bool required)
        {
            this.Name = name;
            this.Description = desc;
            this.Required = required;
        }
    }
}