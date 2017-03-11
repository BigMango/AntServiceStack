namespace AntServiceStack.FluentValidation.Results
{
    using System;

#if !SILVERLIGHT
    [Serializable]
#endif
    public class ValidationFailure {
        /// <summary>
        /// Creates a new validation failure.
        /// </summary>
        public ValidationFailure(string propertyName, string error, string errorCode) : this(propertyName, error, errorCode, null) {
        }

        /// <summary>
        /// Creates a new ValidationFailure.
        /// </summary>
        public ValidationFailure(string propertyName, string error, string errorCode, object attemptedValue) {
            PropertyName = propertyName;
            ErrorMessage = error;
            AttemptedValue = attemptedValue;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; private set; }
        
        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The error code
        /// </summary>
        public string ErrorCode { get; private set; }
        
        /// <summary>
        /// The property value that caused the failure.
        /// </summary>
        public object AttemptedValue { get; private set; }
        
        /// <summary>
        /// Custom state associated with the failure.
        /// </summary>
        public object CustomState { get; set; }

        /// <summary>
        /// Creates a textual representation of the failure.
        /// </summary>
        public override string ToString() {
            return ErrorMessage;
        }
    }
}