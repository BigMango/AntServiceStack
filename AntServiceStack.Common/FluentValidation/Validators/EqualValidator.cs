namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Attributes;
    using Internal;
    using Resources;

    public class EqualValidator : PropertyValidator, IComparisonValidator {
        readonly Func<object, object> func;
        readonly IEqualityComparer comparer;

        public EqualValidator(object valueToCompare) : base(() => Messages.equal_error, ValidationErrors.Equal) {
            this.ValueToCompare = valueToCompare;
        }

        public EqualValidator(object valueToCompare, IEqualityComparer comparer)
            : base(() => Messages.equal_error, ValidationErrors.Equal)
        {
            ValueToCompare = valueToCompare;
            this.comparer = comparer;
        }

        public EqualValidator(Func<object, object> comparisonProperty, MemberInfo member)
            : base(() => Messages.equal_error, ValidationErrors.Equal)
        {
            func = comparisonProperty;
            MemberToCompare = member;
        }

        public EqualValidator(Func<object, object> comparisonProperty, MemberInfo member, IEqualityComparer comparer)
            : base(() => Messages.equal_error, ValidationErrors.Equal)
        {
            func = comparisonProperty;
            MemberToCompare = member;
            this.comparer = comparer;
        }

        protected override bool IsValid(PropertyValidatorContext context) {
            var comparisonValue = GetComparisonValue(context);
            bool success = Compare(comparisonValue, context.PropertyValue);

            if (!success) {
                context.MessageFormatter.AppendArgument("PropertyValue", comparisonValue);
                return false;
            }

            return true;
        }

        private object GetComparisonValue(PropertyValidatorContext context) {
            if(func != null) {
                return func(context.Instance);
            }

            return ValueToCompare;
        }

        public Comparison Comparison {
            get { return Comparison.Equal; }
        }

        public MemberInfo MemberToCompare { get; private set; }
        public object ValueToCompare { get; private set; }

        protected bool Compare(object comparisonValue, object propertyValue) {
            if(comparer != null) {
                return comparer.Equals(comparisonValue, propertyValue);
            }

            return Equals(comparisonValue, propertyValue);
        }
    }
}