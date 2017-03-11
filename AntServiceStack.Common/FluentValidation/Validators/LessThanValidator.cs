namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Attributes;
    using Internal;
    using Resources;

    public class LessThanValidator : AbstractComparisonValidator {
        public LessThanValidator(IComparable value) : base(value, () => Messages.lessthan_error, ValidationErrors.LessThan) {
        }

        public LessThanValidator(Func<object, object> valueToCompareFunc, MemberInfo member)
            : base(valueToCompareFunc, member, () => Messages.lessthan_error, ValidationErrors.LessThan) {
        }

        public override bool IsValid(IComparable value, IComparable valueToCompare) {
            return value.CompareTo(valueToCompare) < 0;
        }

        public override Comparison Comparison {
            get { return Validators.Comparison.LessThan; }
        }
    }
}