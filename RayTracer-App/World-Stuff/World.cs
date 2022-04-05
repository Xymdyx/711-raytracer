//USING LHS W ROW-MAJOR
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics; // for stopwatch
using RayTracer_App.Scene_Objects;
using RayTracer_App.Illumination_Models;
using RayTracer_App.Voxels;
using RayTracer_App.Kd_tree;
//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.World
{
	public class World
	{
		private List<SceneObject> _objects;
		private List<LightSource> _lights;
		private CheckerBoardPattern _checkerboard;
		private AABB _sceneBB;
		private KdTree _kdTree;
		private SceneObject _bestObj;
		private static int MAX_DEPTH = 7; //cp5 max bounces


		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3
		public CheckerBoardPattern checkerboard { get => this._checkerboard; set => this._checkerboard = value; }

		public AABB sceneBB { get => this._sceneBB; set => this._sceneBB = value; } // advanced checkpoint 1
		public Kd_tree.KdTree kdTree { get => this._kdTree; set => this._kdTree = value;  }
		public SceneObject bestObj { get => this._bestObj; set => this._bestObj = value; }



		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
			this._lights = new List<LightSource>();
			this._checkerboard = new CheckerBoardPattern();
			this._kdTree = new KdTree();
			this._sceneBB = null;
			this._bestObj = null;
		}

		//parameter CONSTRUCTOR
		public World( List<SceneObject> objects, List<LightSource> lights)
		{
			this._objects = objects;
			this._lights = lights;
			this._checkerboard = new CheckerBoardPattern();
			this._kdTree = new KdTree();
			this._sceneBB = null;
			this._bestObj = null;
		}

		//debug print
		public void printObjs()
		{
			foreach (SceneObject obj in objects)
			{
				Console.WriteLine( obj );
			}
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
				obj.transform( camViewMat ); //converts all objects to camera space

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

		// general function for finding best intersection of ray given a list ob objects
		public float findRayIntersect( LightRay ray, List<SceneObject> allObjects )
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
					this.bestObj = obj;
				}
			}
			
			return bestW;
		}

		public Color spawnRay( LightRay ray, int recDepth )
		{

			Color currColor = null;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Color lightRadiance = null;
			Point intersection = null;

			//no kdTree

			if (this.kdTree.root == null)
				bestW = findRayIntersect( ray, this.objects );
			else
				bestW = this.kdTree.travelTAB( ray, this );


			//move this out for efficiency
			if ((this.bestObj != null) && (bestW != float.MaxValue))
			{
				Sphere s = this.bestObj as Sphere;
				Polygon t = this.bestObj as Polygon;
				if (s != null) intersection = s.getRayPoint( ray, bestW );
				else if (t != null) intersection = t.getRayPoint( ray, bestW );

				IlluminationModel bestObjLightModel = this.bestObj.lightModel;

				if ((t != null) && (t.hasTexCoord())) // determine floor triangle point color
					this.bestObj.diffuse = this.checkerboard.illuminate( t, CheckerBoardPattern.DEFAULT_DIMS, CheckerBoardPattern.DEFAULT_DIMS ); //return to irradiance for TR

				currColor = bestObjLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, this.bestObj ); //return to irradiance for TR

				//cp5 
				if (recDepth < MAX_DEPTH)
				{
					//need this since we recurse and may update bestObj
					SceneObject localBest = this.bestObj;
					Color recColor = null;

					if (this.bestObj.kRefl > 0)
					{
						//importance sampling here if on
						LightRay reflRay = new LightRay( Vector.reflect( -ray.direction, this.bestObj.normal ), intersection );
						recColor = spawnRay( reflRay, recDepth + 1 );

						if (recColor != null)
							currColor = recColor.scale( localBest.kRefl );
						else
							currColor = recColor;
					}
				//https://phet.colorado.edu/sims/html/bending-light/latest/bending-light_en.html
					if (this.bestObj.kTrans > 0) //cp6 TODO, handle ray passing through an object!
					{
						//spawn transmission ray
						Vector transDir;
						
						float transBias = 1e-6f;
						Vector transDisplacement = -(localBest.normal).scale( transBias );
						Point transOrigin = intersection + transDisplacement;
						//Point transOrigin = intersection.displaceMe( -(localBest.normal) ) ;

						if ( this.bestObj.normal.dotProduct( ray.direction) >= 0)
							transDir = Vector.transmit( ray.direction, this.bestObj.normal, SceneObject.AIR_REF_INDEX, this.bestObj.refIndex );
						else 
							transDir = Vector.transmit( ray.direction, -this.bestObj.normal, this.bestObj.refIndex, SceneObject.AIR_REF_INDEX ); // use negative normal, use object refIdx as ni

						LightRay translRay = new LightRay( transDir, transOrigin );
						ray.entryPt = intersection; //keep track of if we're in an object or not
						recColor = spawnRay( translRay, recDepth + 1 );
						ray.entryPt = null; // we've exited

						if (recColor != null)
							currColor = recColor.scale( localBest.kTrans );
						else
							currColor = recColor;

					}

				}
			}
			return currColor;
		}

/* KDTREE METHODS */

		// set a boundibg box
		// axis.. 0 =x, y =1, 2 =z
		public void setBB( Point p1, Point p2 , int axis = 0 )
		{
			//hardcoded BB that works
			this._sceneBB = new AABB( p1, p2, axis );
		}

		// find max and min x,y,z vals for whole scene ( the front upper-left and back bottom-right corners of the AABB
		// form AABB points from these
		// pass to aabb		public void findBB( int boxAxis = 0 )
		public void findBB( int boxAxis = 0)
		{

			float[] max = new float[3];
			float[] min = new float[3];

			float[] sceneMax = { float.MinValue, float.MinValue, float.MinValue };
			float[] sceneMin = { float.MaxValue, float.MaxValue, float.MaxValue };


			if (this._sceneBB == null)
			{
				foreach (SceneObject obj in objects)
				{
					Polygon t = obj as Polygon;
					Sphere s = obj as Sphere;

					//find the max and min x,y,z vals for each point and compare against scene
					for (int axis = 0; axis < 3; axis++)
					{
						if (t != null)
						{
							max[axis] = t.getMaxPt( axis ).getAxisCoord( axis );
							min[axis] = t.getMinPt( axis ).getAxisCoord( axis );
						}
						else if (s != null)
						{
							max[axis] = s.getMaxPt( axis ).getAxisCoord( axis );
							min[axis] = s.getMinPt( axis ).getAxisCoord( axis );
						}

						//update scene min and max points if applicable
						if (max[axis] > sceneMax[axis])
							sceneMax[axis] = max[axis];
						if (min[axis] < sceneMin[axis])
							sceneMin[axis] = min[axis];
					}

				}

				//set AABB for the scene
				Point minPt = new Point( sceneMin[0], sceneMin[1], sceneMin[2] );
				Point maxPt = new Point( sceneMax[0], sceneMax[1], sceneMax[2] );
				this._sceneBB = new AABB( minPt, maxPt, boxAxis );
			}
		}

		//builds the kdTree for the world
		public void buildKd()
		{
			//time building tree start
			Stopwatch kdTimer = new Stopwatch();
			kdTimer.Start();
			kdTree.maxLeafObjs = this.objects.Count / 2;
			kdTree.root = kdTree.getNode( this.objects, this.sceneBB, 0 );
			kdTimer.Stop();
			Console.WriteLine( "Building the kd tree took " + (kdTimer.ElapsedMilliseconds) + " milliseconds" );
		}
	}
}
