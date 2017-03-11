namespace AntServiceStack.FluentValidation
{
    using System;

    /// <summary>
    /// Gets validators for a particular type.
    /// </summary>
    public interface IValidatorFactory {
        /// <summary>
        /// Gets the validator for the specified type.
        /// </summary>
        IValidator<T> GetValidator<T>();

        /// <summary>
        /// Gets the validator for the specified type.
        /// </summary>
        IValidator GetValidator(Type type);
    }
}