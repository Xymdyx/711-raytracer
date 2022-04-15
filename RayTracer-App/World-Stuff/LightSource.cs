﻿/*
author : Sam Ford
desc: class that reps light source in world
date started: 2/19/2021
 */
using System;
using System.Numerics; //for Matrix4x4 float
using RayTracer_App.Photon_Mapping;

namespace RayTracer_App.World
{
	public class LightSource 
	{
		private Point _position;
		private Color _lightColor;
		private float _power; // for photon mapping

		public Point position { get => this._position; set => this._position = value; }
		public Color lightColor { get => this._lightColor; set => this._lightColor = value; }
		public float power { get => this._power; set => this._power = value; }

		public LightSource ()
		{
			this._position = new Point();

			this._lightColor = new Color();

			this._power = 0;
		}

		public LightSource( Point position, Color lightColor, float power = 100)
		{
			this._position = position;
			this._lightColor = lightColor;
			this._power = power; 
		}

		
		//transform light with the camera..
		public void transform( Matrix4x4 camViewMat )
		{
			// MATRIX MULTI WORKS DEFINITELY
			Vector4 posHmg = position.toHmgCoords(); // 1x4 Vector
			Vector4 newVertVec = Vector4.Transform( posHmg, camViewMat ); // we postMultiply since we are is LHS w Row-major.. Vnew = Vold * A * B
			position.fromHmgCoords( newVertVec ); // [x y z w] => (x/w, y/w, z/w) CP form
			return;
		}

		// for square light -- https://www.cs.princeton.edu/courses/archive/fall16/cos526/lectures/03-photonmapping.pdf

		//emit photons from diffuse point light source
		public void emitPhotonsFromDPLS( World world, int totalPhotons = 1000 )
		{
			float x;
			float y;
			float z;
			float photonW;
			int ne = 0;
			Point photonPos;
			float photonPow = this.power * (1 / totalPhotons);
			while (ne != totalPhotons)
			{
				do
				{
					x = world.photonMapper.randomRange();
					y = world.photonMapper.randomRange();
					z = world.photonMapper.randomRange();
				} while ((x * x) + (y * y) + (z * z) > 1);

				Vector dir = new Vector( x, y, z );
				LightRay photonRay = new LightRay( dir, this.position );
				//do photon-tracing here...
				world.tracePhoton( photonRay, 1 );
				ne++;
			}
			//scale stored phtons by 1/ne
			world.photonMapper.scaleStored( power );
		}
	}
}
