using System;
using RayTracer_App.Voxels;
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
		private AABB _selfAABB;

		//properties
		public int axis { get => this._axis; set => this._axis = value; }
		public float axisVal { get => this._axisVal; set => this._axisVal = value; }
		public KdNode front { get => this._front; set => this._front = value; }
		public KdNode rear { get => this._rear; set => this._rear = value; }
		public AABB selfAABB { get => this._selfAABB; set => this._selfAABB = value; }

		//constructors
		public KdInteriorNode()
		{
			this._axis = 0;
			this._axisVal = 0.0f;
			this._selfAABB = null;
			this._front = null;
			this._rear = null;
		}

		public KdInteriorNode( int axis, float axisVal, AABB selfAABB, KdNode front, KdNode rear )
		{
			this._axis = axis;
			this._axisVal = axisVal;
			this._selfAABB = selfAABB;
			this._front = front;
			this._rear = rear;
		}

		public override string ToString()
		{
			String axisInfo = $"{ this.axisVal} along { (Point.Axes)this.axis} axis" ;
			String info = $"\nKd-Interior split on " + axisInfo + "\n" +
				$" Front of {axisInfo}: {this.front} \n " +
				$"Rear of {axisInfo}: {this.rear}\n "; //retrieve axis String by value; (int) Axes.X; //convert enum val to string

			return info;
		}
	}
}
