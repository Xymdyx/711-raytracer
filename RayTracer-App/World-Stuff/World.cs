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
		private CheckerBoardPattern _checkerboard;

		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3
		public CheckerBoardPattern checkerboard { get => this._checkerboard; set => this._checkerboard = value; }


		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
			this._lights = new List<LightSource>();
			this._checkerboard = new CheckerBoardPattern();
		}

		//parameter CONSTRUCTOR
		public World( List<SceneObject> objects, List<LightSource> lights)
		{
			this._objects = objects;
			this._lights = lights;
			this._checkerboard = new CheckerBoardPattern();
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
		// added shadowBias displacement for shadowRays so I no longer have to check the current object
		public static float checkRayIntersection( LightRay ray, List<SceneObject> allObjects )
		{
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			foreach (SceneObject obj in allObjects)
			{
				currW = obj.intersect( ray );

				if ((currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0))
				{
					bestW = currW;
					break;
				}

			}
			return bestW;
		}

		public Color spawnRay( LightRay ray )
		{

			Color currColor = null;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Color lightRadiance = null;
			Point intersection = null;

			foreach (SceneObject obj in objects)
			{
				currW = obj.intersect( ray );

				if ( (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW < bestW) && (currW > 0 ) )
				{
					bestW = currW;
					currColor = null; //reset the color since we know we're overwriting it

					Sphere s = obj as Sphere;
					Polygon t = obj as Polygon;
					if (s != null) intersection = s.getRayPoint( ray, currW );
					else if (t != null) intersection = t.getRayPoint( ray, currW );

					IlluminationModel objLightModel = obj.lightModel;

					if (t != null) // determine triangle point color
						obj.diffuse = this.checkerboard.illuminate( t, 10, 10 ); //return to irradiance for TR

					currColor = objLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, obj ); //return to irradiance for TR


				}
			}
			return currColor;
		}
	}
}
