namespace AntServiceStack.FluentValidation.TestHelper
{
    using System;

    [Serializable]
    public class ValidationTestException : Exception {
        public ValidationTestException(string message) : base(message) {
        }
    }
}