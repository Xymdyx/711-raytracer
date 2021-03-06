/*
desc: class that does all the RNG in Photon mapping
date started: 4/4
date due: 4/29
 */
using System;
using System.Collections.Generic;
using RayTracer_App.Kd_tree;
using RayTracer_App.Voxels;
using RayTracer_App.World;

//if photons get manipulated in anyway during Pass 2, need to return copies...
namespace RayTracer_App.Photon_Mapping
{
	public class PhotonRNG
	{
		public const int MAX_SHOOT_DEPTH = 100; //30 .15
		public const int K_PHOTONS = 250; //15... able to gather 449 on 1000? usually 50 -500 used in estimate
		public const float DEF_SEARCH_RAD = .15f; //.1f for w direct illum
		public const float CONE_FILTER_CONST = 1f; //for cone filter... >=1

		//RR debug
		private int absorbed;
		private int reflected;
		private int transmitted;
		private int diffused;
		private int _maxGlobal;
		private int _maxCaustic;
		private int _causticHits;
		public int powerless = 0;

		public int maxGlobal { get => this._maxGlobal; set => this._maxGlobal = value; }
		public int maxCaustics { get => this._maxCaustic; set => this._maxCaustic = value; }
		public int caustics { get => this._causticHits; set => this._causticHits = value; }

		public enum RR_OUTCOMES
		{
			ERROR = -1,
			DIFFUSE = 0,
			SPECULAR = 1,
			TRANSMIT = 2,
			ABSORB = 3
		}

		// map type enum to facilitate accessing proper photon maps
		public enum MAP_TYPE
		{
			GLOBAL = 0,
			CAUSTIC = 1,
			VOLUME = 2,
			NONE = 3
		}

		//RNG
		private Random rand; //Random rand = new Random(Guid.NewGuid().GetHashCode()); really random seed

		//PHOTON ARRAYS TO BE MADE INTO KDTREES/ PMs
		private List<Photon> _globalPL;
		private List<Photon> _causticPL;
		private List<Photon> _volumePL;

		//include photn maps here
		private ptKdTree _globalPM; // L{S|D|V }∗D
		private ptKdTree _causticPM; //  LS+D
		private ptKdTree _volumePM; //L{S|D|V }+V

		/* Light Transport Notation:
		 * S is specular reflection or transmission, 
		 * D is diffuse	(ie.non-specular) reflection or transmission,
		 * and V is volume scattering.
		 * * = n times, + = followed by, | = or... L = emission
		*/

		//Properties
		public List<Photon> globalPL { get => this._globalPL; set => this._globalPL = value; }
		public List<Photon> causticPL { get => this._causticPL; set => this._causticPL = value; }
		public List<Photon> volumePL { get => this._volumePL; set => this._volumePL = value; }
		public ptKdTree globalPM { get => this._globalPM; set => this._globalPM = value; }
		public ptKdTree causticPM { get => this._causticPM; set => this._causticPM = value; }
		public ptKdTree volumePM { get => this._volumePM; set => this._volumePM = value; }

		public PhotonRNG( int seed = 1 )
		{
			this.rand = new Random( seed );

			this.globalPL = new List<Photon>();
			this.causticPL = new List<Photon>();
			this.volumePL = null; //over summer.

			// (Spec|Diffuse|Volume) n times, Diffuse
			this.globalPM = new ptKdTree();
			//Spec n times and then Diffuse
			this.causticPM = new ptKdTree();

			this.volumePM = null; //over summer.
		}

//STATIC METHODS
		//grab a photon with a matching position from a given photon list
		public static Photon findPhotonByPos( Point pos, List<Photon> targetPhotons )
		{
			foreach (Photon p in targetPhotons)
			{
				if (pos == p.pos)
					return p;
			}

			return null;
		}

// getter for each photon list depending on desired type
		public List<Photon> getPLbyType( MAP_TYPE listType = MAP_TYPE.GLOBAL )
		{
			switch (listType)
			{
				case MAP_TYPE.GLOBAL:
					return this.globalPL;
				case MAP_TYPE.CAUSTIC:
					return this.causticPL;
				case MAP_TYPE.VOLUME:
					return this.volumePL;
				default:
					return null;
			}
		}

