using System;
using System.Numerics;


//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class SceneObject
	{
		//fields
		private string _material;

		//properties 
		public string material {get => this._material; set => this._material = value; }

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

		public virtual void transform( Matrix4x4 camViewMat ){ return; }

		public virtual Color illuminate()
		{
			return new Color(); //return the background color
		}

		//override ToString()
	}
}
