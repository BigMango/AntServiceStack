namespace AntServiceStack.FluentValidation.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Internal;

    public class PropertyValidatorContext
    {
        private readonly MessageFormatter messageFormatter = new MessageFormatter();
        private bool propertyValueSet;
        private object propertyValue;

        public ValidationContext ParentContext { get; private set; }
        public PropertyRule Rule { get; private set; }
        public string PropertyName { get; private set; }

        public string PropertyDescription
        {
            get { return Rule.GetDisplayName(); }
        }

        public object Instance
        {
            get { return ParentContext.InstanceToValidate; }
        }

        public MessageFormatter MessageFormatter
        {
            get { return messageFormatter; }
        }

        //Lazily load the property value
        //to allow the delegating validator to cancel validation before value is obtained
        public object PropertyValue
        {
            get
            {
                if (!propertyValueSet)
                {
                    propertyValue = Rule.PropertyFunc(Instance);
                    propertyValueSet = true;
                }

                return propertyValue;
            }
            set
            {
                propertyValue = value;
                propertyValueSet = true;
            }
        }

        public PropertyValidatorContext(ValidationContext parentContext, PropertyRule rule, string propertyName)
        {
            ParentContext = parentContext;
            Rule = rule;
            PropertyName = propertyName;
        }
    }
}