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

		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3


		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
			this._lights = new List<LightSource>();
		}

		//parameter CONSTRUCTOR
		public World( List<SceneObject> objects, List<LightSource> lights)
		{
			this._objects = objects;
			this._lights = lights;
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

		//helper for checking if a ray intersects with an object in the scene.
		public float checkRayIntersection( LightRay ray )
		{
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			foreach (SceneObject obj in objects)
			{
				currW = obj.intersect( ray );

				if ((currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0))
				{
					bestW = currW;
				}

			}
			return bestW;
		}

		public Color spawnRay( LightRay ray )
		{

			Color currColor = null;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			//Color radiance = new Color( 0, 0, 0 );
			Color lightRadiance = null;
			Point intersection = null;

			foreach (SceneObject obj in objects)
			{
				currW = obj.intersect( ray );

				if ( (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0 ) )
				{
					bestW = currW;
					//currColor = obj.illuminate(); //comment out for testing CP3

					// get info for shadow ray...CP3
					Sphere s = obj as Sphere;
					Polygon t = obj as Polygon;
					if (s != null) intersection = s.getRayPoint( ray, currW );
					else if (t != null) intersection = t.getRayPoint( ray, currW );

					lightRadiance = new Color( 0f, 0f, 0f );
					foreach( LightSource light in this.lights ) //TODO - 2/20
					{
						//get normal vectors dependent on type of object. spawn shadow ray
						LightRay shadowRay = new LightRay( light.position - intersection, intersection );
						float shadowW = checkRayIntersection( shadowRay );
						if (shadowW != float.MaxValue) //the shadowRay makes it to light source unobstructed.
						{
							// reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )
							Vector reflect = Vector.reflect( shadowRay.direction, obj.normal ); // added normal field to sceneObject, may cause bugs
							IlluminationModel objLightModel = obj.lightModel;
							lightRadiance += objLightModel.illuminate( intersection, obj.normal, shadowRay, reflect, ray.direction, light, obj );
						}
						//update the currentColor
						if (currColor == null)
							currColor = lightRadiance;
						else
							currColor += lightRadiance;
					}

				} 
			}
			return currColor;
		}
	}
}
