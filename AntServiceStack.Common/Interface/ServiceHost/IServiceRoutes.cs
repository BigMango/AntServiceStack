using System.Reflection;

namespace AntServiceStack.ServiceHost
{
    /// <summary>
    /// Allow the registration of user-defined routes for services
    /// </summary>
    public interface IServiceRoutes
    {
        /// <summary>
        ///		Maps the specified REST path to the specified service method.
        /// </summary>
        ///	<param name="mi">The service method to map the path to.</param>
        /// <param name="restPath">The path to map the request DTO to.
        ///		See <see cref="RestServiceAttribute.Path">RestServiceAttribute.Path</see>
        ///		for details on the correct format.</param>
        /// <returns>The same <see cref="IServiceRoutes"/> instance;
        ///		never <see langword="null"/>.</returns>
        IServiceRoutes Add(MethodInfo mi, string restPath);

        /// <summary>
        ///		Maps the specified REST path to the specified service method, and
        ///		specifies the HTTP verbs supported by the path.
        /// </summary>
        ///	<param name="mi">The service method to map the path to.</param>
        /// <param name="restPath">The path to map the request DTO to.
        ///		See <see cref="RestServiceAttribute.Path">RestServiceAttribute.Path</see>
        ///		for details on the correct format.</param>
        /// <param name="verbs">
        ///		The comma-delimited list of HTTP verbs supported by the path, 
        ///		such as "GET,PUT,DELETE".  Specify empty or <see langword="null"/>
        ///		to indicate that all verbs are supported.
        /// </param>
        /// <returns>The same <see cref="IServiceRoutes"/> instance;
        ///		never <see langword="null"/>.</returns>
        IServiceRoutes Add(MethodInfo mi, string restPath, string verbs);

        /// <summary>
        ///		Maps the specified REST path to the specified service method, 
        ///		specifies the HTTP verbs supported by the path, and indicates
        ///		the default MIME type of the returned response.
        /// </summary>
        ///	<param name="mi">The service method to map the path to.</param>
        /// <param name="restPath">The path to map the request DTO to.
        ///		See <see cref="RestServiceAttribute.Path">RestServiceAttribute.Path</see>
        ///		for details on the correct format.</param>
        /// <param name="verbs">
        ///		The comma-delimited list of HTTP verbs supported by the path, 
        ///		such as "GET,PUT,DELETE".
        /// </param>
        /// <param name="summary">
        ///     The short summary of what the REST does. 
        /// </param>
        /// <param name="notes">
        ///     The longer text to explain the behaviour of the REST. 
        /// </param>
        /// <returns>The same <see cref="IServiceRoutes"/> instance;
        ///		never <see langword="null"/>.</returns>
        IServiceRoutes Add(MethodInfo mi, string restPath, string verbs, string summary, string notes);
    }
}