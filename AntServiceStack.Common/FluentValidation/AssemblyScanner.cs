namespace AntServiceStack.FluentValidation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class that can be used to find all the validators from a collection of types.
    /// </summary>
    public class AssemblyScanner : IEnumerable<AssemblyScanner.AssemblyScanResult> {
        readonly IEnumerable<Type> types;

        /// <summary>
        /// Creates a scanner that works on a sequence of types.
        /// </summary>
        public AssemblyScanner(IEnumerable<Type> types) {
            this.types = types;
        }

        /// <summary>
        /// Finds all the validators in the specified assembly.
        /// </summary>
        public static AssemblyScanner FindValidatorsInAssembly(Assembly assembly) {
            return new AssemblyScanner(assembly.GetExportedTypes());
        }

        /// <summary>
        /// Finds all the validators in the assembly containing the specified type.
        /// </summary>
        public static AssemblyScanner FindValidatorsInAssemblyContaining<T>() {
            return FindValidatorsInAssembly(typeof(T).Assembly);
        }

        private IEnumerable<AssemblyScanResult> Execute() {
            var openGenericType = typeof(IValidator<>);

            var query = from type in types
                        let interfaces = type.GetInterfaces()
                        let genericInterfaces = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType)
                        let matchingInterface = genericInterfaces.FirstOrDefault()
                        where matchingInterface != null
                        select new AssemblyScanResult(matchingInterface, type);

            return query;
        }

        /// <summary>
        /// Performs the specified action to all of the assembly scan results.
        /// </summary>
        public void ForEach(Action<AssemblyScanResult> action) {
            foreach(var result in this) {
                action(result);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<AssemblyScanResult> GetEnumerator() {
            return Execute().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Result of performing a scan.
        /// </summary>
        public class AssemblyScanResult {
            /// <summary>
            /// Creates an instance of an AssemblyScanResult.
            /// </summary>
            public AssemblyScanResult(Type interfaceType, Type validatorType) {
                InterfaceType = interfaceType;
                ValidatorType = validatorType;
            }

            /// <summary>
            /// Validator interface type, eg IValidator&lt;Foo&gt;
            /// </summary>
            public Type InterfaceType { get; private set; }
            /// <summary>
            /// Concrete type that implements the InterfaceType, eg FooValidator.
            /// </summary>
            public Type ValidatorType { get; private set; }
        }

    }
}