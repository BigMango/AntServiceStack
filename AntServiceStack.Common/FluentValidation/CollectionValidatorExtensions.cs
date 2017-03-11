namespace AntServiceStack.FluentValidation
{
    using System;
    using System.Collections.Generic;
    using Internal;
    using Validators;

    public static class CollectionValidatorExtensions {
        /// <summary>
        /// Associates an instance of IValidator with the current property rule and is used to validate each item within the collection.
        /// </summary>
        /// <param name="validator">The validator to use</param>
        public static ICollectionValidatorRuleBuilder<T, TCollectionElement> SetCollectionValidator<T, TCollectionElement>(this IRuleBuilder<T, IEnumerable<TCollectionElement>> ruleBuilder, IValidator<TCollectionElement> validator) {
            var adaptor = new ChildCollectionValidatorAdaptor(validator);
            ruleBuilder.SetValidator(adaptor);
            return new CollectionValidatorRuleBuilder<T, TCollectionElement>(ruleBuilder, adaptor);
        }

        public interface ICollectionValidatorRuleBuilder<T,TCollectionElement> : IRuleBuilderOptions<T, IEnumerable<TCollectionElement>> {
            ICollectionValidatorRuleBuilder<T,TCollectionElement> Where(Func<TCollectionElement, bool> predicate);
        }

        private class CollectionValidatorRuleBuilder<T,TCollectionElement> : ICollectionValidatorRuleBuilder<T,TCollectionElement> {
            IRuleBuilder<T, IEnumerable<TCollectionElement>> ruleBuilder;
            ChildCollectionValidatorAdaptor adaptor;

            public CollectionValidatorRuleBuilder(IRuleBuilder<T, IEnumerable<TCollectionElement>> ruleBuilder, ChildCollectionValidatorAdaptor adaptor) {
                this.ruleBuilder = ruleBuilder;
                this.adaptor = adaptor;
            }

            public IRuleBuilderOptions<T, IEnumerable<TCollectionElement>> SetValidator(IPropertyValidator validator) {
                return ruleBuilder.SetValidator(validator);
            }

            [Obsolete]
            public IRuleBuilderOptions<T, IEnumerable<TCollectionElement>> SetValidator(IValidator validator) {
                return ruleBuilder.SetValidator(validator);
            }

            public IRuleBuilderOptions<T, IEnumerable<TCollectionElement>> SetValidator(IValidator<IEnumerable<TCollectionElement>> validator) {
                return ruleBuilder.SetValidator(validator);
            }

            public IRuleBuilderOptions<T, IEnumerable<TCollectionElement>> Configure(Action<PropertyRule> configurator) {
                return ((IRuleBuilderOptions<T, IEnumerable<TCollectionElement>>)ruleBuilder).Configure(configurator);
            }

            public ICollectionValidatorRuleBuilder<T, TCollectionElement> Where(Func<TCollectionElement, bool> predicate) {
                predicate.Guard("Cannot pass null to Where.");
                adaptor.Predicate = x => predicate((TCollectionElement)x);
                return this;
            }
        }
    }
}