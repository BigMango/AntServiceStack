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
  

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerFileAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public SwaggerFileAttribute()
        {
            
        }

        public SwaggerFileAttribute(string name):this()
        {
            Name = name;
        }

        public SwaggerFileAttribute(string name,string description):this(name)
        {
            Description = description;
        }

        public SwaggerFileAttribute(string name, string description, bool required)
            : this(name, description)
        {
            Required = required;
        }
    }
}