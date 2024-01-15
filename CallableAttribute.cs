namespace Flexy.Core
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CallableAttribute : UnityEngine.Scripting.PreserveAttribute
	{
		public		CallableAttribute			( ) { }
		public		CallableAttribute			( Type argumentType ) { ArgumentType = argumentType; }
		
		public		Type	ArgumentType {get; init;}
	}
}