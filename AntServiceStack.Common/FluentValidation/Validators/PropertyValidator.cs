namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Resources;
    using Results;

    public abstract class PropertyValidator : IPropertyValidator {
        private readonly List<Func<object, object>> customFormatArgs = new List<Func<object, object>>();
        private IStringSource errorSource;
        private string errorCode;

        public Func<object, object> CustomStateProvider { get; set; }

        public ICollection<Func<object, object>> CustomMessageFormatArguments {
            get { return customFormatArgs; }
        }

        protected PropertyValidator(string errorMessageResourceName, Type errorMessageResourceType, string errorCode) {
            this.errorSource = new LocalizedStringSource(errorMessageResourceType, errorMessageResourceName, new FallbackAwareResourceAccessorBuilder());
            this.errorCode = errorCode;
        }

        protected PropertyValidator(string errorMessage, string errorCode) {
            this.errorSource = new StaticStringSource(errorMessage);
            this.errorCode = errorCode;
        }

        protected PropertyValidator(Expression<Func<string>> errorMessageResourceSelector, string errorCode) {
            this.errorSource = LocalizedStringSource.CreateFromExpression(errorMessageResourceSelector, new FallbackAwareResourceAccessorBuilder());
            this.errorCode = errorCode;
        }

        public IStringSource ErrorMessageSource {
            get { return errorSource; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                errorSource = value;
            }
        }

        public string ErrorCode
        {
            get { return errorCode; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                errorCode = value;
            }
        }

        public virtual IEnumerable<ValidationFailure> Validate(PropertyValidatorContext context) {
            context.MessageFormatter.AppendPropertyName(context.PropertyDescription);

            if (!IsValid(context)) {
                return new[] { CreateValidationError(context) };
            }

            return Enumerable.Empty<ValidationFailure>();
        }

        protected abstract bool IsValid(PropertyValidatorContext context);

        /// <summary>
        /// Creates an error validation result for this validator.
        /// </summary>
        /// <param name="context">The validator context</param>
        /// <returns>Returns an error validation result.</returns>
        protected virtual ValidationFailure CreateValidationError(PropertyValidatorContext context) {
            context.MessageFormatter.AppendAdditionalArguments(
                customFormatArgs.Select(func => func(context.Instance)).ToArray()
            );

            string error = context.MessageFormatter.BuildMessage(errorSource.GetString());

            var failure = new ValidationFailure(context.PropertyName, error, errorCode, context.PropertyValue);

            if (CustomStateProvider != null) {
                failure.CustomState = CustomStateProvider(context.Instance);
            }

            return failure;
        }
    }
}