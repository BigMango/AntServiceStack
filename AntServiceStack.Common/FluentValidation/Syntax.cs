namespace AntServiceStack.FluentValidation
{
    using System;
    using System.Collections.Generic;
    using Internal;
    using Validators;

    /// <summary>
    /// Rule builder that starts the chain
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IRuleBuilderInitial<T, out TProperty> : IFluentInterface, IRuleBuilder<T, TProperty>, IConfigurable<PropertyRule, IRuleBuilderInitial<T, TProperty>> {
    }

    /// <summary>
    /// Rule builder 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IRuleBuilder<T, out TProperty> : IFluentInterface {
        /// <summary>
        /// Associates a validator with this the property for this rule builder.
        /// </summary>
        /// <param name="validator">The validator to set</param>
        /// <returns></returns>
        IRuleBuilderOptions<T, TProperty> SetValidator(IPropertyValidator validator);

        /// <summary>
        /// Associates an instance of IValidator with the current property rule.
        /// </summary>
        /// <param name="validator">The validator to use</param>
        [Obsolete("This overload of SetValidator is no longer used. If you are trying to set a child validator for a collection, use SetCollectionValidator instead.")]
        IRuleBuilderOptions<T, TProperty> SetValidator(IValidator validator);

        /// <summary>
        /// Associates an instance of IValidator with the current property rule.
        /// </summary>
        /// <param name="validator">The validator to use</param>
        IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator);
    }


    /// <summary>
    /// Rule builder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IRuleBuilderOptions<T, out TProperty> : IRuleBuilder<T, TProperty>, IConfigurable<PropertyRule, IRuleBuilderOptions<T, TProperty>> {

    }
}