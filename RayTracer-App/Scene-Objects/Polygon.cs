using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Scene_Objects
{
	public class Polygon : SceneObject
	{
		private List<Point> _vertices;
		private Vector _normal;


		public List<Point> vertices { get => this._vertices; set => this._vertices = value; } 
		public Vector normal { get => this._normal = normal; set => this._normal = value; }

// default constructor for a plain triangle
		public Polygon()
		{
			this._vertices = new List<Point> { new Point( 1, 0, 0 ), new Point( 0, 1, 0 ), new Point( -1, 0, 0 ) };
			this.normal = new Point();
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
			double w = double.MinValue;

			if (this.vertices.Count == 3)
			{
				//do triangle intersection formula with barycentric coordinates

				//use (w,u,v) = (1/(P . e1)) * ( Q . e2, P . T, Q. D)
				Vector e1 = vertices[1] - vertices[0];
				Vector e2 = vertices[2] - vertices[0];
				Vector T = ray.origin - vertices[0];
				Vector Q = T.crossProduct( e1 );
				Vector P = ray.direction.crossProduct( e2 );
				double denom = P.dotProduct( e1 );

				w = Q.dotProduct( e2 ) / denom;
				double u = P.dotProduct( T ) / denom;
				double v = Q.dotProduct( ray.direction ) / denom;

				// where is our point?
				if (P.isZeroVector() || e1.isZeroVector()) w = double.MinValue;  // ray is parallel to triangle
				else if (w < 0) w = double.MaxValue; // intersection behind origin
				else if ((u < 0) || (v < 0) || (u + v > 1)) w = double.NaN; //outside of triangle
				else
				{
					Vector normal = e1.crossProduct( e2 );
					//u,v are barycentric boordsinates of intersection point
					return w; //w is distance along ray of intersection point
				}

			}

			else { } //implement polygon-ray intersection if time permist
			return w;
		}

		public override Color illuminate()
		{
			return new Color( 0.302, 0.480, 0.320 ); //return the floor color
		}
	}
}
