using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Kd_tree
{
	//STACK ELEMENT CLASS PER TAB ALGO
	public class KdStackEl
	{
		public static int MAX_STACK_SIZE = 50;

		private KdNode _kdNode; //ptr to far child
		private float _t; //entry/exit signed distance
		private Point _pb; //coords of entry/exit
		private int _prev;
		//public KdNode _prev; //ptr to previous stack item

		public KdNode kdNode { get => this._kdNode; set => this._kdNode = value; }
		public float t { get => this._t; set => this._t = value; }
		public Point pb { get => this._pb; set => this._pb = value; }
		public int prev { get => this._prev; set => this._prev = value; }
		//public KdStackEl preve { get => this._prev; set => this._prev = value; }
		//public KdNode preve { get => this._prev; set => this._prev = value; }


		public KdStackEl( KdNode kdNode, float t, Point pb, int prev = 0 )
		{
			this._kdNode = kdNode;
			this._t = t;
			this._pb = pb;
			this._prev = 0;
			//this._prev = prev;
		}

		public KdStackEl()
		{
			this._kdNode = null;
			this._t = float.MaxValue;
			this._pb = new Point( float.MaxValue, float.MaxValue, float.MaxValue);
			this._prev = 0;
			//this._prev = null;
		}

	}
}
