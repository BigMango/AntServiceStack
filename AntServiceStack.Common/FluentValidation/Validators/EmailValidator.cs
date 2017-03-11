namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Text.RegularExpressions;
    using Attributes;
    using Internal;
    using Resources;
    using Results;

    //Email regex from http://hexillion.com/samples/#Regex
    public class EmailValidator : PropertyValidator, IRegularExpressionValidator, IEmailValidator {
        private readonly Regex regex;
        const string expression = @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$";

        public EmailValidator() : base(() => Messages.email_error, ValidationErrors.Email) {
            regex = new Regex(expression, RegexOptions.IgnoreCase);
        }


        protected override bool IsValid(PropertyValidatorContext context) {
            if (context.PropertyValue == null) return true;

            if (!regex.IsMatch((string)context.PropertyValue)) {
                return false;
            }

            return true;
        }

        public string Expression {
            get { return expression; }
        }
    }

    public interface IEmailValidator : IRegularExpressionValidator {
        
    }
}