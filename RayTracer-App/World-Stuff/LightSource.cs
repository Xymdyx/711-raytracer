/*
author : Sam Ford
desc: class that reps light source in world
date started: 2/19/2021
 */
using System;
using System.Numerics; //for Matrix4x4 float
using System.Collections.Generic;
using RayTracer_App.Photon_Mapping;
using SceneObj = RayTracer_App.Scene_Objects.SceneObject;
using Sphere = RayTracer_App.Scene_Objects.Sphere;
using Poly = RayTracer_App.Scene_Objects.Polygon;


namespace RayTracer_App.World
{
	public class LightSource 
	{
		private Point _position;
		private Color _lightColor;
		private float _power; // for photon mapping
		private int _ne;//number of emitted photons for this lightsource
		private int _defPhots;

		public Point position { get => this._position; set => this._position = value; }
		public Color lightColor { get => this._lightColor; set => this._lightColor = value; }
		public float power { get => this._power; set => this._power = value; }
		public int ne { get => this._ne; }
		public int defPhots { get => this._defPhots; }


		public LightSource ()
		{
			this._position = new Point();
			this._lightColor = new Color();
			this._power = 0;
			this._defPhots = 0;
		}

		public LightSource( Point position, Color lightColor, float power = 100f, int defPhots = 10000) //50 pow for debugging
		{
			this._position = position;
			this._lightColor = lightColor;
			this._power = power;
			this._defPhots = defPhots;
		}

		//transform light with the camera..
		public void transform( Matrix4x4 camViewMat )
		{
			Vector4 posHmg = position.toHmgCoords(); // 1x4 Vector
			Vector4 newVertVec = Vector4.Transform( posHmg, camViewMat ); // we postMultiply since we are is LHS w Row-major.. Vnew = Vold * A * B
			position.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form

			return;
		}

		// for square light -- https://www.cs.princeton.edu/courses/archive/fall16/cos526/lectures/03-photonmapping.pdf
		//emit photons from diffuse point light source... 
		public void emitGlobalPhotonsFromDPLS( World world)
		{
			float x;
			float y;
			float z;
			int ne = 0;
			int totalPhotons = defPhots;
			Point photonPos;
			world.photonMapper.maxGlobal = totalPhotons;
			while (world.photonMapper.globalPL.Count < totalPhotons) //while we don't have the totalPhotons
			{
				do
				{
					x = world.photonMapper.randomRange();
					y = world.photonMapper.randomRange();
					z = world.photonMapper.randomRange();
				} while( (x * x) + (y * y) + (z * z) > 1 ) ;

				Vector dir = new Vector( x, y, z );
				LightRay photonRay = new LightRay( dir, this.position );
				world.tracePhoton( photonRay, 1 );
				ne++;
			}

			this._ne += ne;
			float photonPow = (this.power / ne); //according to Jensen, we only scale by EMITTED PHOTONS, not by total
			world.photonMapper.scaleStored( photonPow, PhotonRNG.MAP_TYPE.GLOBAL );
			Console.WriteLine( "Finished global" );

			return;
		}

		// for square light -- https://www.cs.princeton.edu/courses/archive/fall16/cos526/lectures/03-photonmapping.pdf
		//emit photons from diffuse point light source and aim at targets we know will make caustics
		public void emitCausticsFromDPLS( World world, List<SceneObj> targets)
		{
			float x;
			float y;
			float z;
			int targetCount = targets.Count;
			int basePhotons = defPhots;
			int totalPhotons =  targetCount * basePhotons;
			int ne = 0;
			Point photonPos;
			PhotonRNG pMapper = world.photonMapper;
			pMapper.maxCaustics = totalPhotons;

			for (int item = 0; item < targetCount; item++)
			{
				while (pMapper.causticPL.Count < basePhotons * (item + 1) )
				{
					Point randPt = targets[item].randomPointOn( pMapper ); //this should get a randomPoint on the appropriate target
					Vector dir = randPt - this.position;
					LightRay photonRay = new LightRay( dir, this.position );
					int countBefore = pMapper.causticPL.Count;
					world.tracePhotonCaustic( photonRay, 1 );
					if (countBefore == pMapper.causticPL.Count)
						continue;

					ne++; //bad acuuracy for sphere...
				}
			}

			int stored = pMapper.causticPL.Count;

			if (stored != totalPhotons)
				Console.WriteLine( $"Didn't shoot enough photons... Only stored {stored}, wanted {totalPhotons}" );

			this._ne += ne;
			float photonPow = (this.power / ne); //according to Jensen, we only scale by EMITTED PHOTONS, not by total
			pMapper.scaleStored( photonPow, PhotonRNG.MAP_TYPE.CAUSTIC );
			Console.WriteLine( "Finished caustics" );

			return;
		}

	}
}
