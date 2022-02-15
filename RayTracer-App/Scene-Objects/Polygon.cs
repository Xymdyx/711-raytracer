using System;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;
using System.Numerics; //for Matrix4x4 float

//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class Polygon : SceneObject
	{
		private List<Point> _vertices;
		private Vector _normal;


		public List<Point> vertices { get => this._vertices; set => this._vertices = value; } 
		public Vector normal { get => this._normal; set => this._normal = value; }

// default constructor for a plain triangle
		public Polygon()
		{
			this._vertices = new List<Point> { new Point( 1, 0, 0 ), new Point( 0, 1, 0 ), new Point( -1, 0, 0 ) };
			this.normal = new Vector();
		}

// parameter constructor
		public Polygon( List<Point> vertices ) 
		{
			this._vertices = vertices;
			this._normal = new Vector(0,1,0); //TODO calculate normal
		}


		//METHODS.. TODO mplement intersection for polygon
		//use barycentric coordinates formula to get intersection
		public override float intersect( LightRay ray )
		{
			//HACK... put triangle case in here
			float w = float.MaxValue;

			if (this.vertices.Count == 3)
			{
				//do triangle intersection formula with barycentric coordinates
				// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
				// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection
				// try not normalizing anything - 2/13
				//use (w,u,v) = (1/(P . e1)) * ( Q . e2, P . T, Q. D)

				Vector e1 = vertices[1].ptSub( vertices[0] ); // cross, dot, and normalize good
				Vector e2 = vertices[2].ptSub( vertices[0] ); //this may be the issue

				Vector P = ray.direction.crossProduct( e2, false );
				float denom = P.dotProduct( e1 );

				if (( denom >= -1e-8 && denom <= 1e-8) || denom == float.NaN) return float.MaxValue;  // ray is parallel to triangle

				float denomScale = 1 / denom;

				Vector T = ray.origin.ptSub( vertices[0] );
				float u = P.dotProduct( T ) * denomScale;

				if (u < 0 || u > 1) return float.MaxValue;

				Vector Q = T.crossProduct( e1, false );
				float v = Q.dotProduct( ray.direction ) * denomScale;

				if (v < 0 || u + v > 1) return float.MaxValue;
				
				w = Q.dotProduct( e2 ) * denomScale;

				// where is our point?
				if (w < 0 || w == float.NaN) return float.MaxValue; // intersection behind origin

				Vector normal = e1.crossProduct( e2, false );
				// intersection = ray.origin + (ray.direction * w)
				//u,v are barycentric boordsinates of intersection point
				return w; //w is distance along ray of intersection point
			}

			return w;
		}

		public override Color illuminate()
		{
			return new Color( 1.0f, 0.0f, 0.0f ); //return the floor color
		}

		public override void transform( Matrix4x4 camViewMat )
		{
			//VERIFIED - 2/13	
			// use post-multiply since I am using column-major... Vnew = B*A*Vold
			foreach (Point vertex in vertices)
			{
				Matrix4x4 ptHmg = vertex.toHmgCoords(); // 4x4 with only 1st column having x, y, z, w...Rest is 0s.
				Matrix4x4 newVertMat = camViewMat * ptHmg; // we postMultiply since we are is RHS
				vertex.fromHmgCoords( newVertMat ); // [x y z w] => (x/w, y/w, z/w) CP form.. DONE -- MATRIX-MULTI works
			}
		}
	}
}
