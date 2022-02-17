using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;

//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.Scene_Objects
{
	public class Sphere : SceneObject
	{
		private Point _center;
		private float _radius;

		public Point center { get => this._center; set => this._center = value; }
		public float radius { get => this._radius ; set => this._radius = value; }

		public Sphere()
		{
			this._center = new Point() ;
			this._radius = 1.0f;
		}

		public Sphere( Point center, float radius )
		{
			this._center = center;
			this._radius = radius;
		}


		public override float intersect( LightRay ray )
		{
			float kEpsilon = 1e-6f;
			Vector rayDir = ray.direction;
			Point rayPt = ray.origin;
			float w1 = float.MaxValue; // the distance where the ray and sphere intersect
			float w2 = float.MaxValue;

			// A = dx^2 + dy^2 + dz^2.. should be 1
			float A = (float) Math.Pow(rayDir.getLen(),2);

			//may need to swap subtraction.. B = 2( dx(x0- xc) + dy(yo-yc) + dz( zo-zc))
			// https://www.ccs.neu.edu/home/fell/CS4300/Lectures/Ray-TracingFormulas.pdf... it's rayPt - center
			float B = 2 * ( (rayDir.v1 * (rayPt.x - center.x) ) + ( rayDir.v2 * (rayPt.y - center.y) )
							+ ( rayDir.v3 * (rayPt.z - center.z )) ); 

			// C = (xo- xc)^2 + (yo - yc)^2 + (zo -zc)^2 -r^2
			float C = (float) ( Math.Pow( (rayPt.x - center.x), 2 ) + Math.Pow( (rayPt.y - center.y), 2 )
								+ Math.Pow( (rayPt.z - center.z), 2 ) - Math.Pow(this.radius, 2 ) ); //missed radius term...


			//apply quadratic formula since our ray vector is normalized
			float rootTerm = (float) Math.Pow( B, 2 ) - (4 * C);

			if (rootTerm < 0 || rootTerm == float.NaN)
			{
				return float.MinValue; // no real intersection
			}

			else if (rootTerm <= kEpsilon && rootTerm >= 0) //one real root, both results are equivalent
			{
				w1 = (float) (-B + Math.Sqrt( rootTerm )) / 2;
				return w1;
			}

			else					//we want the least positive w here...	
			{
				w1 = (float) (-B + Math.Sqrt( rootTerm )) / 2;
				w2 = (float) (-B - Math.Sqrt( rootTerm )) / 2;
				return Math.Min( w1, w2 );
			}
			// TODO caller function computes the point of intersection with returned w....page 27 in notes
		}

		public override Color illuminate()
		{
			return new Color( 0.0f, 0.0f, 1.0f ); //return the sphere color
		}

		public override void transform( Matrix4x4 camViewMat )
		{
			// MATRIX MULTI WORKS DEFINITELY

			Vector4 centerHmg = center.toHmgCoords(); // 4x1 Vector
			Vector4 newVertVec = Vector4.Transform( centerHmg, camViewMat); // we postMultiply since we are is LHS w Row-major
			center.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form
			return;
		}
	}
}
