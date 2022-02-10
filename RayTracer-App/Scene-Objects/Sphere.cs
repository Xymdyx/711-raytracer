using System;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;


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
			Vector rayDir = ray.direction;
			Point rayPt = ray.origin;
			double w1 = 1.0; // the distance where the ray and sphere intersect
			double w2 = -1.0;

			// A = dx^2 + dy^2 + dz^2
			double A = Math.Pow(rayDir.getLen(),2);

			//may need to swap subtraction.. B = 2( dx(x0- xc) + dy(yo-yc) + dz( zo-zc))
			// https://www.ccs.neu.edu/home/fell/CS4300/Lectures/Ray-TracingFormulas.pdf... it's rayPt - center
			double B = 2 * ( rayDir.v1 * (rayPt.x - center.x) + rayDir.v2 * (rayPt.y - center.y)
							+ rayDir.v3 * (rayPt.z - center.z ) ); 

			// C = (xo- xc)^2 + (yo - yc)^2 + (zo -zc)^2
			double C = Math.Pow( (rayPt.x - center.x), 2 ) + Math.Pow( (rayPt.y - center.y), 2 )
								+ Math.Pow( (rayPt.z - center.z), 2 ) - Math.Pow(this.radius, 2 ); //missed radius term...

			//apply quadratic formula since our ray vector is normalized
			double rootTerm = Math.Pow( B, 2 ) - (4 * C);

			if (rootTerm < 0 || rootTerm == Double.NaN)
			{
				return Double.MinValue; // no real intersection
			}

			else if (rootTerm == 0) //one real root, both results are equivalent
			{
				w1 = (-B + Math.Sqrt( rootTerm )) / 2;
				return w1;
			}

			else					//we want the least positive w here...	
			{
				w1 = (-B + Math.Sqrt( rootTerm )) / 2;
				w2 = (-B - Math.Sqrt( rootTerm )) / 2;
				return Math.Min( w1, w2 );
			}
			// TODO caller function computes the point of intersection with returned w....page 27 in notes
		}

		public override Color illuminate()
		{
			return new Color( 0.0, 0.0, 1.0 ); //return the sphere color
		}

		public override void transform( Matrix4d camViewMat )
		{
			// TODO covert where my sphere is in world coords to camera coords

			Matrix4d centerHmg = center.toHmgCoords(); // 4x4 with only 1st column having x, y, z, w...Rest is 0s.
			Matrix4d newVertMat = camViewMat * centerHmg; // we postMultiply since we are is LHS
			center.fromHmgCoords( newVertMat ); // [x y z w] => (x/w, y/w, z/w) CP form
			return;
		}
	}
}
