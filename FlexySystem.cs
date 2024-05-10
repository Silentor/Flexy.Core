using System.Diagnostics;
using UnityEngine.Profiling;

namespace Flexy.Core;

public abstract class FlexySystem
{
	protected String		_name;
	protected Single		_lastRunUpdateTime;
	protected Int32			_lastRunUpdateFrame;
	protected Stopwatch		_timer = new();

	public	String			Name				=> _name;
	public	Single			LastRunUpdateTime	=> _lastRunUpdateTime;
	public	Int32			LastRunUpdateFrame	=> _lastRunUpdateFrame;
	
	protected internal abstract void Update( );
	
	internal void UpdateProfiled( )
	{
		if( _name == null )
			_name = GetType().FullName;
		
		Profiler.BeginSample( GetType().Name );
		
		_timer.Restart( );
		
		Update( );
		
		_lastRunUpdateFrame	= Time.frameCount;
		_lastRunUpdateTime	= (Single)_timer.Elapsed.TotalMilliseconds;
		_timer.Stop( );
		
		Profiler.EndSample( );
	}
}

// Add ability to make updates in jobs
// https://github.com/gilzoide/unity-update-manager?