using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Scene_Objects
{
	public class Sphere : SceneObject
	{
		private Point _center;
		private double _radius;

		public Point center { get => this._center; set => this._center = value; }
		public double radius { get => this._radius ; set => this._radius = value; }

		public Sphere()
		{
			this._center = new Point() ;
			this._radius = 1.0;
		}

		public Sphere( Point center, double radius )
		{
			this._center = center;
			this._radius = radius;
		}


		//TODO use light-ray intersection formula that involves quadratic formula
		public override double intersect( LightRay ray )
		{
			return 1.0;
		}

		public override Color illuminate()
		{
			return new Color( 0.450, 0.0720, 0.444 ); //return the sphere color
		}
	}
}
