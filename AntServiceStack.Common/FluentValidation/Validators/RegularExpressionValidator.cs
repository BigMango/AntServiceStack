namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Text.RegularExpressions;
    using Attributes;
    using Internal;
    using Resources;
    using Results;

    public class RegularExpressionValidator : PropertyValidator, IRegularExpressionValidator {
        readonly string expression;
        readonly Regex regex;

        public RegularExpressionValidator(string expression) : base(() => Messages.regex_error, ValidationErrors.RegularExpression) {
            this.expression = expression;
            regex = new Regex(expression);

        }

        protected override bool IsValid(PropertyValidatorContext context) {
            if (context.PropertyValue != null && !regex.IsMatch((string)context.PropertyValue)) {
                return false;
            }
            return true;
        }

        public string Expression {
            get { return expression; }
        }
    }

    public interface IRegularExpressionValidator : IPropertyValidator {
        string Expression { get; }
    }
}