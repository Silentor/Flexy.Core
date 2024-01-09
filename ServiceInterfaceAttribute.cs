namespace Flexy.Core
{
	public class ServiceInterfaceAttribute: Attribute
	{
		public ServiceInterfaceAttribute ( params Type[] interfaceType )
		{
			InterfaceType = interfaceType;
		}
		
		public readonly Type[] InterfaceType;
	}
}