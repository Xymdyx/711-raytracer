/*
desc: kdTree interior that uses points rather than voxels
date started: 4/4
date due: 4/29
 */

using System;
using RayTracer_App.Voxels;
using RayTracer_App.Photon_Mapping;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Kd_tree
{
	public class ptKdInteriorNode : KdNode
	{
		//fields
		private int _axis;
		private float _axisVal;
		private Point _partitionPt;
		private KdNode _front;
		private KdNode _rear;
		private AABB _selfAABB;
		private Photon _stored; 

		//properties
		public int axis { get => this._axis; set => this._axis = value; }
		public float axisVal { get => this._axisVal; set => this._axisVal = value; }
		public Point partitionPt { get => this._partitionPt; set => this._partitionPt = value; }
		public KdNode front { get => this._front; set => this._front = value; }
		public KdNode rear { get => this._rear; set => this._rear = value; }
		public AABB selfAABB { get => this._selfAABB; set => this._selfAABB = value; }
		public Photon stored { get => this._stored; set => this._stored = value; }

		//constructors
		public ptKdInteriorNode()
		{
			this._axis = 0;
			this._axisVal = 0.0f;
			this._partitionPt = null;
			this._selfAABB = null;
			this._front = null;
			this._rear = null;
			this.stored = null;
		}

		public ptKdInteriorNode( int axis, float axisVal, AABB selfAABB, KdNode front, KdNode rear, Point partitionPt = null, Photon stored = null)
		{
			this._axis = axis;
			this._axisVal = axisVal;
			this._partitionPt = partitionPt;
			this._selfAABB = selfAABB;
			this._front = front;
			this._rear = rear;
			this._stored = stored;
		}

		public void debugPrint()
		{
			String axisInfo = $"{ this.axisVal} along { (Point.Axes)this.axis} axis at pt {partitionPt}";
			String info = $"Kd-Interior split on " + axisInfo;
			Console.WriteLine(  info );
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
