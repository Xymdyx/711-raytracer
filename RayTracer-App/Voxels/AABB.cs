/*
 * author: stf8464
 * desc: class that represents an axially-aligned bounding box to contain an object
 * date : 3/10/22
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Voxels
{
	public class AABB : Voxel
	{
		public enum Axes { X, Y, Z }

		private int _axis;
		public int axis { get => this._axis; set => this._axis = value; }

		public AABB()
		{
			this._shape = 0; //for box
			this._max = null;
			this._min = null;
			this.axis = (int) Axes.X;
		}

		public AABB( Point max, Point min, int axis )
		{
			this._shape = 0;
			this._max = max;
			this._min = min;
			this.axis = axis;
		}

		
		// intersects with itself
		public override bool intersect()
		{
		/*
			for each axis x, y, z
		if (Aaxis.min > Baxis.max) or
		(Baxis.min > Aaxis.max)
			return FALSE
		return TRUE;
		*/
			return true;
		}

		public bool sphereIntersect( Point center, float radius )
		{
		/*
			d = 0
		for each axis x, y, z
			if ((e = caxis - Aaxis.min) < 0) if (e < -r) return FALSE
			d = d + e2
			else if ((e = caxis - Aaxis.max) > 0) 
				if (e > r) return FALSE
			d = d + e2

		if ( d > r2 ) return FALSE
		return TRUE;
		*/
			return true;
		}

		// 13 axis triangle tests
		public bool triangleIntersect( Scene_Objects.Polygon triangle ) 
		{
			// https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/aabb-triangle.html
			return true;
		}

	}
}
