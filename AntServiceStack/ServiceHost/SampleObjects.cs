using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AntServiceStack.WebHost.Endpoints.Metadata;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;

namespace AntServiceStack.ServiceHost
{
    public class SampleObjects
    {
        private static Dictionary<string, Dictionary<string, SampleMessage>> SampleMessageCache = new Dictionary<string, Dictionary<string, SampleMessage>>();

        private static SampleMessage _checkHealthSampleMessage;
        public static SampleMessage CheckHealthSampleMessage
        {
            get
            {
                if (_checkHealthSampleMessage == null)
                {
                    _checkHealthSampleMessage = PopulateSampleMessage(new SampleMessage(new CheckHealthRequestType(), new CheckHealthResponseType()));
                }
                return _checkHealthSampleMessage;
            }
        }

        protected static SampleMessage PopulateSampleMessage(SampleMessage message)
        {
            return new SampleMessage(ReflectionUtils.PopulateObject(message.Request), ReflectionUtils.PopulateObject(message.Response));
        }

        public static void RegisterSampleMessage(string servicePath, string operation, SampleMessage sampleMessage)
        {
            if (servicePath == null || operation == null || sampleMessage == null || sampleMessage.Request == null || sampleMessage.Response == null)
                return;

            Dictionary<string, SampleMessage> operationSampleMessage;
            SampleMessageCache.TryGetValue(servicePath, out operationSampleMessage);
            if (operationSampleMessage == null)
            {
                operationSampleMessage = new Dictionary<string, SampleMessage>();
                SampleMessageCache[servicePath] = operationSampleMessage;
            }

            operationSampleMessage[operation.Trim().ToLower()] = PopulateSampleMessage(sampleMessage);
        }

        public static SampleMessage GetSampleMessage(string servicePath, string operation)
        {
            if (servicePath == null || operation == null)
                return null;

            Dictionary<string, SampleMessage> operationSampleObjects;
            SampleMessageCache.TryGetValue(servicePath, out operationSampleObjects);
            if (operationSampleObjects == null)
                return null;

            SampleMessage objects;
            operationSampleObjects.TryGetValue(operation.Trim().ToLower(), out objects);
            return objects;
        }
    }
}