		// getter for each photon map depending on desired type
		public ptKdTree getPMbyType( MAP_TYPE mapType = MAP_TYPE.GLOBAL )
		{
			switch (mapType)
			{
				case MAP_TYPE.GLOBAL:
					return this.globalPM;
				case MAP_TYPE.CAUSTIC:
					return this.causticPM;
				case MAP_TYPE.VOLUME:
					return this.volumePM;
				default:
					return null;
			}
		}

		// getter for colors for each photon map
		public Color getPColorbyType( MAP_TYPE listType = MAP_TYPE.GLOBAL )
		{
			switch (listType)
			{
				case MAP_TYPE.GLOBAL:
					return Color.photonColor;
				case MAP_TYPE.CAUSTIC:
					return Color.causticColor;
				case MAP_TYPE.VOLUME:
					return Color.volumetricColor;
				default:
					return Color.defaultBlack;
			}
		}

		// getter for colors for each photon map
		public String getPStringbyType( MAP_TYPE listType = MAP_TYPE.GLOBAL )
		{
			switch (listType)
			{
				case MAP_TYPE.GLOBAL:
					return "globaL";
				case MAP_TYPE.CAUSTIC:
					return "caustic";
				case MAP_TYPE.VOLUME:
					return "volume";
				default:
					return "nonexistent";
			}
		}

		// helper for getting photon positions of a given type
		public List<Point> grabPosByType( MAP_TYPE desired = MAP_TYPE.GLOBAL )
		{
			List<Point> photoPoses = new List<Point>();
			List<Photon> targetList = getPLbyType( desired );

			foreach (Photon p in targetList)
				photoPoses.Add( p.pos );

			return photoPoses;
		}

		//MANAGING PLs
		//makes a new photon and adds it to the global photon list...
		public void addGlobal( Point intersection, float dx, float dy, Vector dir, Color objColor = null, float power = 0 )
		{
			if (globalPL.Count >= maxGlobal)
				return;

			Photon p = new Photon( intersection, power, dx, dy , dir, objColor);
			this.globalPL.Add( p );
		}

		//makes a new photon and adds it to the global photon list...
		public void addCaustic( Point intersection, float dx, float dy, Vector dir, Color objColor, float power = 0 )
		{
			if (causticPL.Count >= maxCaustics)
				return;
			Photon p = new Photon( intersection, power, dx, dy, dir, objColor );
			this.causticPL.Add( p );
		}

		//after photon emission and tracing is complete, scale all stored by 1/emiited.... 4/24
		//currently some of these get manipulated twice.. Assumes you want to scale all photons emitted
		public void scaleStored( float powerScale, MAP_TYPE only = MAP_TYPE.NONE )
		{
			//scale all photons
			if (only == MAP_TYPE.NONE)
			{
				foreach (Photon g in this.globalPL)
				{
					g.power *= powerScale;
					g.pColor = g.pColor.scale( g.power );
				}
				foreach (Photon c in this.causticPL)
				{
					c.power *= powerScale;
					c.pColor = c.pColor.scale( c.power );
				}
				foreach (Photon v in this.volumePL)
				{
					v.power *= powerScale;
					v.pColor = v.pColor.scale( v.power );
				}
			}
			else
			{
				List<Photon> chosen = this.getPLbyType( only );
				foreach (Photon p in chosen)
				{
					p.power *= powerScale;
					p.pColor = p.pColor.scale( p.power );
				}
			}

			return;
		}

		//MAKE THE PMS... my way vs. Jensen's... 4/27
		public void makeGlobalPM()
		{
			//List<Point> globalPoses = grabPosByType( MAP_TYPE.GLOBAL );
			//List<Point> tempPoses = globalPoses.ConvertAll( pos => pos.copy() );
			//this.globalPM.root = globalPM.balance( tempPoses, 0, this, float.MaxValue, MAP_TYPE.GLOBAL );

			if (globalPL.Count > 0)
			{
				List<Photon> globalCopy = globalPL.ConvertAll( phot => phot.copy() );
				globalPM.balanceJensen( globalCopy );
				foreach (Photon p in globalPL)
				{
					if (!globalPM.jensenHeap.Contains( p )) //sanity check
						Console.WriteLine( $"Missing {p} in Global Jensen Heap" );
				}
			}
			return;
		}

