using System;
using System.Numerics;
using RayTracer_App.Illumination_Models;


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
			this._normal = null;
			this._diffuse = Color.sphereColor;
			this._specular = Color.whiteSpecular;
		}

		public Sphere( Point center, float radius )
		{
			this._center = center;
			this._radius = radius;
			this._normal = null;
			this._diffuse = Color.sphereColor;
			this._specular = Color.whiteSpecular;
			this._lightModel = PhongBlinn.regularPhongBlinn; //change iullum model here for now

		}

		public Sphere( Point center, float radius, Color diffuse, Color specular )
		{
			this._center = center;
			this._radius = radius;
			this._normal = null;
			this._diffuse = diffuse;
			this._specular = specular;
		}

		// function for getting where along ray intersection happens with a sphere
		// sets normal somewhere.. see 27 in notes
		public override Point getRayPoint( LightRay ray, float w ) //corrected on 2/27...
		{
			Vector scaledDir = ray.direction.scale( w );
			Point rayPoint = ray.origin + scaledDir;
			this.normal = rayPoint - this.center; //want this normalized
			return rayPoint;
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

			// A = dx^2 + dy^2 + dz^2.. should always be 1 since I normalize the rayDir. Included for completeness
			float magn = rayDir.getLen();
			float A = ( magn * magn );

			//may need to swap subtraction.. B = 2( dx(x0- xc) + dy(yo-yc) + dz( zo-zc))
			// https://www.ccs.neu.edu/home/fell/CS4300/Lectures/Ray-TracingFormulas.pdf... it's rayPt - center
			float B = 2 * ( (rayDir.v1 * (rayPt.x - center.x) ) + ( rayDir.v2 * (rayPt.y - center.y) )
							+ ( rayDir.v3 * (rayPt.z - center.z )) );

			// C = (xo- xc)^2 + (yo - yc)^2 + (zo -zc)^2 -r^2
			float C = (float) ( ((rayPt.x - center.x) * (rayPt.x - center.x)) + ((rayPt.y - center.y) * (rayPt.y - center.y))
								+ ((rayPt.z - center.z) * (rayPt.z - center.z)) - (this.radius * this.radius ) ); //missed radius term...


			//apply quadratic formula since our ray vector is normalized
			float rootTerm = (float) ((B * B) - (4f * C));

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
		}


		public override Color illuminate()
		{
			return new Color( 0.214f, 0.519f, 0.630f ); //return the sphere color
		}

		public override void transform( Matrix4x4 camViewMat )
		{
			// MATRIX MULTI WORKS DEFINITELY
			Vector4 centerHmg = center.toHmgCoords(); // 1x4 Vector
			Vector4 newVertVec = Vector4.Transform( centerHmg, camViewMat); // we postMultiply since we are is LHS w Row-major.. Vnew = Vold * A * B
			center.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form
			return;
		}


		public void scale( float x, float y, float z )
		{
			center.scale( x, y, z );
		}

		public void translate( float x, float y, float z )
		{
			center.translate( x, y, z );
		}

		// helper for getting minimum and max points
		public Point getMaxPt( int axis )
		{
			if (axis >= 3 || axis < 0)
			{
				Console.WriteLine( $"Axis {axis} is not int range [0-2]" );
				return null;
			}

			Vector maxAxisDir = null;
			if ( axis == 0)
				maxAxisDir = new Vector( this.radius, 0, 0, false );
			else if( axis == 1 )
				maxAxisDir = new Vector( 0, this.radius, 0, false );
			else
				maxAxisDir = new Vector( 0, 0, this.radius, false );


			return this.center + maxAxisDir;
		}

		// helper for getting minimum points on sphere
		public Point getMinPt( int axis )
		{
			if (axis >= 3 || axis < 0)
			{
				Console.WriteLine( $"Axis {axis} is not int range [0-2]" );
				return null;
			}

			Vector minAxisDir = null;
			if ( axis == 0)
				minAxisDir = new Vector( this.radius, 0, 0, false );
			else if( axis == 1 )
				minAxisDir = new Vector( 0, this.radius, 0, false );
			else
				minAxisDir = new Vector( 0, 0, this.radius, false );

			return this.center - minAxisDir;
		}
	}

	// equivalent alternative for C in intersect formula
	//Vector raytoOrigin = rayPt.ptSub( center );
	//float magnSquaredTerm = (float) Math.Pow( raytoOrigin.getLen(), 2f );
	//float C2 = (float) ( magnSquaredTerm - Math.Pow( this.radius, 2f ) );
}
