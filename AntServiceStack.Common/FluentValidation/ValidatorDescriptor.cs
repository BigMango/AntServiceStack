namespace AntServiceStack.FluentValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Internal;
    using Validators;

    /// <summary>
    /// Used for providing metadata about a validator.
    /// </summary>
    public class ValidatorDescriptor<T> : IValidatorDescriptor {
        protected IEnumerable<IValidationRule> Rules { get; private set; }

        public ValidatorDescriptor(IEnumerable<IValidationRule> ruleBuilders) {
            Rules = ruleBuilders;
        }

        public virtual string GetName(string property) {
            var nameUsed = Rules
                .OfType<PropertyRule>()
                .Where(x => x.Member.Name == property)
                .Select(x => x.GetDisplayName()).FirstOrDefault();

            return nameUsed;
        }

        public virtual ILookup<string, IPropertyValidator> GetMembersWithValidators() {
            var query = from rule in Rules.OfType<PropertyRule>()
                        where rule.Member != null
                        from validator in rule.Validators
                        select new { memberName = rule.Member.Name, validator };

            return query.ToLookup(x => x.memberName, x => x.validator);
        }

        public IEnumerable<IPropertyValidator> GetValidatorsForMember(string name) {
            return GetMembersWithValidators()[name];
        }

        public IEnumerable<IValidationRule> GetRulesForMember(string name) {
            var query = from rule in Rules.OfType<PropertyRule>()
                        where rule.Member.Name == name
                        select (IValidationRule)rule;

            return query.ToList();
        }

        public virtual string GetName(Expression<Func<T, object>> propertyExpression) {
            var member = propertyExpression.GetMember();

            if (member == null) {
                throw new ArgumentException(string.Format("Cannot retrieve name as expression '{0}' as it does not specify a property.", propertyExpression));
            }

            return GetName(member.Name);
        }
    }
}