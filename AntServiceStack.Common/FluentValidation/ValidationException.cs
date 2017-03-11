using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.Validation;

namespace AntServiceStack.FluentValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Results;

    public class ValidationException : ArgumentException
    {
        public IEnumerable<ValidationFailure> Errors { get; private set; }

        public ValidationException(IEnumerable<ValidationFailure> errors) : base(BuildErrorMesage(errors)) {
            Errors = errors;
        }

        private static string BuildErrorMesage(IEnumerable<ValidationFailure> errors) {
            var arr = errors.Select(x => "\r\n -- " + x.ErrorMessage).ToArray();
            return "Validation failed: " + string.Join("", arr);
        }

        /*
        public ErrorDataType ToErrorData()
        {
            var errors = Errors.ConvertAll(x =>
                new ValidationErrorField(x.ErrorCode, x.PropertyName, x.ErrorMessage));

            var errorData = ErrorUtils.CreateErrorData(typeof(ValidationException).Name, Message, errors);
            return errorData;
        }
        */
    }
}