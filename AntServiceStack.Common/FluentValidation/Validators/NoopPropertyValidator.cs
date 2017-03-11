namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Resources;
    using Results;

    public abstract class NoopPropertyValidator : IPropertyValidator {
        public IStringSource ErrorMessageSource {
            get { return null; }
            set { }
        }

        public string ErrorCode
        {
            get { return null; }
            set { }
        }

        public abstract IEnumerable<ValidationFailure> Validate(PropertyValidatorContext context);

        public virtual ICollection<Func<object, object>> CustomMessageFormatArguments {
            get { return new List<Func<object, object>>(); }
        }

        public virtual bool SupportsStandaloneValidation {
            get { return false; }
        }

        public Func<object, object> CustomStateProvider {
            get { return null; }
            set { }
        }
    }
}