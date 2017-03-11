using AntServiceStack.Baiji.IO;

namespace AntServiceStack.Baiji.Generic
{
    public interface DatumReader
    {
        Schema.Schema Schema
        {
            get;
        }

        /// <summary>
        /// Read a datum.
        /// Traverse the schema, depth-first, reading all leaf values in the schema into a datum that is returned.
        /// If the provided datum is non-null it may be reused and returned.
        /// </summary>
        object Read(object reuse, IDecoder decoder);

        /// <summary>
        /// Read a datum.
        /// Traverse the schema, depth-first, reading all leaf values in the schema into a datum that is returned.
        /// If the provided datum is non-null it may be reused and returned.
        /// </summary>
        T Read<T>(T reuse, IDecoder decoder);
    }
}