using System;
using System.Numerics;


//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class SceneObject
	{
		//fields
		private string _material;
		private Vector _normal;

		//properties 
		public string material {get => this._material; set => this._material = value; }
		public Vector normal { get => this._normal; set => this._normal = value; }

		//constructors
		public SceneObject() { _material = "None"; }

		public SceneObject( string material )
		{
			this._material = material;
		}


		//return distance along ray where it intersects an object....
		public virtual float intersect( LightRay ray )
		{
			return 0.0f;
		}

		public virtual Point getRayPoint( LightRay ray, float w )
		{
			return new Point( 0f, 0f, 0f );
		}


		public virtual void transform( Matrix4x4 camViewMat ){ return; }

		public virtual Color illuminate()
		{
			return new Color(); //return the background color
		}

		//override ToString()
	}
}
