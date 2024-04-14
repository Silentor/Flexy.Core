namespace Flexy.Core;

public class PlymorphAttribute : PropertyAttribute 
{
	public PlymorphAttribute( ){}
	public PlymorphAttribute( Type baseType ) { BaseType = baseType; }

	public Type BaseType { get; set; }
}