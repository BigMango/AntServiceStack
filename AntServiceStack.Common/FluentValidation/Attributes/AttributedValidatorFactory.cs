namespace AntServiceStack.FluentValidation.Attributes {
    using System;
    using Internal;

    /// <summary>
    /// Implementation of IValidatorFactory that looks for ValidatorAttribute instances on the specified type in order to provide the validator instance.
    /// </summary>
    public class AttributedValidatorFactory : IValidatorFactory {
        readonly InstanceCache cache = new InstanceCache();

        /// <summary>
        /// Gets a validator for the appropriate type.
        /// </summary>
        public IValidator<T> GetValidator<T>() {
            return (IValidator<T>)GetValidator(typeof(T));
        }

        /// <summary>
        /// Gets a validator for the appropriate type.
        /// </summary>
        public virtual IValidator GetValidator(Type type) {
            if (type == null)
                return null;

            var attribute = (ValidatorAttribute)Attribute.GetCustomAttribute(type, typeof(ValidatorAttribute));

            if (attribute == null || attribute.ValidatorType == null)
                return null;

            return cache.GetOrCreateInstance(attribute.ValidatorType) as IValidator;
        }
    }
}