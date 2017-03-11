namespace AntServiceStack.FluentValidation.Internal
{
    /// <summary>
    /// Determines whether or not a rule should execute.
    /// </summary>
    public interface IValidatorSelector {

        /// <summary>
        /// Determines whether or not a rule should execute.
        /// </summary>
        /// <param name="rule">The rule</param>
        /// <param name="propertyPath">Property path (eg Customer.Address.Line1)</param>
        /// <param name="context">Contextual information</param>
        /// <returns>Whether or not the validator can execute.</returns>
        bool CanExecute(IValidationRule rule, string propertyPath, ValidationContext context);
    }
}