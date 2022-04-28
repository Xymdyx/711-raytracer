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
		private int _photoHits;
		private int _causticHits;
		private bool pmOn;

		public int highestK = 0;
		public int allK = 0;

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

		// advCp2 debug
		public int photoHits { get => this._photoHits; set => this._photoHits = value; }
		public int causticHits { get => this._causticHits; set => this._causticHits = value; }


		//default CONSTRUCTOR
		public World()
		{
			this._objects = new List<SceneObject>();
			this._lights = new List<LightSource>();
			this._checkerboard = new CheckerBoardPattern();
			this._kdTree = new KdTree();
			this._sceneBB = null;
			this._bestObj = null;
			this.pmOn = false;
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
			this.pmOn = false;
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

		//helper for offsetting intersection for next ray
		public Point offsetIntersect( Point intersection, Vector outgoing, Vector normal )
		{
			if (outgoing.dotProduct( normal ) < 0)
				return intersection.displaceMe( -normal );
			else
				return intersection.displaceMe( normal );
		}

		//recursive method for finding the color when a ray hits an object. Whitted recursive method
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
				currColor = Color.defaultBlack;
				Sphere s = this.bestObj as Sphere;
				Polygon t = this.bestObj as Polygon;
				if (s != null) intersection = s.getRayPoint( ray, bestW );
				else if (t != null) intersection = t.getRayPoint( ray, bestW );

				IlluminationModel bestObjLightModel = this.bestObj.lightModel;

				if ((t != null) && (t.hasTexCoord())) // determine floor triangle point color
					this.bestObj.diffuse = this.checkerboard.illuminate( t, CheckerBoardPattern.DEFAULT_DIMS, CheckerBoardPattern.DEFAULT_DIMS ); //return to irradiance for TR

				//if(bestObj.kTrans != 0 || bestObj.kRefl != 0) //specular stuff
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
						reflOrigin = offsetIntersect( intersection, reflDir, localBest.normal );
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
							transDir = Vector.transmit( ray.direction, nHit, localBest.refIndex, SceneObject.AIR_REF_INDEX );
						else
							transDir = Vector.transmit( ray.direction, nHit, SceneObject.AIR_REF_INDEX, localBest.refIndex );

						//if ( transDir.dotProduct( localBest.normal) < 0 )
						//	transOrigin = intersection.displaceMe( -localBest.normal );
						//else
						//	transOrigin = intersection.displaceMe( localBest.normal );
						transOrigin = offsetIntersect( intersection, transDir, localBest.normal );

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

		//called by the camera to start Photon mapping
		public void beginpmPassOne()
		{
			this.photonMapper = new PhotonRNG();

			foreach (LightSource l in this.lights) //187 visible using BFS photon list check on 500. Only lose about 30 with kd tree visuals, which makes sense
				l.emitGlobalPhotonsFromDPLS( this); //rendering took 34 minutes with 20k photons. All of these wind up in scene.

			//construct photon maps from lists
			photonMapper.makePMs();
		}

		//helper for finding correct diffuse direction for Monte Carlo sampling
		private Vector getRightDiffuse( IlluminationModel model, float u1, float u2, Vector normal = null )
		{
			Phong p = model as Phong;
			PhongBlinn pb = model as PhongBlinn;
			Vector hemiVec = Vector.ZERO_VEC;

			if (p == null && pb == null) //error
			{
				Console.WriteLine( "No illumination model aborting..." );
				Environment.Exit( -1 );
			}

			if (p != null)
			{
				 hemiVec = p.mcDiffuseDir( u1, u2, normal );
			}
			else if (pb != null)
			{
				hemiVec = pb.mcDiffuseDir( u1, u2, normal );
			}
			return hemiVec;
		}

		//called by lightsources in the scene when shooting photons. 
		// uses Russian roulette to determine photons' fates. Stores photons in the PhotonMapper
		// we don't focus our shoots like we do for the caustic pass		
		//https://users.csc.calpoly.edu/~zwood/teaching/csc572/final15/dschulz/index.html... photon implementation with params
		public void tracePhoton( LightRay photonRay, int depth, bool fromSpec = false, bool transmitting = false )
		{
			//TODO SHOOT BETTER.... //TODO try having it where the Photon absorbs color along its path.
			float bestW = float.MaxValue;
			Color flux = null;
			Point intersection = null;
			PhotonRNG.RR_OUTCOMES rrOutcome = PhotonRNG.RR_OUTCOMES.TRANSMIT; //assume we're transmitting for simplicity.
			//no kdTree for now TODO
			bestW = findRayIntersect( photonRay );

			//move this out for efficiency
			if ((this.bestObj != null) && (bestW != float.MaxValue))		// what happens to the light stored in photon ray
			{
				intersection = grabIntersectPt( photonRay, bestW );
				if (intersection == null)
				{
					Console.WriteLine( " Null intersection return... Aborting!" );
					Environment.Exit( -1 );
				}

				IlluminationModel bestObjLightModel = this.bestObj.lightModel;
				Phong p = bestObjLightModel as Phong;
				PhongBlinn pb = bestObjLightModel as PhongBlinn;

				if (p == null && pb == null) //error
				{
					Console.WriteLine( "No illumination model aborting..." );
					Environment.Exit( -1 );
				}

				//Russian roulette to determine if we go again or not...Only when we are outside objects for now
				if (!transmitting)
					rrOutcome = this.photonMapper.RussianRoulette( bestObjLightModel.kd, bestObjLightModel.ks, bestObj.kRefl, bestObj.kTrans );

				//debug
				if (bestObj as Sphere != null)
					Console.WriteLine( "Hit sphere" );

				flux = bestObjLightModel.illuminate( intersection, photonRay.direction, this.lights, this.objects, this.bestObj, true, true ); //last flag is for giving photons a pass from shadows
				if (flux.isShadowed())
					this.photonMapper.powerless++;

				float u1 = this.photonMapper.random01();
				float u2 = this.photonMapper.random01();
				Vector travelDir = Vector.ZERO_VEC;
				bool causticsMark = fromSpec;
				bool transMark = transmitting;
				Point pOrigin = offsetIntersect( intersection, travelDir, bestObj.normal );

				switch (rrOutcome) //am I properly extending this for transmission??? - 4/24
				{
					case PhotonRNG.RR_OUTCOMES.DIFFUSE:
						travelDir = getRightDiffuse( bestObjLightModel, u1, u2, bestObj.normal ); //photon's flux needs to be multiplied by  stored surface color TODO???? -4/24
						this.photonMapper.addGlobal( pOrigin.copy(), photonRay.direction.v1, photonRay.direction.v2, photonRay.direction.copy(), flux, 1.0f );
						if (causticsMark)
						{
							this.photonMapper.addCaustic( pOrigin.copy(), photonRay.direction.v1, photonRay.direction.v2, photonRay.direction.copy(), flux, 1.0f );
							causticsMark = false; 
						}
						break;
					case PhotonRNG.RR_OUTCOMES.SPECULAR:
						travelDir = Vector.reflect2( photonRay.direction, this.bestObj.normal );
						causticsMark = true;
						break;
					case PhotonRNG.RR_OUTCOMES.TRANSMIT:
						travelDir = Vector.transmit2( photonRay.direction, this.bestObj.normal, SceneObject.AIR_REF_INDEX, bestObj.refIndex );
						transMark = !transMark; //toggle current transmission setting since we're only doing single refraction
						causticsMark = true;
						break;
					case PhotonRNG.RR_OUTCOMES.ABSORB:
						if (bestObj.kRefl == 0 && bestObj.kTrans == 0) //we only store at NON-SPECULAR surfaces
						{
							this.photonMapper.addGlobal( pOrigin.copy(), photonRay.direction.v1, photonRay.direction.v2, photonRay.direction.copy(), flux, 1.0f );
							if (causticsMark)
								this.photonMapper.addCaustic( pOrigin.copy(), photonRay.direction.v1, photonRay.direction.v2, photonRay.direction.copy(), flux, 1.0f );
						}
						break;
					default:
						break;
				}

				if (!travelDir.isZeroVector() && depth <= PhotonRNG.MAX_SHOOT_DEPTH)
				{ //we survived
					LightRay pRay = new LightRay( travelDir, pOrigin);
					tracePhoton( pRay, depth + 1, causticsMark, transMark );
				}

				return; // end of path for this photon
			}
			return;
		}

		//helper to call PhotonMapper to find photon intersections. I don't think the TAB algo visits all of them
		public Color overlayPhotons( LightRay ray, bool kdMode = false)
		{
			PhotonRNG.MAP_TYPE mapVal = PhotonRNG.MAP_TYPE.NONE;

			//ascending order of preference
			if (!kdMode && photonMapper.intersectListQuick( ray, PhotonRNG.MAP_TYPE.GLOBAL ))
				mapVal = PhotonRNG.MAP_TYPE.GLOBAL;
			else if (kdMode && ( photonMapper.intersectPMFull( ray, this, PhotonRNG.MAP_TYPE.GLOBAL ) != float.MaxValue) )
				mapVal = PhotonRNG.MAP_TYPE.GLOBAL;

			if (!kdMode && photonMapper.intersectListQuick( ray, PhotonRNG.MAP_TYPE.CAUSTIC ) )
				mapVal = PhotonRNG.MAP_TYPE.CAUSTIC;
			else if (kdMode && (photonMapper.intersectPMFull( ray, this, PhotonRNG.MAP_TYPE.CAUSTIC ) != float.MaxValue))
				mapVal = PhotonRNG.MAP_TYPE.CAUSTIC;

			if (mapVal != PhotonRNG.MAP_TYPE.NONE) photoHits++;
			if (mapVal == PhotonRNG.MAP_TYPE.CAUSTIC) causticHits++;

			return photonMapper.getPColorbyType( mapVal ); // nothing if black
		}

		//do pass two here, where we render and gather the N nearest photons in a sphere of radius r
		// to compute indirect illumination and solve the rendering equation's 4 integrals
		public void beginpmPassTwo()
		{
			//as far as I can tell the photons are around, even if my visualization algo doesn't pick up all of them
			this.pmOn = true;
			return;
		}

/* *collect the k nearest photons and make calculation for global and caustic PMs
* Direct & specular illumination calculated using MC Raytracing
* Caustics estimated using caustic map, never MC raytracing
* indirect illumination comes from photon maps
* figure out caustics and indirect illumination
** implement a cone filter if ambitious */
		public Color callPhotons( Point intersection, Vector outgoing, Vector objNormal ) //incoming to a point, outgoing from a point to the eye...
		{
			Color photonAdditive = Color.defaultBlack;
			if (pmOn && intersection != null)
			{
				float defRadius = PhotonRNG.DEF_SEARCH_RAD; //this is r^2 already
				MaxHeap<Photon> nearestPhotons =
					this.photonMapper.kNearestPhotons( intersection, PhotonRNG.K_PHOTONS, defRadius );

				if ( nearestPhotons.heapSize >= 5) //per Jensen's implementation. Reduces noise... 4/24
				{
					float circleRad = (float)nearestPhotons.doubleMazHeap[1]; //stored as r^2
					float radRoot = (float) Math.Sqrt(circleRad);
					photonAdditive = Color.defaultBlack;
					//float coneConst = PhotonRNG.CONE_FILTER_CONST; //once we get it working without the filter

					for (int el = 0; el <= nearestPhotons.heapSize; el++)
					{
						Photon p = nearestPhotons.objMaxMHeap[el];
						float pDist = (float)nearestPhotons.doubleMazHeap[el];
						if (p != null)
						{
							//float brdfScaler = this.bestObj.lightModel.mcBRDF( p.dir, -outgoing, objNormal ); //probability of this photon being visible from the eye
							//Color tempColor = p.pColor.scale( brdfScaler ); //dividing by brdf did something strange... TODO does this need to be scaled by anything else...
							//Color brdfColor = this.bestObj.lightModel.illuminate( intersection, outgoing, this.lights, this.objects, this.bestObj, true, true );
							Color tempColor = p.pColor;
							photonAdditive += tempColor;        //.scale( pConeWeight );//float pConeWeight = 1f - (float)(pDist / (coneConst * radRoot)); //the cone filter!
						}
					}

					//float coneDivisor = 1f - (2f / (coneConst * 3f));
					float areaScaler = (float) ((1f / Math.PI)/ circleRad) ; //this gets insanely high without the check for 8+ photons...4/24
					photonAdditive = photonAdditive.scale( areaScaler ); //average over area

					//debug
					if (nearestPhotons.heapSize > this.highestK) this.highestK = nearestPhotons.heapSize;
					this.allK += nearestPhotons.heapSize;
				}
			}
			if (photonAdditive.whiteOrHigher()) Console.WriteLine( "Indirect illumination is white or higher" );

			return photonAdditive;
		}

		// Importance sampling using Phong-Bassed BRDF, Russian Roulette.
		// Implemented for Photon Mapping ease.
		public Color spawnRayPM( LightRay ray, int recDepth, bool fromDiffuse = false, int maxBounces = 4 )
		{

			Color currColor = Color.defaultBlack;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Color lightRadiance = null;
			Point intersection = null;
			bool diffuseFlag = fromDiffuse;

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

				IlluminationModel localLightModel = this.bestObj.lightModel;
				SceneObject localBest = this.bestObj;
				if ((t != null) && (t.hasTexCoord())) 
					localBest.diffuse = this.checkerboard.illuminate( t, CheckerBoardPattern.DEFAULT_DIMS, CheckerBoardPattern.DEFAULT_DIMS );

				currColor = localLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, localBest, true ); //gather direct illumination


				//calculate indirect illumination & caustics via PM queries...directly at diffuse surfaces. Results made sense but no color bleeding
				//if(localBest.kRefl == 0 && localBest.kTrans == 0){
				//	Color photonCols = callPhotons( intersection, -ray.direction, localBest.normal );
				//	currColor += photonCols;
				//}

				//attempt 2 using importance sampling via PMs

				// this approach of final gather gives grainy images when coupled with importance sampling. - 4/27
				if( fromDiffuse ){
					Color photonCols = callPhotons( intersection, -ray.direction, localBest.normal );
					return photonCols;
				}

				Vector nHit = localBest.normal;
				bool inside = false;
				float checkDp = nHit.dotProduct( ray.direction );

				if (checkDp > 0)
				{
					nHit = -nHit;
					inside = true;
				}

				//importance sampling/russian roulette here TODO... figure out how to incorporate this into transmittance... did I do this right 4/24...
				float u1 = this.photonMapper.random01();
				float u2 = this.photonMapper.random01();
				float materialScale = 1f;

				Vector travelDir = Vector.ZERO_VEC;
				Point pOrigin = offsetIntersect( intersection, travelDir, localBest.normal );
				PhotonRNG.RR_OUTCOMES rrOutcome = PhotonRNG.RR_OUTCOMES.TRANSMIT; //assume we're transmitting for simplicity.

				if (!inside && localLightModel.kd > 0)
					rrOutcome = this.photonMapper.RussianRoulette( localLightModel.kd, localLightModel.ks, localBest.kRefl, localBest.kTrans );

				//https://henrikdahlberg.github.io/2016/09/12/fresnel-reflection-and-refraction.html
				//https://blog.demofox.org/2017/01/09/raytracing-reflection-refraction-fresnel-total-internal-reflection-and-beers-law/
				switch (rrOutcome)
				{
					case PhotonRNG.RR_OUTCOMES.DIFFUSE:

						if ((localBest.kRefl == 0 && localBest.kTrans == 0)){
							Color colorBleed = Color.defaultBlack;
							int samps = 1;
							for (int sk = 0; sk < samps; sk++) //attempt for color-bleeding via Importance sampling
							{
								travelDir = getRightDiffuse( localLightModel, u1, u2, localBest.normal ); //this is wi... camera ray is outgoing
								LightRay pRay = new LightRay( travelDir, pOrigin );
								float p = localLightModel.diffuseContribution( travelDir, localBest.normal );
								diffuseFlag = true;
								float cosTheta = travelDir.dotProduct( localBest.normal );
								Color mdAdd = spawnRayPM( pRay, recDepth + 1, diffuseFlag );
								colorBleed += mdAdd.scale( cosTheta / p );
							}
							currColor += (colorBleed.scale( 1f / samps ));
						}
						//if we're diffuse then collect photons at this point...
						break;
					case PhotonRNG.RR_OUTCOMES.SPECULAR:
						//travelDir = localBestLightModel.mcSpecDir( u1, u2); //grainy when used in place of mirror, which is expected for 1 sample... 4/24
						diffuseFlag = false;
						break;
					case PhotonRNG.RR_OUTCOMES.TRANSMIT: //handles logic for negating normal and cos term inside method
						break;
					case PhotonRNG.RR_OUTCOMES.ABSORB:
						diffuseFlag = false;
						break;
					default:
						diffuseFlag = false;
						break;
				}

				diffuseFlag = false;
				if (recDepth < maxBounces) //handle reflection and transmission here...
				{
					Color recColor = null;
					//reflection
					if (localBest.kRefl > 0)
					{
						//importance sampling here if on
						//Point reflOrigin;
						Vector reflDir = Vector.reflect2( ray.direction, localBest.normal ); //equivalent way with what I did for Phong. But this is just reflecting back the same ray
						Point reflOrigin = offsetIntersect( intersection, reflDir, localBest.normal );
						LightRay reflRay = new LightRay( reflDir, reflOrigin );
						recColor = spawnRayPM( reflRay, recDepth + 1 );

						if (recColor != null)
							currColor += recColor.scale( localBest.kRefl );
					}

					//refraction - cp6
					//https://phet.colorado.edu/sims/html/bending-light/latest/bending-light_en.html... app
					//https://www.scratchapixel.com/code.php?id=8&origin=/lessons/3d-basic-rendering/ray-tracing-overview... better
					if (localBest.kTrans > 0) //cp6 TODO, handle ray passing through an object!
					{
						//spawn transmission ray
						Vector transDir;
						Point transOrigin;

						if (inside) //these do alternate
							transDir = Vector.transmit( ray.direction, nHit, localBest.refIndex, SceneObject.AIR_REF_INDEX );
						else
							transDir = Vector.transmit( ray.direction, nHit, SceneObject.AIR_REF_INDEX, localBest.refIndex );

						transOrigin = offsetIntersect( intersection, transDir, localBest.normal );
						LightRay translRay = new LightRay( transDir, transOrigin );
						ray.entryPt = intersection; //keep track of if we're in an object or not
						recColor = spawnRayPM( translRay, recDepth + 1 );

						ray.entryPt = null; // we've exited

						if (recColor != null)
							currColor += recColor.scale( localBest.kTrans );
					}
				}
			}
			return currColor;
		}

		//path-tracing test
		// Implemented to test hemisphere sampling
		public Color spawnRayPath( LightRay ray, int recDepth, bool fromDiffuse = false, int maxBounces = 5 )
		{
			Color currColor = Color.defaultBlack;
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Color lightRadiance = null;
			Point intersection = null;
			bool diffuseFlag = fromDiffuse;

			if (this.kdTree.root == null)
				bestW = findRayIntersect( ray );
			else
				bestW = this.kdTree.travelTAB( ray, this );

			if ((this.bestObj != null) && (bestW != float.MaxValue))
			{
				Sphere s = this.bestObj as Sphere;
				Polygon t = this.bestObj as Polygon;
				if (s != null) intersection = s.getRayPoint( ray, bestW );
				else if (t != null) intersection = t.getRayPoint( ray, bestW );

				IlluminationModel localLightModel = this.bestObj.lightModel;
				SceneObject localBest = this.bestObj;

				if ((t != null) && (t.hasTexCoord()))
					localBest.diffuse = this.checkerboard.illuminate( t, CheckerBoardPattern.DEFAULT_DIMS, CheckerBoardPattern.DEFAULT_DIMS );

				currColor = localLightModel.illuminate( intersection, -ray.direction, this.lights, this.objects, localBest, true ); //gather direct illumination

				Vector nHit = localBest.normal;
				bool inside = false;
				float checkDp = nHit.dotProduct( ray.direction );

				if (checkDp < 0)
				{
					nHit = -nHit;
					inside = true;
				}

				//importance sampling/russian roulette here TODO... figure out how to incorporate this into transmittance... did I do this right 4/24...
				float u1 = this.photonMapper.random01();
				float u2 = this.photonMapper.random01();
				float materialScale = 1f;

				Vector travelDir = Vector.ZERO_VEC;
				Point pOrigin = offsetIntersect( intersection, travelDir, localBest.normal );
				PhotonRNG.RR_OUTCOMES rrOutcome = PhotonRNG.RR_OUTCOMES.TRANSMIT; //assume we're transmitting for simplicity.
				float prob = 1f;
				float dpScale = 1f;

				if (!inside)
					rrOutcome = this.photonMapper.RussianRoulette( localLightModel.kd, localLightModel.ks, localBest.kRefl, localBest.kTrans );

				//https://henrikdahlberg.github.io/2016/09/12/fresnel-reflection-and-refraction.html
				//https://blog.demofox.org/2017/01/09/raytracing-reflection-refraction-fresnel-total-internal-reflection-and-beers-law/
				switch (rrOutcome)
				{
					case PhotonRNG.RR_OUTCOMES.DIFFUSE:
						travelDir = getRightDiffuse( localLightModel, u1, u2, localBest.normal ); //this is wi... camera ray is outgoing
						prob = localLightModel.diffuseContribution( travelDir, localBest.normal );
						dpScale = travelDir.dotProduct( localBest.normal );
						//if we're diffuse then collect photons at this point...
						break;
					case PhotonRNG.RR_OUTCOMES.SPECULAR:
						travelDir = localLightModel.mcSpecDir( u1, u2); //grainy when used in place of mirror, which is expected for 1 sample... 4/24
						prob = localLightModel.specContribution( travelDir, ray.direction, localBest.normal );
						dpScale = travelDir.dotProduct( localBest.normal );
						break;
					case PhotonRNG.RR_OUTCOMES.TRANSMIT: //handles logic for negating normal and cos term inside method
						travelDir = Vector.transmit2( ray.direction, localBest.normal, SceneObject.AIR_REF_INDEX, localBest.refIndex );
						break;
					case PhotonRNG.RR_OUTCOMES.ABSORB:
						break;
					default:
						break;
				}

				if (!travelDir.isZeroVector() && recDepth <= maxBounces)
				{ //we diffusely or specularly reflect
					LightRay pRay = new LightRay( travelDir, pOrigin );
					Color indirect = spawnRayPath( pRay, recDepth + 1, diffuseFlag );
					if (prob <= 0)
						Console.WriteLine( "Negative proability in path trace?" );
					currColor += indirect.scale( dpScale/ prob);
				}
			}
			return currColor;
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

//Old code archive:

Indirect via PM attempts:
//if (rrOutcome == PhotonRNG.RR_OUTCOMES.DIFFUSE && recDepth < maxBounces && localBest.kRefl == 0 && localBest.kTrans == 0)
//{ //we diffusely or specularly reflect
//	LightRay pRay = new LightRay( travelDir, pOrigin );
//	float prob = localLightModel.mcBRDF( travelDir, -ray.direction, localBest.normal );
//	currColor += spawnRayPM( pRay, recDepth + 1, diffuseFlag );
//}
 */