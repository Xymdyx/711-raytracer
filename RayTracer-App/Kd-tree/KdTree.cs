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
		private static int MAX_KD_DEPTH = 5 * 4;
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
		
		//helper for finding Vector to translate points along split plane for an AABB
		private Vector findSplitVec( AABB vox, int axis )
		{
			Vector splitVec = vox.extents.copy();
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

			return splitVec;
		}

		public bool terminal( List<SceneObject> objects, AABB vox, int recDepth )
		{
			return (recDepth >= (MAX_KD_DEPTH ) || objects.Count <= 2 ); //the spheres are in the same voxel...
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
			Vector splitVec = findSplitVec( vox, axis );

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

		//helper for traverseTAB
		private Point splitPlaneRayIntersection( LightRay ray, Point center, Vector sVec )
		{
			//this is the plane defined by a fixed axis value on a major axis. The other two points can have different values that ultimately evaluate to normal vector of 1 in the appropriate axis

			//https://web.ma.utexas.edu/users/m408m/Display12-5-4.shtml
			//https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
			return new Point();
		}
		
		// a given ray traverses the tree and gets the closest intersection
		public float travelTAB( LightRay ray, AABB parentBox, KdNode node = null )
		{
			// a[coord] = proper coord of entry pt
			// b[coord] = proper coord of exit point
			// s = splitting plane offset... from splitting-plane ray intersection
			//ray must intersect whole scene bounding box prior
			Point entryPt = null;
			Point exitPt = null;
			Point sPlanePt = null;
			Vector sVec = null;

			if (node == null)
				node = this.root;


			KdLeafNode leaf = node as KdLeafNode;
			KdInteriorNode inner = node as KdInteriorNode;

			if (leaf != null)
				return 0.0f ;// test all intersections with objects and return the closest;
			
			else if( inner != null)
			{
				parentBox.intersect( ray ); // update entry and exit points
				sVec = findSplitVec( parentBox, inner.axis );


			}

			return float.MaxValue; //error
		}

		// if time permits
		public float doSAH()
		{
			return 0.0f;
		}

		public override string ToString()
		{
			return this.root.ToString();
		}
	}
}
