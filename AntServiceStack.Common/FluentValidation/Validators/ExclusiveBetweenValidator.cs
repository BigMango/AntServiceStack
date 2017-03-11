namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using Attributes;
    using Resources;

    public class ExclusiveBetweenValidator : PropertyValidator, IBetweenValidator {
        public ExclusiveBetweenValidator(IComparable from, IComparable to) : base(() => Messages.exclusivebetween_error, ValidationErrors.ExclusiveBetween) {
            To = to;
            From = from;

            if (to.CompareTo(from) == -1) {
                throw new ArgumentOutOfRangeException("to", "To should be larger than from.");
            }
        }

        public IComparable From { get; private set; }
        public IComparable To { get; private set; }


        protected override bool IsValid(PropertyValidatorContext context) {
            var propertyValue = (IComparable)context.PropertyValue;

            // If the value is null then we abort and assume success.
            // This should not be a failure condition - only a NotNull/NotEmpty should cause a null to fail.
            if (propertyValue == null) return true;

            if (propertyValue.CompareTo(From) <= 0 || propertyValue.CompareTo(To) >= 0) {

                context.MessageFormatter
                    .AppendArgument("From", From)
                    .AppendArgument("To", To)
                    .AppendArgument("Value", context.PropertyValue);

                return false;
            }
            return true;
        }
    }
}