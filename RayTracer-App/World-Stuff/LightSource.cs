/*
author : Sam Ford
desc: class that reps light source in world
date started: 2/19/2021
 */
using System;

namespace RayTracer_App.World
{
	public class LightSource 
	{
		private Point _position;
		private Color _lightColor;
		private float _power; // for photon mapping

		public Point position { get => this._position; set => this._position = value; }
		public Color lightColor { get => this._lightColor; set => this._lightColor = value; }
		public float power { get => this._power; set => this._power = value; }

		public LightSource ()
		{
			this._position = new Point();

			this._lightColor = new Color();

			this._power = 0;
		}

		public LightSource( Point position, Color lightColor, float power = 100)
		{
			this._position = position;
			this._lightColor = lightColor;
			this._power = power; 
		}
	}
}
