/*
 author: Sam Ford (stf8464)
date started: 2/6/22
desc: class that represents a light ray
*/

using System;

public class LightRay 
{

	private Vector _direction;
	private Point _origin;
	private Point _entryPt; //use these to keep track if we're in an object 
	private Point _exitPt;


	public Vector direction { get => this._direction; set => this._direction = value; }
	public Point origin { get => this._origin; set => this._origin = value; }
	public Point entryPt { get => this._entryPt; set => this._entryPt = value; }
	public Point exitPt { get => this._exitPt; set => this._exitPt = value; }

	public LightRay( Vector direction, Point origin)
	{
		this._direction = direction;
		this._origin = origin;
	}
	public void clearObjPts()
	{
		this._entryPt = null;
		this._exitPt = null;
	}

	public bool insideObj()
	{
		return (entryPt != null);
	}

	public Point findPtAlong( float w )
	{
		Vector scaledDir = this.direction.scale( w );
		return this.origin + scaledDir;
	}
}