namespace AntServiceStack.FluentValidation
{
    /// <summary>
    /// Validator implementation that allows rules to be defined without inheriting from AbstractValidator.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Customer {
    ///   public int Id { get; set; }
    ///   public string Name { get; set; }
    /// 
    ///   public static readonly InlineValidator&lt;Customer&gt; Validator = new InlineValidator&lt;Customer&gt; {
    ///     v =&gt; v.RuleFor(x =&gt; x.Name).NotNull(),
    ///     v =&gt; v.RuleFor(x =&gt; x.Id).NotEqual(0),
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T"></typeparam>
    public class InlineValidator<T> : AbstractValidator<T> {
        /// <summary>
        /// Delegate that specifies configuring an InlineValidator.
        /// </summary>
        public delegate IRuleBuilderOptions<T, TProperty> InlineRuleCreator<TProperty>(InlineValidator<T> validator);

        /// <summary>
        /// Allows configuration of the validator.
        /// </summary>
        public void Add<TProperty>(InlineRuleCreator<TProperty> ruleCreator) {
            ruleCreator(this);
        }
    }
}