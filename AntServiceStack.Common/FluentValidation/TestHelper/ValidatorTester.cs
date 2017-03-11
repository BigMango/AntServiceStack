namespace AntServiceStack.FluentValidation.TestHelper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    public class ValidatorTester<T, TValue> where T : class {
        private readonly IValidator<T> validator;
        private readonly TValue value;
        private readonly MemberInfo member;

        public ValidatorTester(Expression<Func<T, TValue>> expression, IValidator<T> validator, TValue value) {
            this.validator = validator;
            this.value = value;
            member = expression.GetMember();
        }


        public void ValidateNoError(T instanceToValidate) {
            SetValue(instanceToValidate);

            var count = validator.Validate(instanceToValidate).Errors.Count(x => x.PropertyName == member.Name);

            if (count > 0) {
                throw new ValidationTestException(string.Format("Expected no validation errors for property {0}", member.Name));
            }
        }

        public void ValidateError(T instanceToValidate) {
            SetValue(instanceToValidate);
            var count = validator.Validate(instanceToValidate).Errors.Count(x => x.PropertyName == member.Name);

            if (count == 0) {
                throw new ValidationTestException(string.Format("Expected a validation error for property {0}", member.Name));
            }
        }

        private void SetValue(object instance) {
            var property = member as PropertyInfo;
            if (property != null) {
                property.SetValue(instance, value, null);
                return;
            }

            var field = member as FieldInfo;
            if (field != null) {
                field.SetValue(instance, value);
            }
        }
    }
}