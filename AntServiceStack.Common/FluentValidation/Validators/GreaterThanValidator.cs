namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Reflection;
    using Attributes;
    using Internal;
    using Resources;

    public class GreaterThanValidator : AbstractComparisonValidator {
        public GreaterThanValidator(IComparable value) : base(value, () => Messages.greaterthan_error, ValidationErrors.GreaterThan) {
        }

        public GreaterThanValidator(Func<object, object> valueToCompareFunc, MemberInfo member)
            : base(valueToCompareFunc, member, () => Messages.greaterthan_error, ValidationErrors.GreaterThan) {
        }

        public override bool IsValid(IComparable value, IComparable valueToCompare) {
            return value.CompareTo(valueToCompare) > 0;
        }

        public override Comparison Comparison {
            get { return Validators.Comparison.GreaterThan; }
        }
    }
}