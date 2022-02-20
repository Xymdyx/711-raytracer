//USING LHS W ROW-MAJOR
using System;
using System.Numerics;
using System.Collections.Generic;

using RayTracer_App.Scene_Objects;
using RayTracer_App.Illumination_Models;
//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.World
{
	public class World
	{
		private List<SceneObject> _objects;
		private List<LightSource> _lights;
		private IlluminationModel _lightModel;
		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3
		public IlluminationModel lightModel { get => this._lightModel; set => this._lightModel = value; }


		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
			this._lights = new List<LightSource>();
			this._lightModel = Phong.regularPhong;
		}

		//parameter CONSTRUCTOR
		public World( List<SceneObject> objects, List<LightSource> lights)
		{
			this._objects = objects;
			this._lights = lights;
			this._lightModel = Phong.regularPhong;
		}

		//full CONSTRUCTOR
		public World( List<SceneObject> objects, List<LightSource> lights, IlluminationModel lightModel )
		{
			this._objects = objects;
			this._lights = lights;
			this._lightModel = lightModel;
		}

		// add object to objectlist
		public void addObject( SceneObject obj )
		{
			this._objects.Add( obj );
			return;
		}

		// add lightSource to lightList
		public void addLight( LightSource light )
		{
			this._lights.Add( light );
			return;
		}

		//transform stub
		public void transform( SceneObject obj, Matrix4x4 camViewMat )
		{
			_objects.Find( item => item.Equals( obj ) ).transform( camViewMat );
			return;
		}

		//transformAll iterates through all objects and transforms them.
		public void transformAll( Matrix4x4 camViewMat)
		{
			//transforming before and after camera transform gives same results, so long as it is done before the ray shooting
			 foreach (SceneObject obj in objects)
			{
				obj.transform( camViewMat ); //converts all objects to camera space
			}

			return;
		}
		
		public Color spawnRay( LightRay ray )
		{

			Color currColor = null;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			float radiance = 0.0f;
			Point intersection = null;

			foreach (SceneObject obj in objects)
			{
				currW = obj.intersect( ray );

				if ( (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0 ) )
				{
					//TOD get intersection point, make object switch case
					//get normal vectors dependent on type of object. spawn shadow ray
					bestW = currW;
					currColor = obj.illuminate();

					// get info for shadow ray...
					Sphere s = obj as Sphere;
					Polygon t = obj as Polygon;
					if (s != null) intersection = s.getRayPoint( ray, currW );
					else if (t != null) intersection = t.getRayPoint( ray, currW );

					foreach( LightSource light in this.lights ) //TODO - 2/20
					{
						Vector shadowRay = light.position - intersection;
						Vector reflect = Vector.reflect( shadowRay, obj.normal ); // added normal field to sceneObject, may cause bugs
						radiance = lightModel.illuminate( intersection, obj.normal, shadowRay, reflect, ray.direction, this.lights );
						
					}
				} 
			}
			return currColor;
		}
	}
}
