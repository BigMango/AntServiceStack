namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Base class for all unnamed schemas
    /// </summary>
    public abstract class UnnamedSchema : Schema
    {
        protected UnnamedSchema(SchemaType type, PropertyMap props)
            : base(type, props)
        {
        }

        public override string Name
        {
            get
            {
                return Type.ToString().ToLower();
            }
        }
    }
}