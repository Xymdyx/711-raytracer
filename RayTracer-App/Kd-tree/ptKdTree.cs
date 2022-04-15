/*
desc: kdTree that splits via points and stores photons
date started: 4/4
date due: 4/29
 */

using System;
using System.Collections.Generic;
using RayTracer_App.World;
using RayTracer_App.Voxels;
using RayTracer_App.Scene_Objects;

namespace RayTracer_App.Kd_tree
{
	public class ptKdTree
	{
		//fields
		private KdNode _root;

		private int _maxLeafObjs;

		private static int MAX_KD_DEPTH = 5 * 4;
		public KdNode root { get => this._root; set => this._root = value; }
		public int maxLeafObjs { get => this._maxLeafObjs; set => this._maxLeafObjs = value; }

		// constructors
		public ptKdTree() 
		{
			this._root =  null;
			this._maxLeafObjs = 2;
		}

		public ptKdTree( KdNode root )
		{
			this._root = root;
			this._maxLeafObjs = 2;
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

		public bool terminal( List<SceneObject> objects, AABB vox, int depth )
		{
			return ( objects.Count <= maxLeafObjs ); //the spheres are in the same voxel...
		}

		/*get node... starts as //getNode( allObjects, sceneBoundingBox)
		* if (Terminal (L, V)) return new leaf node (L)
		* Find partition plane P
		* Split V with P producing VFRONTand VREAR
		* Partition elements of L producing LFRONTand LREAR
		* return new interior node (P, getNode(LFRONT, VFRONT),
		*  getNode(LREAR, VREAR))
		*/

		/* FOR PM: The balancing http://graphics.ucsd.edu/~henrik/papers/rendering_caustics/rendering_caustics_gi96.pdf
		algorithm converts the unordered list of photons into a balanced kd-tree
		by recursively selecting the root node among the data-set as the median
element in the direction which represents the largest interval.*/
		public KdNode getNode( List<SceneObject> objects, AABB vox, int depth )
		{
			//base case
			if (terminal( objects, vox, depth ))
				return new ptKdLeafNode(objects);

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

			AABB vFront = new AABB( vox.max, vox.center - splitVec , axis );
			AABB vRear = new AABB( vox.min, vox.center + splitVec, axis );
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

			return new ptKdInteriorNode( axis, partitionVal, vox,
				getNode(frontObjs, vFront, depth + 1), getNode( rearObjs, vRear, depth + 1 ) );
		}
		
		private bool intersectGood( float currW )
		{
			return (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue);
		}

		// a given ray traverses the tree and gets the closest intersection
		public float travelTAB( LightRay ray, World.World world ,KdNode node = null )
		{
			// a[coord] = proper coord of entry pt
			// b[coord] = proper coord of exit point
			// s = splitting plane offset... from splitting-plane ray intersection
			//ray must intersect whole scene bounding box prior
			Point entryPt;
			Point exitPt;
			Point sPlanePt;
			Vector sVec;
			float aCoord;
			float bCoord;
			float splitOffset;

			float bestW = float.MaxValue; //initialize to error per convention so far

			if (node == null)
				node = this.root;

			KdLeafNode leaf = node as KdLeafNode;
			KdInteriorNode inner = node as KdInteriorNode;

			if (leaf != null)
				return world.findRayIntersect( ray, leaf.objectPtrs ) ;// test all intersections with objects and return the closest;
			
			// N = negative, P = positive, Z = along splitting plane
			else if( inner != null)
			{
				inner.selfAABB.intersect( ray ); // update entry and exit points

				if (ray.entryPt == null || ray.exitPt == null)
					return bestW;

				entryPt = ray.entryPt.copy();
				exitPt = ray.exitPt.copy();
				aCoord = entryPt.getAxisCoord( inner.axis );
				bCoord = exitPt.getAxisCoord( inner.axis );
				splitOffset = inner.axisVal;

				if( aCoord <= splitOffset)
				{
					if (bCoord < splitOffset)
						bestW = travelTAB( ray, world, inner.rear ); //N1, N2, N3, P5, Z3
					else
					{
						if (bCoord == splitOffset)
							bestW = travelTAB( ray, world, inner.front ); //traverse arbitrary child node.. Z2
						else
						{
							//compute and store location of splitOffset....I believe this is already done(?)
							bestW = travelTAB( ray, world, inner.rear );

							if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

							bestW = travelTAB( ray, world, inner.front ); //N4
						}
					}
				}
				else // aCoord > splitOffset
				{
					if (bCoord > splitOffset)
						bestW = travelTAB( ray, world, inner.front ); // P1, P2, P3, N5, Z1
					else
					{
						//compute and store location of splitOffset....I believe this is already done(?)
						bestW = travelTAB( ray, world,  inner.front );

						if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

						bestW = travelTAB( ray, world, inner.rear ); //P4
					}
				}


			}

			return bestW; //error
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
