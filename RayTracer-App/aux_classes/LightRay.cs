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

	public Vector direction { get => this._direction; set => this._direction = value; }
	public Point origin { get => this._origin; set => this._origin = value; }

	public LightRay( Vector direction, Point origin)
	{
		this._direction = direction;
		this._origin = origin;
	}
}