﻿/*
desc: class that represents a Photon
date started: 4/4
date due: 4/29
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Photon_Mapping
{
	public class Photon
	{
		private Point _pos; //posiiton
		private float _power; //the power
		private float _theta, _phi; //incident direction as spherical coords 
		private float _kdFlag;
		private bool _litFlag; //for photon visualization

		public Point pos { get => this._pos; set => this._pos = value; }
		public float power { get => this._power; set => this._power = value; }
		public float theta { get => this._theta; set => this._theta = value; }
		public float phi { get => this._phi; set => this._phi = value; }
		public float kdFlag { get => this._kdFlag; set => this._kdFlag = value; }
		public bool litFlag { get => this._litFlag; set => this._litFlag = value; }


		// dy and dx are Cartesian points... unit vectors' x and y of incident direction.
		/*
		 * "The balancing
		algorithm is performed after all the photons have been emitted.The photons
		are stored in a linked list of large arrays (each array having 65536 photons).
		-- Jensen96
		
		"The incident direction is a mapping of the spherical coordinates of the photon direction
		to 65536 possible directions - Jensen09"
		http://graphics.ucsd.edu/~henrik/papers/rendering_caustics/rendering_caustics_gi96.pdf */
		//https://en.wikipedia.org/wiki/Spherical_coordinate_system#In_astronomy ...spherical coords
		//https://en.wikipedia.org/wiki/Atan2
		public Photon( Point pos, float power, float dx, float dy )
		{
			this._pos = pos;
			this._power = power;
			this._phi = (float) (255 * (  Math.Atan2( dy, dx ) + Math.PI) / (2 * Math.PI)); //from Cartesian -> Spherical
			this._theta = (float) (255 * Math.Acos( dx ) / Math.PI) ;
			this._kdFlag = float.MaxValue; // this is for the splitting plane axis in the kd-tree),
			this.litFlag = false;
		}

		//ray intersect formula is simply if the photon lies on a ray's path
		public float rayPhotonIntersect( LightRay ray )
		{
			return this.pos.ptRayIntersect( ray );
		}

		//ray intersect formula bool version
		public bool rayPhotonIntersectQuick( LightRay ray )
		{
			return this.pos.ptRayIntersectQuick( ray );
		}
	}
}
