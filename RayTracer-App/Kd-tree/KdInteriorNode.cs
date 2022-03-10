using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Kd_tree
{
	public class KdInteriorNode : KdNode
	{
		//fields
		private int _axis;
		private float _axisVal;
		private KdNode _front;
		private KdNode _rear;

		//properties
		public int axis { get => this._axis; set => this._axis = value; }
		public float axisVal { get => this._axisVal; set => this._axisVal = value; }
		public KdNode front { get => this._front; set => this._front = value; }
		public KdNode rear { get => this._rear; set => this._rear = value; }

		//constructors
		public KdInteriorNode()
		{
			this._axis = 0;
			this._axisVal = 0.0f;
			this._front = null;
			this._rear = null;
		}

		public KdInteriorNode( int axis, float axisVal, KdNode front, KdNode rear )
		{
			this._axis = axis;
			this._axisVal = axisVal;
			this._front = front;
			this._rear = rear;
		}
	}
}
