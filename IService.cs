namespace Flexy.Core;

public interface IService		{ void		OrderedInit			( GameContext ctx ); }
public interface IServiceAsync	{ UniTask	OrderedInitAsync	( GameContext gameWorld ); }