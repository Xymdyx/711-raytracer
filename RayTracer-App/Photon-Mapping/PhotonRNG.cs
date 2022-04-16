/*
desc: class that does all the RNG in Photon mapping
date started: 4/4
date due: 4/29
 */
using System;
using System.Collections.Generic;
using RayTracer_App.Kd_tree;
using RayTracer_App.World;

//if photons get manipulated in anyway during Pass 2, need to return copies...
namespace RayTracer_App.Photon_Mapping
{
	public class PhotonRNG
	{
		//russian roulette enum for more readable code
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
		private ptKdTree _globalPM; // LS+D
		private ptKdTree _causticPM; // L{S|D|V }∗D
		private ptKdTree _volumePM; //L{S|D|V }+V

		/*
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
		public void addGlobal( Point intersection, float dx, float dy, float power = 0 )
		{
			Photon p = new Photon( intersection, power, dx, dy );
			this.globalPL.Add( p );
		}

		//makes a new photon and adds it to the global photon list...
		public void addCaustic( Point intersection, float dx, float dy, float power = 0 )
		{
			Photon p = new Photon( intersection, power, dx, dy );
			this.causticPL.Add( p );
		}

		//after photon emission and tracing is complete, scale all stored by 1/phontonNum
		//currently some of these get manipulated twice
		public void scaleStored( float powerScale )
		{
			foreach (Photon g in this.globalPL)
				g.power *= powerScale;
			foreach (Photon c in this.causticPL)
				c.power *= powerScale;
		}

		//MAKE THE PMS
		public void makeGlobalPM()
		{
			List<Point> globalPoses = grabPosByType( MAP_TYPE.GLOBAL );
			this.globalPM.root = globalPM.balance( globalPoses, 0, this );
			return;
		}

		public void makeCausticPM()
		{
			List<Point> causticPoses = grabPosByType( MAP_TYPE.CAUSTIC );
			this.causticPM.root = causticPM.balance( causticPoses, 0, this );
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
				if (pos == p.pos)
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
			float bestW = desired.travelTAB( ray, world );

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
		public RR_OUTCOMES RussianRoulette( float diffuse, float spec, float refl, float trans )
		{
			float chance = random01();

			if (0 <= chance && chance <= diffuse)
				return RR_OUTCOMES.DIFFUSE;
			else if (diffuse < chance && chance <= spec + diffuse)
			{
				//do we reflect or transmit?
				chance = random01();
				if ((0 <= chance && chance <= trans))
					return RR_OUTCOMES.TRANSMIT;
				else
					return RR_OUTCOMES.SPECULAR;
			}
			else if (spec + diffuse < chance && chance <= 1.0f)
				return RR_OUTCOMES.ABSORB;
			else
				return RR_OUTCOMES.ABSORB;
		}

	}
}
