/*
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
		private float _theta, _phi; //incident direction
		private bool _kdFlag;

		public Point pos { get => this._pos; set => this._pos = value; }
		public float power { get => this._power; set => this._power = value; }
		public float theta { get => this._theta; set => this._theta = value; }
		public float phi { get => this._phi; set => this._phi = value; }
		public bool kdFlag { get => this._kdFlag; set => this._kdFlag = value; }

		// dy and dx are spherical coords
		public Photon( Point pos, float power, float dx, float dy )
		{
			this._pos = pos;
			this._power = power;
			this._phi = (float) (255 * (  Math.Atan2( dy, dx ) + Math.PI) / (2 * Math.PI));
			this._theta = (float) (255 * Math.Acos( dx ) / Math.PI) ;
			this._kdFlag = false;
		}
	}
}
