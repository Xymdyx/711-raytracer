using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Kd_tree
{
	public abstract class Voxel
	{
		private int _shape;

		public int shape { get => this._shape; set => this._shape = value; }

		public float intersect()
		{
			return 0.0f;
		}

	}
}
