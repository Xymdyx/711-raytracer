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
		public enum RR_OUTCOMES
		{
			ERROR = -1,
			DIFFUSE = 0,
			SPECULAR = 1,
			ABSORB = 2
		}

		private Random rand;

		public PhotonRNG( int seed = 1 )
		{
			this.rand = new Random( seed );
		}

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