		public void makeCausticPM()
		{
			//List<Point> causticPoses = grabPosByType( MAP_TYPE.CAUSTIC );
			//List<Point> tempPoses = causticPoses.ConvertAll( pos => pos.copy() );
			//if ( causticPoses.Count > 0)
			//	this.causticPM.root = causticPM.balance( tempPoses, 0, this, float.MaxValue, MAP_TYPE.CAUSTIC );

			if (causticPL.Count > 0)
			{
				List<Photon> causticCopy = causticPL.ConvertAll( phot => phot.copy() );
				causticPM.balanceJensen( causticCopy );
				foreach (Photon p in causticPL)
				{
					if (!causticPM.jensenHeap.Contains( p )) //sanity check
						Console.WriteLine( $"Missing {p} in Caustic Jensen Heap" );
				}
			}

			return;
		}

		public void makeVolumePM()
		{
			List<Point> volumePoses = grabPosByType( MAP_TYPE.VOLUME );
			this.volumePM.root = volumePM.balance( volumePoses, 0, this );
			return;
		}

		// make all pms
		public void makePMs()
		{
			makeGlobalPM();
			makeCausticPM();
			//makeVolumePM();
			return;
		}

		//grab a photon from a particular photon list with a matching position that isn't in the kdTree yet
		public Photon grabPhotonByPos( Point pos, MAP_TYPE targetList = MAP_TYPE.GLOBAL )
		{
			List<Photon> targetPhotons = getPLbyType( targetList );

			foreach (Photon p in targetPhotons)
			{
				if (pos == p.pos && !p.litFlag )
					return p;
			}

			return null;
		}


//intersection testing for Photon Visualizing.
		// Go through all photons in the map and return closest w
		public float intersectListFull( LightRay ray, MAP_TYPE mapType = MAP_TYPE.GLOBAL )
		{
			List<Photon> desired = getPLbyType( mapType );
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Photon closest;
			foreach (Photon p in desired)
			{
				if (!p.litFlag)
				{
					currW = p.rayPhotonIntersect( ray );
					if ((currW != float.MinValue) && (currW != float.NaN) &&
						(currW != float.MaxValue) && (currW < bestW) && (currW > 0))
					{
						bestW = currW;
						closest = p;
					}
				}
			}

			return bestW; //Color.photonColor
		}

		// quick version of the above
		// intersection testing for Photon Visualizing.
		// we hit a photon, light it up no matter what
		public bool intersectListQuick( LightRay ray, MAP_TYPE mapType = MAP_TYPE.GLOBAL )
		{
			List<Photon> desired = getPLbyType( mapType );
			float bestW = float.MaxValue;
			float currW = float.MaxValue;
			Photon closest;
			foreach (Photon p in desired)
			{
				if (!p.litFlag)
				{
					if (p.rayPhotonIntersectQuick( ray ))
					{
						p.litFlag = true;
						return true;
					}
				}
			}

			return false; //Color.photonColor
		}

		//intersection testing for Photon Visualizing.
		// Go through all photons in the map and return closest w
		public float intersectPMFull( LightRay ray, World.World world, MAP_TYPE mapType = MAP_TYPE.GLOBAL )
		{
			ptKdTree desired = getPMbyType( mapType );
			float bestW = desired.travelTAB( ray, world);
//			float bestW = desired.travelTAB( ray, world, 0 );

			return bestW; //Color.photonColor
		}

		//MONTE CARLO STUFF
		// for grabbing a random number between [min, max)
		public float randomRange( float min = -1, float max = 1 )
		{
			float diff = max - min;
			float ranPercent = random01();

			return (float)(diff * ranPercent) + min;
		}

		// for grabbing a random number between [0, 1)
		public float random01() { return (float)rand.NextDouble(); }

