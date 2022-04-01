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
		private static int MAX_KD_DEPTH = 3 * 10;
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

		public bool terminal( List<SceneObject> objects, AABB vox, int recDepth )
		{
			return (recDepth >= MAX_KD_DEPTH || objects.Count <= 1 );
		}

		/*get node... starts as //getNode( allObjects, sceneBoundingBox)
		* if (Terminal (L, V)) return new leaf node (L)
		* Find partition plane P
		* Split V with P producing VFRONTand VREAR
		* Partition elements of L producing LFRONTand LREAR
		* return new interior node (P, getNode(LFRONT, VFRONT),
		*  getNode(LREAR, VREAR))
		*/
		public KdNode getNode( List<SceneObject> objects, AABB vox, int depth )
		{
			//base case
			if (terminal( objects, vox, depth ))
				return new KdLeafNode(objects);

			int axis = depth % 3;
			float partitionVal = vox.center.getAxisCoord( axis );
			Vector splitVec = vox.extents;

			/* to split an AABB:
			keep min and max
			translate center point by the extents that aren't the axis we are splitting along
			for x split:
			axisVal = center.X
			new vox(minpt, center + [0, yExt, zExt]
			new vox( maxpt, center - [0, yExtm zExt]

			alternatively: translate the min and max pts along by the plane extent we're splitting along:
			i.e. new vox( minPt, maxPt - [xExt,0, ]
				new vox( maxpt, minPt + [xExt,0, ])
			*/
			switch (axis)
			{
				case 0:
					splitVec.v1 = 0;
					break;
				case 1:
					splitVec.v2 = 0;
					break;
				case 2:
					splitVec.v3 = 0;
					break;
			}

			AABB vFront = new AABB( vox.min, vox.center + splitVec, axis );
			AABB vRear = new AABB( vox.max, vox.center - splitVec, axis );
			List<SceneObject> frontObjs = new List<SceneObject>();
			List<SceneObject> rearObjs = new List<SceneObject>();

			//check if new voxels contain these objects
			foreach (SceneObject obj in objects)
			{
				Sphere s = obj as Sphere;
				Polygon t = obj as Polygon;
				bool hitsFront = false;
				bool hitsRear = false;

				if( s != null)
				{
					hitsFront = vFront.sphereIntersect( s.center, s.radius );
					hitsRear = vRear.sphereIntersect( s.center, s.radius );
				}
				else if( t != null)
				{
					hitsFront = vFront.triangleIntersect( t );
					hitsRear = vRear.triangleIntersect( t );
				}

				if (hitsFront) frontObjs.Add( obj );
				if (hitsRear) rearObjs.Add( obj );
			}

			return new KdInteriorNode( axis, partitionVal, 
				getNode(frontObjs, vFront, depth + 1), getNode( rearObjs, vRear, depth + 1 ) );
		}
		
		// a given ray traverses the tree
		public void travelTAB( LightRay ray )
		{
			return;
		}

		// if time permits
		public float doSAH()
		{
			return 0.0f;
		}
	}
}
