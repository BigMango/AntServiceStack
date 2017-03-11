using System;
using System.IO;
using AntServiceStack.Baiji.Specific;

namespace AntServiceStack.Baiji
{
    public interface ISerializer
    {
        /// <summary>
        /// Serialize an object to the specified stream. Only types implementing ISpecificRecord is allowed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentException">The type doesn't implement <code>ISpecificRecord</code>.</exception>
        void Serialize(object obj, Stream stream);

        /// <summary>
        /// Serialize an object to the specified stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        void Serialize<T>(T obj, Stream stream) where T : ISpecificRecord, new();

        /// <summary>
        /// Deserialize an object of the specified type from the specified stream.
        /// The type must implement ISpecificRecord.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentException">The type doesn't implement <code>ISpecificRecord</code>.</exception>
        /// <returns></returns>
        object Deserialize(Type type, Stream stream);

        /// <summary>
        /// Deserialize an object of the specified type from the specified stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T Deserialize<T>(Stream stream) where T : ISpecificRecord, new();
    }
}