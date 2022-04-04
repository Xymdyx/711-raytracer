/*
desc: class that does all the RNG in Photon mapping
date started: 4/4
date due: 4/29
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Photon_Mapping
{
	public class PhotonRNG
	{
		private Random rand;

		public PhotonRNG( int seed = 1 )
		{
			this.rand = new Random( seed );
		}

		public float random01() { return (float) rand.NextDouble(); }

		public float RussianRoulette()
		{
			return 0.0f;
		}
	}
}
