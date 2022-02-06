using System;
using System.Collections.Generic;
using System.Text;

using RayTracer_App.Scene_Objects;

namespace RayTracer_App.World_Stuff
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
		public void transform( SceneObject obj )
		{
			_objects.Find( item => item.Equals( obj ) ).transform();
			return;
		}

		//transformAll stub
		public void transformAll()
		{
			 foreach (SceneObject obj in objects)
			{
				obj.transform();
			}

			return;
		}
		
		//TODO implement spawnRay
		public void spawnRay( Vector ray )
		{
			return;
		}
		//snap( this )
	}
}
