namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Linq.Expressions;
    using Attributes;
    using Resources;

    public class LengthValidator : PropertyValidator, ILengthValidator {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public LengthValidator(int min, int max) : this(min, max, () => Messages.length_error) {
        }

        public LengthValidator(int min, int max, Expression<Func<string>> errorMessageResourceSelector) : base(errorMessageResourceSelector, ValidationErrors.Length) {
            Max = max;
            Min = min;

            if (max < min) {
                throw new ArgumentOutOfRangeException("max", "Max should be larger than min.");
            }

        }

        protected override bool IsValid(PropertyValidatorContext context) {
            int length = context.PropertyValue == null ? 0 : context.PropertyValue.ToString().Length;

            if (length < Min || length > Max) {
                context.MessageFormatter
                    .AppendArgument("MinLength", Min)
                    .AppendArgument("MaxLength", Max)
                    .AppendArgument("TotalLength", length);

                return false;
            }

            return true;
        }
    }

    public class ExactLengthValidator : LengthValidator {
        public ExactLengthValidator(int length) : base(length,length, () => Messages.exact_length_error) {
            
        }
    }

    public interface ILengthValidator : IPropertyValidator {
        int Min { get; }
        int Max { get; }
    }
}