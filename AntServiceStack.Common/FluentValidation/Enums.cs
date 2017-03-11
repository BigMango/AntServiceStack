namespace AntServiceStack.FluentValidation
{
    /// <summary>
    /// Specifies how rules should cascade when one fails.
    /// </summary>
    public enum CascadeMode {
        /// <summary>
        /// When a rule fails, execution continues to the next rule.
        /// </summary>
        Continue,
        /// <summary>
        /// When a rule fails, validation is stopped and all other rules in the chain will not be executed.
        /// </summary>
        StopOnFirstFailure
    }

    /// <summary>
    /// Specifies where a When/Unless condition should be applied
    /// </summary>
    public enum ApplyConditionTo {
        /// <summary>
        /// Applies the condition to all validators declared so far in the chain.
        /// </summary>
        AllValidators,
        /// <summary>
        /// Applies the condition to the current validator only.
        /// </summary>
        CurrentValidator
    }
}