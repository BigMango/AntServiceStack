namespace Ant.Tools.SOA.CodeGeneration.Options
{
    /// <summary>
    /// Used to indicate if service or client code is being generated.
    /// </summary>
    public enum CodeGeneratorMode
    {
        /// <summary>
        /// Service code is being generated.
        /// </summary>
        Service,

        /// <summary>
        /// Client code is being generated.
        /// </summary>
        Client,

        /// <summary>
        /// Client code for test is being generated.
        /// </summary>
        ClientForTest
    }
}