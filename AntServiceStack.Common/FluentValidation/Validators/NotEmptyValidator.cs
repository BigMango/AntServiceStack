namespace AntServiceStack.FluentValidation.Validators
{
    using System.Collections;
    using Resources;
    using System.Linq;

    public class NotEmptyValidator : PropertyValidator, INotEmptyValidator {
        readonly object defaultValueForType;

        public NotEmptyValidator(object defaultValueForType) : base(() => Messages.notempty_error, ValidationErrors.NotEmpty) {
            this.defaultValueForType = defaultValueForType;
        }

        protected override bool IsValid(PropertyValidatorContext context) {
            if (context.PropertyValue == null
                || IsInvalidString(context.PropertyValue)
                || IsEmptyCollection(context.PropertyValue)
                || Equals(context.PropertyValue, defaultValueForType)) {
                return false;
            }

            return true;
        }

        bool IsEmptyCollection(object propertyValue) {
            var collection = propertyValue as IEnumerable;
            return collection != null && !collection.Cast<object>().Any();
        }

        bool IsInvalidString(object value) {
            if (value is string) {
                return IsNullOrWhiteSpace(value as string);
            }
            return false;
        }

        bool IsNullOrWhiteSpace(string value) {
            if (value != null) {
                for (int i = 0; i < value.Length; i++) {
                    if (!char.IsWhiteSpace(value[i])) {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public interface INotEmptyValidator : IPropertyValidator {
    }
}