//Use a Right-Handed Coordinate system and column-major matrices
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math;

using RayTracer_App.Scene_Objects;

//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.World
{
	public class World
	{
		private List<SceneObject> _objects;
		//private int[] attributes;

		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }

		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
		}

		//parameter CONSTRUCTOR
		public World( List<SceneObject> objects )
		{
			this._objects = objects;
		}

		// add object to objectlist
		public void add( SceneObject obj )
		{
			this._objects.Add( obj );
			return;
		}

		//transform stub
		public void transform( SceneObject obj, Matrix4x4 camViewMat )
		{
			_objects.Find( item => item.Equals( obj ) ).transform( camViewMat );
			return;
		}

		//transformAll stub
		public void transformAll( Matrix4x4 camViewMat)
		{
			 foreach (SceneObject obj in objects)
			{
				obj.transform( camViewMat ); //converts all objects to camera space
			}

			return;
		}
		
		//TODO implement spawnRay
		public Color spawnRay( LightRay ray )
		{

			Color currColor = null;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;

			foreach (SceneObject obj in objects)
			{
				currW = obj.intersect( ray );

				if ( (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0 ) )
				{
					bestW = currW;
					currColor = obj.illuminate();
				} 
			}
			return currColor;
		}
	}
}
