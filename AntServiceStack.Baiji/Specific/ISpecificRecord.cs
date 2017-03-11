namespace AntServiceStack.Baiji.Specific
{
    /// <summary>
    /// Interface class for generated classes
    /// </summary>
    public interface ISpecificRecord
    {
        Schema.Schema GetSchema();

        /// <summary>
        /// Gets the value of a field given its position.
        /// </summary>
        /// <param name="fieldPos"></param>
        /// <returns></returns>
        object Get(int fieldPos);

        /// <summary>
        /// Sets the value of a field given its position.
        /// </summary>
        /// <param name="fieldPos"></param>
        /// <param name="fieldValue"></param>
        void Put(int fieldPos, object fieldValue);

        /// <summary>
        /// Gets the value of a field given its name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        object Get(string fieldName);

        /// <summary>
        /// Sets the value of a field given its name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        void Put(string fieldName, object fieldValue);
    }
}