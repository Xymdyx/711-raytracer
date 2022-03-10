using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer_App.Kd_tree
{
	public class KdTree
	{
		//fields
		private KdNode _root;

		public KdNode root { get => this._root; set => this._root = value; }

		// constructors
		public KdTree() 
		{
			this._root =  new KdInteriorNode();
		}

		public KdTree( KdNode root )
		{
			this._root = root;
		}

		public float doRoundRobin()
		{
			return 0.0f;
		}

		// if time permits
		public float doSAH()
		{
			return 0.0f;
		}
	}
}
