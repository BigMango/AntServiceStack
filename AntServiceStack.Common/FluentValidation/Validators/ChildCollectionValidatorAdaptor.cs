namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;
    using Results;

    public class ChildCollectionValidatorAdaptor : NoopPropertyValidator {
        readonly IValidator childValidator;

        public IValidator Validator {
            get { return childValidator; }
        }

        public Func<object, bool> Predicate { get; set; }

        public ChildCollectionValidatorAdaptor(IValidator childValidator) {
            this.childValidator = childValidator;
        }

        public override IEnumerable<ValidationFailure> Validate(PropertyValidatorContext context) {
            if (context.Rule.Member == null) {
                throw new InvalidOperationException(string.Format("Nested validators can only be used with Member Expressions."));
            }

            var collection = context.PropertyValue as IEnumerable;

            if (collection == null) {
                yield break;
            }

            int count = 0;
            
            var predicate = Predicate ?? (x => true);

            foreach (var element in collection) {

                if(element == null || !(predicate(element))) {
                    // If an element in the validator is null then we want to skip it to prevent NullReferenceExceptions in the child validator.
                    // We still need to update the counter to ensure the indexes are correct.
                    count++;
                    continue;
                }

                var newContext = context.ParentContext.CloneForChildValidator(element);
                newContext.PropertyChain.Add(context.Rule.Member);
                newContext.PropertyChain.AddIndexer(count++);

                var results = childValidator.Validate(newContext).Errors;

                foreach (var result in results) {
                    yield return result;
                }
            }
        }
    }
}