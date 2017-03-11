namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections.Generic;
    using Resources;
    using Results;

    /// <summary>
    /// A custom property validator.
    /// This interface should not be implemented directly in your code as it is subject to change.
    /// Please inherit from <see cref="PropertyValidator">PropertyValidator</see> instead.
    /// </summary>
    public interface IPropertyValidator
    {
        IEnumerable<ValidationFailure> Validate(PropertyValidatorContext context);
        ICollection<Func<object, object>> CustomMessageFormatArguments { get; }
        Func<object, object> CustomStateProvider { get; set; }
        IStringSource ErrorMessageSource { get; set; }
        string ErrorCode { get; set; }
    }
}