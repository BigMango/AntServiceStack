using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AntServiceStack.Common")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("AntServiceStack")]
[assembly: AssemblyCopyright("Copyright © 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("66e01590-e4dc-4107-a47f-d682fe4ea15a")]

// CCB Custom
[assembly: ContractNamespace("http://schemas.AntServiceStack.net/types",
 ClrNamespace = "AntServiceStack.Common.ServiceClient.Web")]

[assembly: ContractNamespace("http://schemas.AntServiceStack.net/types",
 ClrNamespace = "AntServiceStack.Common.ServiceModel")]

[assembly: InternalsVisibleTo("AntServiceStack")]
[assembly: InternalsVisibleTo("AntServiceStack.Client")]