		//Monte Carlo for determining if a Photon gets absorbed, reflected diffusely, or reflected specularly
		//https://github.com/ningfengh/SC_Tracer/blob/master/source/photon_map.cpp
		// http://www.cs.cmu.edu/afs/cs.cmu.edu/academic/class/15864-s04/www/assignment4/pm.pdf
		// Schlick's approximation? https://en.wikipedia.org/wiki/Schlick%27s_approximation
		//TODO I think this needs work.
		public RR_OUTCOMES RussianRoulette( float diffuse, float spec, float refl, float trans )
		{
			float chance = random01();

			if (0 <= chance && chance <= diffuse)
			{
				diffused++;
				return RR_OUTCOMES.DIFFUSE;
			}
			else if (diffuse < chance && chance <= spec + diffuse)
			{
				//do we reflect or transmit? ... TODO... is this a valid way to do it?
				chance = random01();
				if ((0 <= chance && chance <= trans))
				{
					transmitted++;
					return RR_OUTCOMES.TRANSMIT;
				}
				else
				{
					reflected++;
					return RR_OUTCOMES.SPECULAR;
				}
			}
			else if (spec + diffuse < chance && chance <= 1.0f)
			{
				absorbed++;
				return RR_OUTCOMES.ABSORB;
			}
			else
			{
				absorbed++;
				return RR_OUTCOMES.ABSORB;
			}
		}

		//gather k-Nearest photons around a ray's given intersection point
		// use a priority queue here. MHigher val = lower priority
		// we check within a certain radius for photons
		// the longest one we use to make a sphere
		// we gather nearby ones, add to max heap
		// do this for all nearest photons, replace closer ones with farther ones
		// return the list of k photons for calculations
		public unsafe MaxHeap<Photon> kNearestPhotons( Point pos, int k, float rad = PhotonRNG.DEF_SEARCH_RAD, MAP_TYPE desired = MAP_TYPE.GLOBAL, bool debug = false )
		{

			float* radPtr = &rad;
			MaxHeap<Photon> nearestHeap = new MaxHeap<Photon>(k);
			ptKdTree queryMap = this.getPMbyType( desired );

			if (queryMap != null)
			{
				//queryMap.locatePhotons( 1, k, pos, radPtr, nearestHeap );
				queryMap.locatePhotonsJensen( 1, k, pos, radPtr, nearestHeap );
				if (nearestHeap.heapFull() && debug)
					Console.WriteLine( $"Actually found {nearestHeap.heapSize} photons near pt {pos}" );
			}
			return nearestHeap; //this way we have the photons nd their distances for use
		}

		//counts all photons
		public int getTotalPhots()
		{
			int all = 0;
			if (globalPL != null) all += globalPL.Count;
			if (causticPL != null) all += causticPL.Count;
			if (volumePL != null) all += volumePL.Count;
			return all;
		}

		//debug RR stats
		public void rrStats()
		{
			Console.WriteLine( $"{this.diffused} diffused" );
			Console.WriteLine( $"{this.reflected} reflected" );
			Console.WriteLine( $"{this.transmitted} transmitted" );
			Console.WriteLine( $"{this.absorbed} absorbed" );
			Console.WriteLine( $" {this.powerless} powerless / {this.getTotalPhots()}" );
		}

		//debug how many photons are in scene bounds
		// Go through all photons in the LIST and see how many are in the bounds
		public void printPhotonsInScene( AABB sceneBox, MAP_TYPE mapType = MAP_TYPE.GLOBAL )
		{
			List<Photon> desired = getPLbyType( mapType );
			String mapName = getPStringbyType( mapType );
			int inScene = 0;

			foreach (Photon p in desired)
			{
				Point pPos = p.pos;
				Point sMax = sceneBox.max;
				Point sMin = sceneBox.min;

				if ( (pPos.x <= sMax.x && pPos.x >= sMin.x)
				&& (pPos.y <= sMax.y && pPos.y >= sMin.y) 
				&& (pPos.z <= sMax.z && pPos.z >= sMin.z) )
					{
						inScene++;
					}
			}

			Console.WriteLine( $" {inScene}/{desired.Count} {mapName} photons in scene bounds" );

			if (mapType == MAP_TYPE.CAUSTIC)
				Console.WriteLine( $"{this._causticHits} caustics hit aimed at targets" );

			return;
			}

		//show the power, gather, and search radius stats
		public String photonSearchStats()
		{
			int allPhotons = globalPL.Count + causticPL.Count;
			return $"Shot {allPhotons} with { globalPL[0].power} watts. Looked for {K_PHOTONS} photns. Search rad: {DEF_SEARCH_RAD}";
		}
		}
}

