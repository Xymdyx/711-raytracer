﻿/*
desc: class that does all the RNG in Photon mapping
date started: 4/4
date due: 4/29
 */
using System;
using System.Collections.Generic;
using RayTracer_App.Kd_tree;

namespace RayTracer_App.Photon_Mapping
{
	public class PhotonRNG
	{
		public enum RR_OUTCOMES
		{
			ERROR = -1,
			DIFFUSE = 0,
			SPECULAR = 1,
			ABSORB = 2
		}

		//RNG
		private Random rand;


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
			this.volumePM= null; //over summer.
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
		public void scaleStored( float powerScale )
		{
			foreach( Photon g in this.globalPL)
				g.power *= powerScale;
			foreach (Photon c in this.causticPL)
				c.power *= powerScale;
		}


		//MONTE CARLO STUFF
		// for grabbing a random number between [min, max)
		public float randomRange( float min = -1, float max = 1)
		{
			float diff = max - min;
			float ranPercent = random01();

			return (float)(diff * ranPercent) + min;
		}

		// for grabbing a random number between [0, 1)
		public float random01() { return (float) rand.NextDouble(); }

		//Monte Carlo for determining if a Photon gets absorbed, reflected diffusely, or reflected specularly
		public RR_OUTCOMES RussianRoulette( float diffuse, float spec )
		{
			float chance = random01();

			if (0 <= chance && chance <= diffuse)
				return RR_OUTCOMES.DIFFUSE;
			else if (diffuse < chance && chance <= spec + diffuse)
				return RR_OUTCOMES.SPECULAR;
			else if (spec + diffuse < chance && chance <= 1.0f)
				return RR_OUTCOMES.ABSORB;
			else
				return RR_OUTCOMES.ABSORB;
		}
	}
}
