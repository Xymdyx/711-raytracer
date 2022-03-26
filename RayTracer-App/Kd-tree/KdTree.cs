using System;
using System.Collections.Generic;
using System.Text;
using RayTracer_App.Voxels;
using RayTracer_App.Scene_Objects;

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

		//methods

		//traverse tree using TA-B algo specified in 6-2 slides
		public KdLeafNode traverse( Point entry, Point exit, float splitOffset )
		{
			return null;
		}

		public bool terminal( List<SceneObject> objects, Voxel vox, int recDepth )
		{
			return true;
		}

		//get node... starts as //getNode( allObjects, sceneBoundingBox)
		public KdNode getNode( List<SceneObject> objects, Voxel vox, int axis )
		{
			/*
			 * if (Terminal (L, V)) return new leaf node (L)
			Find partition plane P
			Split V with P producing VFRONTand VREAR
			Partition elements of L producing LFRONTand LREAR
			return new interior node (P, getNode(LFRONT, VFRONT),
			getNode(LREAR, VREAR))
			*/

			return new KdInteriorNode();
		}

		//methods for splitting the tree
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
