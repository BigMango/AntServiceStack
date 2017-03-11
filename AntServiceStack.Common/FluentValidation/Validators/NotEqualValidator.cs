namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Attributes;
    using Internal;
    using Resources;

    public class NotEqualValidator : PropertyValidator, IComparisonValidator {
        readonly IEqualityComparer comparer;
        readonly Func<object, object> func;

        public NotEqualValidator(Func<object, object> func, MemberInfo memberToCompare)
            : base(() => Messages.notequal_error, ValidationErrors.NotEqual) {
            this.func = func;
            MemberToCompare = memberToCompare;
        }

        public NotEqualValidator(Func<object, object> func, MemberInfo memberToCompare, IEqualityComparer equalityComparer)
            : base(() => Messages.notequal_error, ValidationErrors.NotEqual)
        {
            this.func = func;
            this.comparer = equalityComparer;
            MemberToCompare = memberToCompare;
        }

        public NotEqualValidator(object comparisonValue)
            : base(() => Messages.notequal_error, ValidationErrors.NotEqual)
        {
            ValueToCompare = comparisonValue;
        }

        public NotEqualValidator(object comparisonValue, IEqualityComparer equalityComparer)
            : base(() => Messages.notequal_error, ValidationErrors.NotEqual)
        {
            ValueToCompare = comparisonValue;
            comparer = equalityComparer;
        }

        protected override bool IsValid(PropertyValidatorContext context) {
            var comparisonValue = GetComparisonValue(context);
            bool success = !Compare(comparisonValue, context.PropertyValue);

            if (!success) {
                context.MessageFormatter.AppendArgument("PropertyValue", context.PropertyValue);
                return false;
            }

            return true;
        }

        private object GetComparisonValue(PropertyValidatorContext context) {
            if (func != null) {
                return func(context.Instance);
            }

            return ValueToCompare;
        }

        public Comparison Comparison {
            get { return Comparison.NotEqual; }
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