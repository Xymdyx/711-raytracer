﻿/*
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

		public Point position { get => this._position; set => this._position = value; }
		public Color lightColor { get => this._lightColor; set => this._lightColor = value; }
	}
}
