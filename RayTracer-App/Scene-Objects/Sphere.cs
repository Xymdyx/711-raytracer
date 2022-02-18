using System;
using System.Numerics;

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

		// Ray-sphere intersection, triple checking on 2/18/22
		// https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
		public override float intersect( LightRay ray )
		{
			float kEpsilon = 1e-6f;
			Vector rayDir = ray.direction;
			Point rayPt = ray.origin;
			float w1 = float.MaxValue; // the distance where the ray and sphere intersect
			float w2 = float.MaxValue;

			// A = dx^2 + dy^2 + dz^2.. should always be 1 since I normalize the rayDir
			float A = (float) Math.Pow(rayDir.getLen(),2);

			//may need to swap subtraction.. B = 2( dx(x0- xc) + dy(yo-yc) + dz( zo-zc))
			// https://www.ccs.neu.edu/home/fell/CS4300/Lectures/Ray-TracingFormulas.pdf... it's rayPt - center
			float B = 2 * ( (rayDir.v1 * (rayPt.x - center.x) ) + ( rayDir.v2 * (rayPt.y - center.y) )
							+ ( rayDir.v3 * (rayPt.z - center.z )) );

			// C = (xo- xc)^2 + (yo - yc)^2 + (zo -zc)^2 -r^2
			float C = (float) ( Math.Pow( (rayPt.x - center.x), 2f ) + Math.Pow( (rayPt.y - center.y), 2f )
								+ Math.Pow( (rayPt.z - center.z), 2f ) - Math.Pow(this.radius, 2f ) ); //missed radius term...


			//apply quadratic formula since our ray vector is normalized
			float rootTerm = (float) Math.Pow( B, 2f) - (4f * C);

			if (rootTerm < 0 || rootTerm == float.NaN)
			{
				return float.MinValue; // no real intersection
			}

			else if (rootTerm <= kEpsilon && rootTerm >= -kEpsilon) //one real root, both results are equivalent
			{
				w1 = (float) (-B + Math.Sqrt( rootTerm )) / 2f;
				return w1;
			}

			else					//we want the least positive w here...	
			{
				w1 = (float) (-B + Math.Sqrt( rootTerm )) / 2f;
				w2 = (float) (-B - Math.Sqrt( rootTerm )) / 2f;
				return Math.Min( w1, w2 );
			}
			// TODO caller function computes the point of intersection with returned w....page 27 in notes
		}

		public override Color illuminate()
		{
			return new Color( 0.214f, 0.519f, 0.630f ); //return the sphere color
		}

		public override void transform( Matrix4x4 camViewMat )
		{
			// MATRIX MULTI WORKS DEFINITELY
			Vector4 centerHmg = center.toHmgCoords(); // 1x4 Vector
			Vector4 newVertVec = Vector4.Transform( centerHmg, camViewMat); // we postMultiply since we are is LHS w Row-major
			center.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form
			return;
		}


		public void scale( float x, float y, float z )
		{
			Vector4 ptHmg = center.toHmgCoords();
			Matrix4x4 scale = new Matrix4x4
				( x, 0, 0, 0,
					0, y, 0, 0,
					0, 0, z, 0,
					0, 0, 0, 1 );
			Vector4 newScaledVec = Vector4.Transform( ptHmg, scale );
			center.fromHmgCoords( newScaledVec );
		}

		public void translate( float x, float y, float z )
		{
			Vector4 ptHmg = center.toHmgCoords();
			Matrix4x4 scale = new Matrix4x4
				( 1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					x, y, z, 1 );
			Vector4 newTransVec = Vector4.Transform( ptHmg, scale );
			center.fromHmgCoords( newTransVec );
		}
	}

	// equivalent alternative for C in intersect formula
	//Vector raytoOrigin = rayPt.ptSub( center );
	//float magnSquaredTerm = (float) Math.Pow( raytoOrigin.getLen(), 2f );
	//float C2 = (float) ( magnSquaredTerm - Math.Pow( this.radius, 2f ) );
}
