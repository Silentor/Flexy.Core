namespace Flexy.Core
{
	public class ServiceInterfaceAttribute: Attribute
	{
		public ServiceInterfaceAttribute ( Type interfaceType )
		{
			InterfaceType = interfaceType;
		}
		
		public readonly Type InterfaceType;
	}
}