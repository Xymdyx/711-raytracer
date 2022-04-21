using System;
using System.Collections.Generic;
using System.Numerics; //for Matrix4x4 float
using RayTracer_App.Illumination_Models;

//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class Polygon : SceneObject
	{
		private List<Point> _vertices;
		private float _u = 0.0f; //for cp4
		private float _v = 0.0f; //for cp4


		public List<Point> vertices { get => this._vertices; set => this._vertices = value; } 
		public float u { get => this._u; set => this._u = value; }
		public float v { get => this._v; set => this._v = value; }


		// default constructor for a plain triangle
		public Polygon()
		{
			this._vertices = new List<Point> { new Point( 1, 0, 0 ), new Point( 0, 1, 0 ), new Point( -1, 0, 0 ) };
			this._normal = null;
			this._diffuse = Color.floorColor;
			this._specular = Color.whiteSpecular;
			this._lightModel = PhongBlinn.regularPhongBlinn;
		}

// parameter constructor
		public Polygon( List<Point> vertices, IlluminationModel lightModel = null ) 
		{
			this._vertices = vertices;
			this._normal = null; //TODO calculate normal
			this._diffuse = Color.floorColor;
			this._specular = Color.whiteSpecular;
			this._lightModel = Phong.floorPhong; //change this to change lighting

			if (lightModel != null)
				this._lightModel = lightModel;

		}

		public Polygon( List<Point> vertices, Color diffuse, IlluminationModel lightModel = null, Color specular = null )
		{
			this._vertices = vertices;
			this._normal = null; //TODO calculate normal
			this._diffuse = diffuse;
			this._specular = Color.whiteSpecular;
			this._lightModel = Phong.floorPhong;

			if (specular != null)
				this._specular = specular;
			if (lightModel != null)
				this._lightModel = lightModel;
		}


		// function for getting where along ray intersection happens with a triangle
		// sets normal somewhere.. see 28 in notes
		public override Point getRayPoint( LightRay ray, float w ) //double-checked on 2/27
		{
			Vector scaledDir = ray.direction.scale( w );
			Point rayPoint = ray.origin + scaledDir;
			return rayPoint;
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

				// TODO... CP4... store u and v values somewhere...

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

				this._normal = e1.crossProduct( e2, true ); //set the normal here while we have these

				this.u = u;
				this.v = v;
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

		// scaling all three 
		public void scale( float x, float y, float z )
		{
			foreach(Point vertex in vertices)
			{
				vertex.scale( x, y, z );
			}
		}

		public void translate( float x, float y, float z )
		{
			foreach (Point vertex in vertices)
			{
				vertex.translate( x, y, z );
			}
		}

		public void rotateX( float degrees)
		{
			foreach (Point vertex in vertices)
			{
				vertex.rotateX( degrees );
			}
		}

		public void rotateY( float degrees )
		{
			foreach (Point vertex in vertices)
			{
				vertex.rotateY( degrees );
			}
		}

		public void rotateZ( float degrees )
		{
			foreach (Point vertex in vertices)
			{
				vertex.rotateZ( degrees );
			}
		}

		// helper for getting minimum and max points
		public Point getMaxPt( int axis )
		{
			Point maxPt = this.vertices[0];
			if( axis >= 3 || axis < 0)
			{
				Console.WriteLine( $"Axis {axis} is not int range [0-2]" );
				return null;
			}

			foreach( Point p in this.vertices)
			{
				if( axis == 0)
				{
					if (p.x > maxPt.x)
						maxPt = p;
				} 
				else if( axis == 1)
				{
					if (p.y > maxPt.y)
						maxPt = p;
				}
				else
				{
					if (p.z > maxPt.z)
						maxPt = p;
				}
			}

			return maxPt;
		}

		// helper for getting minimum and max points
		public Point getMinPt( int axis )
		{
			Point minPt = this.vertices[0];
			if (axis >= 3 || axis < 0)
			{
				Console.WriteLine( $"Axis {axis} is not int range [0-2]" );
				return null;
			}

			foreach (Point p in this.vertices)
			{
				if (axis == 0)
				{
					if (p.x < minPt.x)
						minPt = p;
				}
				else if (axis == 1)
				{
					if (p.y < minPt.y)
						minPt = p;
				}
				else
				{
					if (p.z < minPt.z)
						minPt = p;
				}
			}

			return minPt;
		}
		public override bool hasTexCoord()
		{
			foreach( Point vertex in this.vertices)
			{
				if (vertex.texCoord == null)
					return false;
			}
			return true;
		}
		public override string ToString()
		{
			String info = $"Triangle with vertices: {vertices[0]} , {vertices[1]} , {vertices[2]}" ;
			return info;
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
/*				NOT-NORMALIZED
 *				//do triangle intersection formula with barycentric coordinates
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

				this._normal = e1.crossProduct( e2, true ); //set the normal here while we have these

				return w; //w is distance along ray of intersection point*/