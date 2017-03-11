using System;
using System.Linq;
using AntServiceStack.ServiceHost;
using AntServiceStack.FluentValidation;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.Validation
{
    public static class ValidationFilters
    {
        public static void RequestFilter(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            var validator = ValidatorCache.GetValidator(req, requestDto.GetType());
            if (validator == null) return;

            var validatorWithHttpRequest = validator as IRequiresHttpRequest;
            if (validatorWithHttpRequest != null)
                validatorWithHttpRequest.HttpRequest = req;

            var ruleSet = req.HttpMethod;
            var validationResult = validator.Validate(
                new ValidationContext(requestDto, null, new MultiRuleSetValidatorSelector(ruleSet)));

            if (validationResult.IsValid) return;

            // mark request validation exception
            res.ExecutionResult.ValidationExceptionThrown = true;

            // find response type
            Type responseType = string.IsNullOrEmpty(req.OperationName) ? null : EndpointHost.Config.MetadataMap[req.ServicePath].GetResponseTypeByOpName(req.OperationName); 

            var errorResponse = ErrorUtils.CreateValidationErrorResponse(
                req, validationResult.ToException(), responseType);

            var validationFeature = EndpointHost.GetPlugin<ValidationFeature>();
            if (validationFeature != null && validationFeature.ErrorResponseFilter != null)
            {
                errorResponse = validationFeature.ErrorResponseFilter(validationResult, errorResponse);
            }

            res.WriteToResponse(req, errorResponse);
        }
    }
}
