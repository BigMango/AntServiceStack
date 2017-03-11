using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.FluentValidation
{
    public static class CNLanguageResource
    {
        public static string email_error
        {
            get
            {
                return "'{PropertyName}'表示的邮件地址非法.";
            }
        }

        public static string equal_error
        {
            get
            {
                return "'{PropertyName}' 应该等于 '{ComparisonValue}' .";
            }
        }

        public static string exact_length_error
        {
            get
            {
                return "'{PropertyName}' 的长度必须是 {MaxLength} . 你已输入 {TotalLength} 个字符.";
            }
        }

        public static string exclusivebetween_error
        {
            get
            {
                return "'{PropertyName}' 不允许出现在 {From} 到 {To} 之间. 你输入了： {Value} .";
            }
        }

        public static string greaterthan_error
        {
            get
            {
                return "'{PropertyName}' 必须大于 '{ComparisonValue}' .";
            }
        }

        public static string greaterthanorequal_error
        {
            get
            {
                return "'{PropertyName}' 必须大于或等于 '{ComparisonValue}' .";
            }
        }

        public static string inclusivebetween_error
        {
            get
            {
                return "'{PropertyName}' 必须在 {From} 到 {To} 之间. 你输入了： {Value} .";
            }
        }

        public static string length_error
        {
            get
            {
                return "'{PropertyName}' 的长度必须在 {MinLength} 到 {MaxLength} 之间.您输入了 {TotalLength} 字符.";
            }
        }

        public static string lessthan_error
        {
            get
            {
                return "'{PropertyName}' 必须小于 '{ComparisonValue}' .";
            }
        }

        public static string lessthanorequal_error
        {
            get
            {
                return "'{PropertyName}' 必须小于或等于 '{ComparisonValue}' .";
            }
        }

        public static string notempty_error
        {
            get
            {
                return "'{PropertyName}' 不能为空字符串.";
            }
        }

        public static string notequal_error
        {
            get
            {
                return "'{PropertyName}' 不等于 '{ComparisonValue}' .";
            }
        }

        public static string notnull_error
        {
            get
            {
                return "'{PropertyName}' 未定义.";
            }
        }

        public static string predicate_error
        {
            get
            {
                return "'{PropertyName}' 不符合指定条件.";
            }
        }

        public static string regex_error
        {
            get
            {
                return "'{PropertyName}' 格式不正确.";
            }
        }
    }
}
