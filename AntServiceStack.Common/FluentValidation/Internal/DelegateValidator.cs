namespace AntServiceStack.FluentValidation.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Results;
    using Validators;

    /// <summary>
    /// Custom IValidationRule for performing custom logic.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateValidator<T> : IValidationRule {
        private readonly Func<T, ValidationContext<T>, IEnumerable<ValidationFailure>> func;
        
        /// <summary>
        /// Rule set to which this rule belongs.
        /// </summary>
        public string RuleSet { get; set; }

        /// <summary>
        /// Creates a new DelegateValidator using the specified function to perform validation.
        /// </summary>
        public DelegateValidator(Func<T, ValidationContext<T>, IEnumerable<ValidationFailure>> func) {
            this.func = func;
        }

        /// <summary>
        /// Creates a new DelegateValidator using the specified function to perform validation.
        /// </summary>
        public DelegateValidator(Func<T, IEnumerable<ValidationFailure>> func) {
            this.func = (x, ctx) => func(x);
        }

        /// <summary>
        /// Performs validation using a validation context and returns a collection of Validation Failures.
        /// </summary>
        /// <param name="context">Validation Context</param>
        /// <returns>A collection of validation failures</returns>
        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context) {
            return func(context.InstanceToValidate, context);
        }

        /// <summary>
        /// The validators that are grouped under this rule.
        /// </summary>
        public IEnumerable<IPropertyValidator> Validators {
            get { yield break; }
        }

        /// <summary>
        /// Performs validation using a validation context and returns a collection of Validation Failures.
        /// </summary>
        /// <param name="context">Validation Context</param>
        /// <returns>A collection of validation failures</returns>
        public IEnumerable<ValidationFailure> Validate(ValidationContext context) {
            if (!context.Selector.CanExecute(this, "", context)) {
                return Enumerable.Empty<ValidationFailure>();
            }

            var newContext = new ValidationContext<T>((T)context.InstanceToValidate, context.PropertyChain, context.Selector);
            return Validate(newContext);
        }
    }
}