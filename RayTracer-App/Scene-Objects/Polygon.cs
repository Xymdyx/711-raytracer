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
			return base.intersect( ray );
		}

		public override Color illuminate()
		{
			return new Color( 0.302, 0.480, 0.320 ); //return the floor color
		}
	}
}
