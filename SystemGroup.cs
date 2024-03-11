﻿using UnityEngine.Profiling;

namespace Flexy.Core;

public class SystemGroup : System
{
	public SystemGroup( String name ) { _name = name; }

	private List<System> _systems = new( );

	public String					Name	=> _name;
	public List<System>	Systems	=> _systems;

	public Single					LastRunUpdateTime	=> _lastRunUpdateTime;
	public Int32					LastRunUpdateFrame	=> _lastRunUpdateFrame;

	protected internal override void Update( )
	{
		Profiler.BeginSample( _name );
		
		_timer.Restart( );
		
		foreach ( var s in _systems )
		{
			try						{ s.UpdateProfiled( );		}
			catch (Exception ex)	{ Debug.LogException(ex);	}
		}
		
		_lastRunUpdateFrame	= Time.frameCount;
		_lastRunUpdateTime	= (Single)_timer.Elapsed.TotalMilliseconds;
		_timer.Stop( );
		
		Profiler.EndSample( );
	}
}