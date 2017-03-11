using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Specific
{
    public abstract class SpecificRecordBase : ISpecificRecord
    {
        #region Implementation of ISpecificRecord
        public abstract Schema.Schema GetSchema();

        /// <summary>
        /// Gets the value of a field given its position.
        /// </summary>
        /// <param name="fieldPos"></param>
        /// <returns></returns>
        public abstract object Get(int fieldPos);

        /// <summary>
        /// Sets the value of a field given its position.
        /// </summary>
        /// <param name="fieldPos"></param>
        /// <param name="fieldValue"></param>
        public abstract void Put(int fieldPos, object fieldValue);

        /// <summary>
        /// Gets the value of a field given its name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object Get(string fieldName)
        {
            var recordSchema = GetSchema() as RecordSchema;
            if (recordSchema == null)
            {
                return null;
            }
            Field field;
            if (!recordSchema.TryGetField(fieldName, out field))
            {
                return null;
            }
            return Get(field.Pos);
        }

        /// <summary>
        /// Sets the value of a field given its name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public void Put(string fieldName, object fieldValue)
        {
            var recordSchema = GetSchema() as RecordSchema;
            if (recordSchema == null)
            {
                return;
            }
            Field field;
            if (!recordSchema.TryGetField(fieldName, out field))
            {
                return;
            }
            Put(field.Pos, fieldValue);
        }
        #endregion
    }
}