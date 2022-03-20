//USING LHS W ROW-MAJOR
using System;
using System.Numerics;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.Illumination_Models;
using RayTracer_App.Voxels;
//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.World
{
	public class World
	{
		private List<SceneObject> _objects;
		private List<LightSource> _lights;
		private AABB _sceneBB;
		private Kd_tree.KdTree _kdTree;

		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3

		public AABB sceneBB { get => this._sceneBB; set => this._sceneBB = value; } // advanced checkpoint 1
		public Kd_tree.KdTree kdTree { get => this._kdTree; set => this._kdTree = value;  } 


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
					currColor = objLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, obj ); //return to irradiance for TR
				} 
			}
			return currColor;
		}

		public void findBB( int axis = 0) 
		{
			Point max = null;
			Point min = null;
			Point realMax = null;
			Point realMin = null;

			if (this._sceneBB == null)
			{
				foreach( SceneObject obj in objects)
				{
					Polygon t = obj as Polygon;
					Sphere s = obj as Sphere;
					if( t != null ){
						max = t.getMaxPt( axis );
						min = t.getMinPt( axis );
					} 
					else if ( s != null)
					{
						max = s.getMaxPt( axis );
						min = s.getMinPt( axis );
					}

					if (max.getAxisCoord(axis) > realMax.getAxisCoord(axis) ) realMax = max;
					if (min.getAxisCoord(axis) < realMin.getAxisCoord(axis) ) realMin = min;
				}

				this._sceneBB = new AABB( realMax, realMin, axis );
			}
		}
	}

	/*For illuminate when main logic was done in spawnRay
	 * 
	//1. migrate all of this logic to the Phong illuminate method
	//lightRadiance = Color.defaultBlack;
	//foreach( LightSource light in this.lights ) //TODO - 2/20
	//{
	//	//get normal vectors dependent on type of object. spawn shadow ray
	//	LightRay shadowRay = new LightRay( light.position - intersection, intersection );
	//	float shadowW = checkRayIntersection( shadowRay, this.objects, obj );
	//	if (shadowW == float.MaxValue) //the shadowRay makes it to light source unobstructed.
	//	{
	//		// reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )
	//		Vector reflect = Vector.reflect( intersection - light.position, obj.normal ); // added normal field to sceneObject, may cause bugs
	//		Vector halfWay = -ray.direction + -shadowRay.direction;
	//		IlluminationModel objLightModel = obj.lightModel;
	//		//objLightModel.illuminate( intersection, obj.normal, shadowRay, reflect, -ray.direction, light, obj ); Phong
	//		//Phong blinn- replace reflection with halfway
	//		lightRadiance += objLightModel.illuminate( intersection, obj.normal, shadowRay, reflect, -ray.direction, light, obj );
	//	}
	//	//update the currentColor
	//	if (currColor == null)
	//		currColor = lightRadiance;
	//	else
	//		currColor += lightRadiance;
	//}
*/
}
