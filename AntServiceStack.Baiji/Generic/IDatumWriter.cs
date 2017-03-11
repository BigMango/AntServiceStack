using AntServiceStack.Baiji.IO;

namespace AntServiceStack.Baiji.Generic
{
    public interface DatumWriter
    {
        Schema.Schema Schema
        {
            get;
        }

        void Write(object datum, IEncoder encoder);

        void Write<T>(T datum, IEncoder encoder);
    }
}