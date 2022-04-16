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
using RayTracer_App.Photon_Mapping;

namespace RayTracer_App.Kd_tree
{
	public class ptKdTree
	{
		//fields
		private KdNode _root;

		private int _maxLeafObjs; //will be removed for points

		private int _kdSize;
		
		private static int MAX_KD_DEPTH = 5 * 4;
		public KdNode root { get => this._root; set => this._root = value; }
		public int maxLeafObjs { get => this._maxLeafObjs; set => this._maxLeafObjs = value; }
		public int kdSize { get => this._kdSize; set => this._kdSize = value; }

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

		public bool terminal( List<Point> points, int depth )
		{
			return ( points.Count <= 1 ); //the spheres are in the same voxel...
		}

		// find longest axis of bounding box
		public int getLongestAxis( AABB vox )
		{
			float xDiff = vox.max.x - vox.min.x;
			float yDiff = vox.max.y - vox.min.y;
			float zDiff = vox.max.z - vox.min.z;

			float maxDiff = (float)Math.Max( xDiff, Math.Max( yDiff, zDiff ) );

			if (xDiff == maxDiff)
				return 0;
			else if (yDiff == maxDiff)
				return 1;
			else if( zDiff == maxDiff)
				return 2;

			return -1;
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
		public KdNode balance( List<Point> points, int depth, PhotonRNG mapper,
			float prevAxis = float.MaxValue, PhotonRNG.MAP_TYPE sampleList = PhotonRNG.MAP_TYPE.GLOBAL ) //need two extra defaults for initial purposes
		{
			//base case
			if (terminal( points, depth ))
			{
				Photon stored = null;

				if (points.Count != 0)
					stored = mapper.grabPhotonByPos( points[0], sampleList ); //grab proper photon from photon list we're building from
				else Console.WriteLine( " Leaf node with no photon" );

				return new ptKdLeafNode( stored, prevAxis );
			}

			AABB vox = AABB.boxAroundPoints( points ); // form box from points
			int axis = getLongestAxis( vox ); // choose dim of cube with biggest difference betw points.

			//use lambda to sort points by longest axis value .. Tried sorting the y axis by descending FAILED.
			points.Sort( ( p1, p2 ) => p1.getAxisCoord( axis ).CompareTo(p2.getAxisCoord( axis )) ); 

			int size = points.Count;
			int midIdx = (size - 1) / 2;
			Point medianPt;

			// if median is odd.. list[n/2]
			if (size % 2 == 1)
				medianPt = points[midIdx];
			else //22 is mid of 46
				medianPt = (points[midIdx] + points[midIdx + 1].toVec()) * .5f; 
			//this will give median index of sorted list.. median https://www.statisticshowto.com/probability-and-statistics/statistics-definitions/median/
			int amount = size - (midIdx + 1) ; //23 for 46 elements

			float partitionVal = medianPt.getAxisCoord( axis ); //we split along median...
			List<Point> frontPts = points.GetRange( midIdx + 1 , amount); //skip over middle for odd-sized sets
			List<Point> rearPts = points.GetRange( 0, amount );

			return new ptKdInteriorNode( axis, partitionVal, vox,
				balance(frontPts, depth + 1, mapper, axis, sampleList),
				balance( rearPts, depth + 1, mapper, axis, sampleList ), medianPt ); //swapping rear and front pts didn't seem to work
		}
		
		private bool intersectGood( float currW )
		{
			if (currW < 0)
				Console.WriteLine( " Negative distance evaluated in ptKdTree intersect..." );
			return (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue); //the distance cannot be negative, must be positive(?)
		}

		// a given ray traverses the tree and gets the closest intersection
		// There's a problem with this traversal method. Building the map works well
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

			ptKdLeafNode leaf = node as ptKdLeafNode;
			ptKdInteriorNode inner = node as ptKdInteriorNode;

			if (leaf != null)
				return leaf.stored.rayPhotonIntersect( ray ) ;// test ray photon intersection;
			
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

		public override string ToString()
		{
			return this.root.ToString();
		}
	}
}
