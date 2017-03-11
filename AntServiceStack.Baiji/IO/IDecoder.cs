using System;
namespace AntServiceStack.Baiji.IO
{
    /// <summary>
    /// Decoder is used to decode Baiji data on a stream. There are methods to read the Baiji types on the stream. There are also
    /// methods to skip items, which are usually more efficient than reading, on the stream.
    /// </summary>
    public interface IDecoder
    {
        /// <summary>
        /// Reads a null Baiji type.
        /// </summary>
        void ReadNull();

        /// <summary>
        /// Read a boolean Baiji type
        /// </summary>
        /// <returns>The boolean just read</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads an int Baiji type.
        /// </summary>
        /// <returns>The int just read</returns>
        int ReadInt();

        /// <summary>
        /// Reads a long Baiji type.
        /// </summary>
        /// <returns>The long just read</returns>
        long ReadLong();

        /// <summary>
        /// Reads a float Baiji type
        /// </summary>
        /// <returns>The float just read</returns>
        float ReadFloat();

        /// <summary>
        /// Reads a double Baiji type
        /// </summary>
        /// <returns>The double just read</returns>
        double ReadDouble();

        /// <summary>
        /// Reads the bytes Baiji type
        /// </summary>
        /// <returns>The bytes just read</returns>
        byte[] ReadBytes();

        /// <summary>
        /// Reads a string Baiji type
        /// </summary>
        /// <returns>The string just read</returns>
        string ReadString();

        /// <summary>
        /// Reads an enum BaijiType
        /// </summary>
        /// <returns>The enum just read</returns>
        int ReadEnum();

        DateTime ReadDateTime();

        /// <summary>
        /// Starts reading the array Baiji type. This, together with ReadArrayNext() is used to read the
        /// items from Baiji array. This returns the number of entries in the initial chunk. After consuming
        /// the chunk, the client should call ReadArrayNext() to get the number of entries in the next
        /// chunk. The client should repeat the procedure until there are no more entries in the array.
        /// 
        /// for (int n = decoder.ReadArrayStart(); n > 0; n = decoder.ReadArrayNext())
        /// {
        ///     // Read one array entry.
        /// }
        /// </summary>
        /// <returns>The number of entries in the initial chunk, 0 if the array is empty.</returns>
        long ReadArrayStart();

        /// <summary>
        /// See ReadArrayStart().
        /// </summary>
        /// <returns>The number of array entries in the next chunk, 0 if there are no more entries.</returns>
        long ReadArrayNext();

        /// <summary>
        /// Starts reading the map Baiji type. This, together with ReadMapNext() is used to read the
        /// entries from Baiji map. This returns the number of entries in the initial chunk. After consuming
        /// the chunk, the client should call ReadMapNext() to get the number of entriess in the next
        /// chunk. The client should repeat the procedure until there are no more entries in the array.
        /// for (int n = decoder.ReadMapStart(); n > 0; n = decoder.ReadMapNext())
        /// {
        ///     // Read one map entry.
        /// }
        /// </summary>
        /// <returns>The number of entries in the initial chunk, 0 if the map is empty.</returns>
        long ReadMapStart();

        /// <summary>
        /// See ReadMapStart().
        /// </summary>
        /// <returns>The number of map entries in the next chunk, 0 if there are no more entries.</returns>
        long ReadMapNext();

        /// <summary>
        /// Reads the index, which determines the type in an union Baiji type.
        /// </summary>
        /// <returns>The index of the type within the union.</returns>
        int ReadUnionIndex();
    }
}
