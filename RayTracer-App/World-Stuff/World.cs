//USING LHS W ROW-MAJOR
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics; // for stopwatch
using RayTracer_App.Scene_Objects;
using RayTracer_App.Illumination_Models;
using RayTracer_App.Voxels;
using RayTracer_App.Kd_tree;
using RayTracer_App.Photon_Mapping;
//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.World
{
	public class World
	{
		private static int MAX_DEPTH = 10; //cp6 max bounces

		//fields
		private List<SceneObject> _objects;
		private List<LightSource> _lights;
		private CheckerBoardPattern _checkerboard;
		private AABB _sceneBB;
		private KdTree _kdTree;
		private SceneObject _bestObj;
		private PhotonRNG _photonMapper;

		//private int[] attributes;
		public List<SceneObject> objects { get => this._objects; set => this._objects = value; }
		public List<LightSource> lights { get => this._lights ; set => this._lights = value; } // checkpoint 3
		public CheckerBoardPattern checkerboard { get => this._checkerboard; set => this._checkerboard = value; }

		//advCP1
		public AABB sceneBB { get => this._sceneBB; set => this._sceneBB = value; } // advanced checkpoint 1
		public Kd_tree.KdTree kdTree { get => this._kdTree; set => this._kdTree = value;  }
		
		//refactor in cp6
		public SceneObject bestObj { get => this._bestObj; set => this._bestObj = value; }

		//advCp2 Pm
		public PhotonRNG photonMapper { get => this._photonMapper; set => this._photonMapper = value; }



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

			foreach (LightSource ls in lights)
				ls.transform( camViewMat);

			return;
		}

		// helper for getting ray intersection point.. used by PM
		public Point grabIntersectPt( LightRay ray, float w )
		{
			Point intersection = null;
			Sphere s = this.bestObj as Sphere;
			Polygon t = this.bestObj as Polygon;
			if (s != null) intersection = s.getRayPoint( ray, w );
			else if (t != null) intersection = t.getRayPoint( ray, w );

			return intersection;
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

		//helper for checking if a ray intersects with an object in the scene.
		// added shadowBias displacement for shadowRays so I no longer have to check the current object
		public static SceneObject checkRayIntersectionObj( LightRay ray, List<SceneObject> allObjects, LightSource light = null )
		{
			float currW = float.MaxValue;
			float litW = ray.origin.distance( light.position );
			SceneObject collided = null;
			foreach (SceneObject obj in allObjects)
			{
				currW = obj.intersect( ray );

				if ((currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue) && (currW > 0) && (currW < litW) )
				{
					collided = obj;
					break;
				}
			}
			return collided;
		}

		// general function for finding best intersection of ray given a list ob objects
		public float findRayIntersect( LightRay ray, List<SceneObject> allObjs = null )
		{
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			List<SceneObject> objs = allObjs;

			if (allObjs == null)
				objs = this.objects;

			foreach (SceneObject obj in objs)
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

		//method for finding the color when a ray hits an object. Whitted method
		// TODO: Decompose into smaller methods
		public Color spawnRay( LightRay ray, int recDepth )
		{

			Color currColor = Color.bgColor;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Color lightRadiance = null;
			Point intersection = null;

			//no kdTree
			if (this.kdTree.root == null)
				bestW = findRayIntersect( ray );
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

				currColor = bestObjLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, this.bestObj, true ); //return to irradiance for TR

				if (recDepth < MAX_DEPTH)
				{
					//need this since we recurse and may update bestObj
					SceneObject localBest = this.bestObj;
					Color recColor = null;

					//refraction check
					Vector nHit = localBest.normal;
					bool inside = false;
					float checkDp = nHit.dotProduct( ray.direction );

					if (checkDp > 0) // issue was using Math.Min for return in sphere intersect
					{
						nHit = -nHit;
						inside = true;
					}

					//reflection
					if (this.bestObj.kRefl > 0)
					{
						//importance sampling here if on
						Point reflOrigin;
						//Vector reflDir = Vector.reflect( -ray.direction, localBest.normal );
						Vector reflDir = Vector.reflect2( ray.direction, localBest.normal ); //equivalent way with what I did for Phong. But this is just reflecting back the same ray

						if (reflDir.dotProduct( localBest.normal ) < 0)
							reflOrigin = intersection.displaceMe( -localBest.normal );
						else
							reflOrigin = intersection.displaceMe( localBest.normal );

						LightRay reflRay = new LightRay( reflDir, reflOrigin );
						recColor = spawnRay( reflRay, recDepth + 1 );

						if (recColor != null)
							currColor += recColor.scale( localBest.kRefl );

					}

					//refraction - cp6
					//https://phet.colorado.edu/sims/html/bending-light/latest/bending-light_en.html... app
					//https://www.scratchapixel.com/code.php?id=8&origin=/lessons/3d-basic-rendering/ray-tracing-overview... better
					if (this.bestObj.kTrans > 0) //cp6 TODO, handle ray passing through an object!
					{
						//spawn transmission ray
						Vector transDir;
						Point transOrigin;

						if (inside) //these do alternate
						{
							//Console.WriteLine( "in" );
							transDir = Vector.transmit( ray.direction, nHit, localBest.refIndex, SceneObject.AIR_REF_INDEX );
						}
						else
						{
							//Console.WriteLine( "out" );
							transDir = Vector.transmit( ray.direction, nHit, SceneObject.AIR_REF_INDEX, localBest.refIndex );
						}

						if ( transDir.dotProduct( localBest.normal) < 0 )
							transOrigin = intersection.displaceMe( -localBest.normal );
						else
							transOrigin = intersection.displaceMe( localBest.normal );

						LightRay translRay = new LightRay( transDir, transOrigin );
						ray.entryPt = intersection; //keep track of if we're in an object or not

						recColor = spawnRay( translRay, recDepth + 1 );

						ray.entryPt = null; // we've exited

						if (recColor != null)
							currColor += recColor.scale( localBest.kTrans );

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
			//time building tree start.... this sometimes still has issues...
			Stopwatch kdTimer = new Stopwatch();
			kdTimer.Start();
			kdTree.maxLeafObjs = (int) Math.Ceiling( (decimal) this.objects.Count / 2 );
			kdTree.root = kdTree.getNode( this.objects, this.sceneBB, 0 );
			kdTimer.Stop();
			Console.WriteLine( "Building the kd tree took " + (kdTimer.ElapsedMilliseconds) + " milliseconds" );
		}

		//PHOTON-MAPPING METHODS
		//called by lightsources in the scene when shooting photons. 
		// uses Russian roulette to determine photons' fates. Stores photons in the PhotonMapper
		public void tracePhoton( LightRay photonRay, int depth )
		{
			float bestW = float.MaxValue;
			Color flux = null;
			Point intersection = null;
			PhotonRNG.RR_OUTCOMES rrOutcome;
			//no kdTree
			bestW = findRayIntersect( photonRay );

			//move this out for efficiency
			if ((this.bestObj != null) && (bestW != float.MaxValue))
			{
				intersection = grabIntersectPt( photonRay, bestW );
				if (intersection == null)
				{
					Console.WriteLine( " Null intersection return... Aborting!" );
					Environment.Exit( -1 );
				}

				IlluminationModel bestObjLightModel = this.bestObj.lightModel;
				Polygon t = this.bestObj as Polygon;
				if ((t != null) && (t.hasTexCoord())) // determine floor triangle point color
					this.bestObj.diffuse = this.checkerboard.illuminate( t, CheckerBoardPattern.DEFAULT_DIMS, CheckerBoardPattern.DEFAULT_DIMS ); //return to irradiance for TR

				flux = bestObjLightModel.illuminate( intersection, -photonRay.direction, this.lights, this.objects, this.bestObj, true ); //return to irradiance for TR

				//Russian roulette to determine if we go again or not...
				rrOutcome = this.photonMapper.RussianRoulette(bestObjLightModel.kd, bestObjLightModel.ks);
				Vector travelDir;
				switch(rrOutcome)
				{
					case PhotonRNG.RR_OUTCOMES.DIFFUSE:
						//diffuse reflection via Monte Carlo
						break;
					case PhotonRNG.RR_OUTCOMES.SPECULAR:
						travelDir = Vector.reflect2( photonRay.direction, this.bestObj.normal );
						break;
					case PhotonRNG.RR_OUTCOMES.ABSORB:
						// stop tracing
						break;
					default:
						break;
				}
			}

			/*photonW = world.findRayIntersect( photonRay );
			photonPos = world.grabIntersectPt( photonRay, photonW );
			Photon photon = new Photon( photonPos, photonPow, )*/
			return;
		}
	}
}

/* issue...
 // gives more or less same direction back...
Vector reflDir = Vector.reflect2( ray.direction, localBest.normal );
{Vector (u1, u2, u3) = (0.12933731, 0.3664578 , 0.92140144)
}

Vector reflDir = Vector.reflect( -ray.direction, localBest.normal );
{Vector (u1, u2, u3) = (0.12933731, 0.3664578 , 0.92140144)
 */