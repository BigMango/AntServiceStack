namespace AntServiceStack.FluentValidation
{
    using System;

    public abstract class ValidatorFactoryBase : IValidatorFactory {
        public IValidator<T> GetValidator<T>() {
            return (IValidator<T>)GetValidator(typeof(T));
        }

        public IValidator GetValidator(Type type) {
            var genericType = typeof(IValidator<>).MakeGenericType(type);
            return CreateInstance(genericType);
        }

        public abstract IValidator CreateInstance(Type validatorType);
    }
}