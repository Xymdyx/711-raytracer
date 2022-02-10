using System;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;

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
		public override double intersect( LightRay ray )
		{
			//HACK... put triangle case in here
			double w = Double.MaxValue;

			if (this.vertices.Count == 3)
			{
				//do triangle intersection formula with barycentric coordinates

				//use (w,u,v) = (1/(P . e1)) * ( Q . e2, P . T, Q. D)
				Vector e1 = vertices[1] - vertices[0]; // cross, dot, and normalize good
				Vector e2 = vertices[2] - vertices[0]; //this may be the issue
				Vector P = ray.direction.crossProduct( e2, false );


				double denom = P.dotProduct( e1 );

				if (( denom >= 0 && denom <= 1e-8) || denom == Double.NaN) return Double.MaxValue;  // ray is parallel to triangle

				double denomScale = 1 / denom;

				Vector T = ray.origin - vertices[0];
				double u = P.dotProduct( T ) * denomScale;

				if (u < 0 || u > 1) return Double.MaxValue;

				Vector Q = T.crossProduct( e1, false );
				double v = Q.dotProduct( ray.direction ) * denomScale;

				if (v < 0 || u + v > 1) return Double.MaxValue;
				
				w = Q.dotProduct( e2 ) * denomScale;

				// where is our point?
				if (w < 0 || w == Double.NaN) return Double.MaxValue; // intersection behind origin

				Vector normal = e1.crossProduct( e2, false );
				//u,v are barycentric boordsinates of intersection point
				return w; //w is distance along ray of intersection point
			}

			return w;
		}

		public override Color illuminate()
		{
			return new Color( 1.0, 0.0, 0.0 ); //return the floor color
		}

		public override void transform( Matrix4d camViewMat )
		{
			//TODO set up my polygon in camera coordinates before rendering the world
			// use post-multiply since I am using column-major... Vnew = B*A*Vold
			foreach (Point vertex in vertices)
			{
				Matrix4d ptHmg = vertex.toHmgCoords(); // 4x4 with only 1st column having x, y, z, w...Rest is 0s.
				Matrix4d newVertMat = camViewMat * ptHmg; // we postMultiply since we are is LHS
				vertex.fromHmgCoords( newVertMat ); // [x y z w] => (x/w, y/w, z/w) CP form.. DONE -- MATRIX-MULTI works
			}
		}
	}
}
