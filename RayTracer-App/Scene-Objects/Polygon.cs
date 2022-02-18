using System;
using System.Collections.Generic;
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


		//use barycentric coordinates formula to get intersection
		public override float intersect( LightRay ray )
		{
			float w = float.MaxValue;

			if (this.vertices.Count == 3)
			{
				//do triangle intersection formula with barycentric coordinates
				// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
				// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection
				// try not normalizing anything - 2/13

				// intersection = ray.origin + (ray.direction * w)
				//u,v are barycentric boordsinates of intersection point
				//use (w,u,v) = (1/(P . e1)) * ( Q . e2, P . T, Q. D)
				// cross, dot, and normalize good
				float kEpsilon = 1e-6f;

				Vector e1 = vertices[1].ptSub( vertices[0] ); // e1 = v1 - v0
				Vector e2 = vertices[2].ptSub( vertices[0] ); // e2= v2 - v0

				Vector P = ray.direction.crossProduct( e2, false ); // P = rayDirection x e2
				float denom = P.dotProduct( e1 ); // denom = p dot e1

				if ((denom >= -kEpsilon && denom <= kEpsilon) || denom == float.NaN) return float.MaxValue;  // ray is parallel to triangle

				float denomScale = 1f / denom;

				Vector T = ray.origin.ptSub( vertices[0] ); // T = rayDirection - v0
				float u = P.dotProduct( T ) * denomScale; // u = (P dot T) * denomScale

				if (u < 0 || u > 1) return float.MaxValue;

				Vector Q = T.crossProduct( e1, false ); // Q = T x e1
				float v = Q.dotProduct( ray.direction ) * denomScale; //  v = (Q dot rayDir) * denomScale

				if (v < 0 || u + v > 1) return float.MaxValue;

				w = Q.dotProduct( e2 ) * denomScale; //point along ray where we intersect... w = (Q dot e2) * denomScale

				// where is our point?
				if (w < 0 || w == float.NaN) return float.MaxValue; // intersection behind origin

				Vector normal = e1.crossProduct( e2, false );

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
			// use pre-multiply
			foreach (Point vertex in vertices)
			{
				Vector4 ptHmg = vertex.toHmgCoords();
				Vector4 newVertVec = Vector4.Transform( ptHmg, camViewMat ); // we postMultiply since we are is LHS
				vertex.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form.. DONE -- MATRIX-MULTI works
			}
		}

		// scaling all three has no visible effect
		public void scale( float x, float y, float z )
		{
			foreach(Point vertex in vertices)
			{
				Vector4 ptHmg = vertex.toHmgCoords();
				Matrix4x4 scale = new Matrix4x4
					( x, 0 , 0, 0,
					 0, y, 0, 0,
					 0, 0, z, 0,
					 0, 0, 0, 1);
				Vector4 newScaledVec = Vector4.Transform( ptHmg, scale );
				vertex.fromHmgCoords( newScaledVec );
			}
		}

		public void translate( float x, float y, float z )
		{
			foreach (Point vertex in vertices)
			{
				Vector4 ptHmg = vertex.toHmgCoords();
				Matrix4x4 scale = new Matrix4x4
					( 1, 0, 0, 0,
					 0, 1, 0, 0,
					 0, 0, 1, 0,
					 x, y, z, 1 );
				Vector4 newTransVec = Vector4.Transform( ptHmg, scale );
				vertex.fromHmgCoords( newTransVec );
			}
		}
	}

	/*triangle-ray intersection NORMALIZED VECS*/
	//Vector e1 = vertices[1] - vertices[0]; // cross, dot, and normalize good
	//Vector e2 = vertices[2] - vertices[0]; //this may be the issue

	//Vector P = ray.direction.crossProduct( e2 );
	//float denom = P.dotProduct( e1 );

	//if (( denom >= -1e-8 && denom <= 1e-8) || denom == float.NaN) return float.MaxValue;  // ray is parallel to triangle

	//float denomScale = 1 / denom;

	//Vector T = ray.origin - vertices[0];
	//float u = P.dotProduct( T ) * denomScale;

	//if (u < 0 || u > 1) return float.MaxValue;

	//Vector Q = T.crossProduct( e1 );
	//float v = Q.dotProduct( ray.direction ) * denomScale;

	//if (v < 0 || u + v > 1) return float.MaxValue;

	//w = Q.dotProduct( e2 ) * denomScale;

	////where is our point?
	//if (w < 0 || w == float.NaN) return float.MaxValue; // intersection behind origin

	//Vector normal = e1.crossProduct( e2 );

}
