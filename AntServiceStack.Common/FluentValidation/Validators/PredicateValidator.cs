namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using Attributes;
    using Internal;
    using Resources;
    using Results;

    public class PredicateValidator : PropertyValidator, IPredicateValidator {
        public delegate bool Predicate(object instanceToValidate, object propertyValue, PropertyValidatorContext propertyValidatorContext);

        private readonly Predicate predicate;

        public PredicateValidator(Predicate predicate) : base(() => Messages.predicate_error, ValidationErrors.Predicate) {
            predicate.Guard("A predicate must be specified.");
            this.predicate = predicate;
        }

        protected override bool IsValid(PropertyValidatorContext context) {
            if (!predicate(context.Instance, context.PropertyValue, context)) {
                return false;
            }

            return true;
        }
    }

    public interface IPredicateValidator : IPropertyValidator { }
}