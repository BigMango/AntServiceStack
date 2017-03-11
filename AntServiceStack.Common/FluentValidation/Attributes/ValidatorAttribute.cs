namespace AntServiceStack.FluentValidation.Attributes
{
    using System;

    /// <summary>
    /// Validator attribute to define the class that will describe the Validation rules
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidatorAttribute : Attribute
    {
        /// <summary>
        /// The type of the validator used to validate the current type.
        /// </summary>
        public Type ValidatorType { get; private set; }

        /// <summary>
        /// Creates an instance of the ValidatorAttribute allowing a validator type to be specified.
        /// </summary>
        public ValidatorAttribute(Type validatorType)
        {
            ValidatorType = validatorType;
        }
    }
}