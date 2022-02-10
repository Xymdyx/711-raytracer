using System;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;

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
		public virtual double intersect( LightRay ray )
		{
			return 0.0;
		}


		public virtual void transform( Matrix4d camViewMat ){ return; }

		public virtual Color illuminate()
		{
			return new Color(); //return the background color
		}

		//override ToString()
	}
}
