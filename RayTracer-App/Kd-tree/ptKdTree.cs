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

		private int _kdLevels; // tells how many levels we have

		private int _kdSize;

		private int _photonNum;

		private List<KdNode> _pmHeap; //for locating photons helper... some nodes won't have photons...

		private static int MAX_KD_DEPTH = 5 * 4;
		public KdNode root { get => this._root; set => this._root = value; }
		public int maxLeafObjs { get => this._maxLeafObjs; set => this._maxLeafObjs = value; }
		public int kdLevels { get => this._kdLevels; set => this._kdLevels = value; }
		public int kdSize { get => this._kdSize; set => this._kdSize = value; }
		public int photonNum { get => this._photonNum; set => this._photonNum = value; }
		public List<KdNode> pmHeap { get => this._pmHeap;}

		// constructors
		public ptKdTree( )
		{
			this._root = null;
			this._pmHeap = new List<KdNode>();
			this._maxLeafObjs = 2;
			this.kdLevels = 0;
			this.kdSize = 0;
			this.photonNum = 0;
		}

		public ptKdTree( KdNode root, int pSize )
		{
			this._root = root;
			this._pmHeap = new List<KdNode>();
			this._maxLeafObjs = 2;
			this.kdLevels = 1;
			this.kdSize = 1;
			this.photonNum = 0;
		}

		//methods
		//called before building photon maps so we can fill up the heap as we go.
		public void fillHeap( int total )
		{
			for (int idx = 0; idx < total; idx++)
				this.pmHeap.Add( null );
		}

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

		/* FOR PM: The balancing http://graphics.ucsd.edu/~henrik/papers/rendering_caustics/rendering_caustics_gi96.pdf
		algorithm converts the unordered list of photons into a balanced kd-tree
		by recursively selecting the root node among the data-set as the median
element in the direction which represents the largest interval.*/
		public KdNode balance( List<Point> points, int depth, PhotonRNG mapper,
			float prevAxis = float.MaxValue, PhotonRNG.MAP_TYPE sampleList = PhotonRNG.MAP_TYPE.GLOBAL, int heapIdx = 1 ) //need two extra defaults for initial purposes
		{
			Photon stored = null;
			//base case
			if (terminal( points, depth ))
			{

				if (points.Count != 0)
				{
					stored = mapper.grabPhotonByPos( points[0], sampleList ); //grab proper photon from photon list we're building from
					if( stored != null)
						this.photonNum++;
					else 
						Console.WriteLine( "Leaf with no photon" );
				}
				else Console.WriteLine( " Leaf node with no photon" );

				if (depth + 1 > kdLevels) kdLevels = depth + 1; //debugging
				ptKdLeafNode ptLeaf = new ptKdLeafNode( stored, prevAxis );

				if (pmHeap.Count < heapIdx) //this ensures the heap is the right size...
					fillHeap( heapIdx );

				this.pmHeap[heapIdx - 1] = ptLeaf;
				return ptLeaf;
			}

			AABB vox = AABB.boxAroundPoints( points ); // form box from points
			int axis = getLongestAxis( vox ); // choose dim of cube with biggest difference betw points.

			//use lambda to sort points by longest axis value .. Tried sorting the y axis by descending FAILED.
			points.Sort( ( p1, p2 ) => p1.getAxisCoord( axis ).CompareTo(p2.getAxisCoord( axis )) ); 
	
			int size = points.Count;
			int midIdx = (size - 1) / 2;
			Point medianPt;

			// if median is odd.. list[n/2]
			// 22 is mid of 46
			//23 for 46 elements
			if (size % 2 == 1)
				medianPt = points[midIdx];
			else
				medianPt = (points[midIdx] + points[midIdx + 1].toVec()) * .5f; //works
			//this will give median index of sorted list.. median https://www.statisticshowto.com/probability-and-statistics/statistics-definitions/median/
			int amount = size - (midIdx + 1) ;

			float partitionVal = medianPt.getAxisCoord( axis ); //we split along median...
			List<Point> frontPts = points.GetRange( midIdx + 1 , amount); //RIGHT NODE
			List<Point> rearPts = points.GetRange( 0, amount ); //LEFT NODE

			if (frontPts.Count != rearPts.Count) Console.WriteLine("uneven list sizes in build");

			stored = mapper.grabPhotonByPos( medianPt, sampleList ); //grab proper photon from photon list we're building from.. This can be null for interior nodes
			if (stored != null)
				this.photonNum++;

			this._kdSize += 2;
			int frontIdx = (heapIdx * 2) + 1; //right child at 2i + 1... with i >= 1
			int rearIdx = (heapIdx * 2);  // left child at 2i... with i >=1

			ptKdInteriorNode ptInt = new ptKdInteriorNode( axis, partitionVal, vox,
				balance( frontPts, depth + 1, mapper, axis, sampleList, frontIdx),
				balance( rearPts, depth + 1, mapper, axis, sampleList, rearIdx ), medianPt, stored ); //swapping rear and front pts didn't seem to work

			this.pmHeap[heapIdx - 1] = ptInt;
			return ptInt;
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
		// this is one ray through the whole ass box... we compute initial pts
		// https://slideplayer.com/slide/4991637/
		//https://dcgi.fel.cvut.cz/home/havran/DISSVH/dissvh.pdf ... C psuedocode on p. 171. It's good reference for actual implementation and HARD to find
		// if you do read through my code, I strongly recommend adding this to the kdSlides since I misunderstood this algorithm immensely.
		//https://github.com/dragonbook/mitsuba-ctcloth/blob/master/include/mitsuba/render/sahkdtree2.h
		public float travelTAB( LightRay ray, World.World world, bool debug = false)
		{
//WORKS...photon shooting is jank
			// a[coord] = proper coord of entry pt
			// b[coord] = proper coord of exit point
			// s = splitting plane offset... from splitting-plane ray intersection
			//ray must intersect whole scene bounding box prior
			Point entryPt;
			Point exitPt;
			// debug vars
			int reached = 0;
			int hit = 0;
			//coords
			float aCoord;
			float bCoord;
			float splitOffset;

			//distances
			float aDist;
			float bDist;
			float splitDist;

			//kick off with root
			KdNode currNode = this.root;

			float bestW = float.MaxValue; //initialize to error per convention so far

			ptKdInteriorNode inner = currNode as ptKdInteriorNode;

			if (inner == null) return bestW; //error as there is no root
	
			inner.selfAABB.intersect( ray ); // update entry and exit points
			if (ray.entryPt == null || ray.exitPt == null) // don't intersect scene box
				return bestW;

			entryPt = ray.entryPt.copy();
			exitPt = ray.exitPt.copy();
			aDist = inner.selfAABB.tNear;
			bDist = inner.selfAABB.tFar;

			// the stack
			List<KdStackEl> stack = new List<KdStackEl>();

			for (int idx = 0; idx < KdStackEl.MAX_STACK_SIZE; idx++)
				stack.Add( new KdStackEl() );

			KdNode farChild;

			int entIdx = 0;
			stack[entIdx].t = aDist; //set signed distance of entry pt

			//external ray origin
			if (aDist >= 0.0)
				stack[entIdx].pb = entryPt;
			else
				stack[entIdx].pb = ray.origin;

			int extIdx = 1; //stack exit ptr
			stack[extIdx].t = bDist;
			stack[extIdx].pb = exitPt; //ray.origin + ray.dir * b... aka findPtAlong
			stack[extIdx].kdNode = null; //termination flag
			int depth = 0;
			// N = negative, P = positive, Z = along splitting plane
			while (currNode != null) //while we point to somewhere
			{
				ptKdLeafNode leaf;
				while ((leaf = currNode as ptKdLeafNode) == null)
				{
					depth++;
					ptKdInteriorNode currInner = currNode as ptKdInteriorNode;
					// need this to account for N4 and P4, where we recompute s to sub into a or b in children. Need these axes
					int currAxis = currInner.axis;
					int nextAxis = Point.getNextAxis( currAxis );
					int prevAxis = Point.getPrevAxis( currAxis );
					splitOffset = currInner.axisVal; // - ray.origin.getAxisCoord(inner.axis);

					aCoord = stack[entIdx].pb.getAxisCoord( currAxis );
					bCoord = stack[extIdx].pb.getAxisCoord( currAxis ); //the exit point should remain the same...

					//distance not an issue, it's the coords

					if (aCoord <= splitOffset)
					{
						if (bCoord <= splitOffset) //visit leftnode
						{
							currNode = currInner.rear; //N1, N2, N3, P5, Z3, Z2 						//Z2 visit arbitrary child node
							continue;
						}
						if (aCoord == splitOffset) //typo in thesis orginally "bCoord == splitOffset" Z1
						{
							currNode = currInner.front;
							continue;
						}
						// visit left then right (lower -> upper)
						currNode = currInner.rear;
						farChild = currInner.front; //getRight                //N4
					}
					else // aCoord > splitOffset
					{
						if (bCoord > splitOffset)
						{//visit right
							currNode = currInner.front; // P1, P2, P3, N5
							continue;
						}
						//visit right then left (upper -> lower)
						farChild = currInner.rear; //getLeft //P4
						currNode = currInner.front; //getRight
					}

					//case P4 or N4... traverse both children
					// recompute splitting offset
					splitDist = (splitOffset - ray.origin.getAxisCoord( currAxis )) / ray.direction.getAxisComp( currAxis );

					//setup new exit pt
					int tmp = extIdx;
					extIdx++;

					// possibly skip current entry pt
					if (extIdx == entIdx)
						extIdx++;

					//push values to stack
					float nextCoord = ray.origin.getAxisCoord( nextAxis ) + splitDist * ray.direction.getAxisComp( nextAxis );
					float prevCoord = ray.origin.getAxisCoord( prevAxis ) + splitDist * ray.direction.getAxisComp( prevAxis );
					stack[extIdx].prev = tmp;
					stack[extIdx].t = splitDist;
					stack[extIdx].kdNode = farChild;
					stack[extIdx].pb.setAxisCoord( currAxis, splitOffset );
					stack[extIdx].pb.setAxisCoord( nextAxis, nextCoord );
					stack[extIdx].pb.setAxisCoord( prevAxis, prevCoord );
				} //end leaf while

				// found leaf
				bestW = leaf.leafIntersect( ray, aDist, bDist, false );// test ray photon intersection... turning off the distance checks did result in more visible photons in map

				reached++;
				if (intersectGood( bestW ))
				{   //found it, stop
					if(debug)
						Console.WriteLine( $"Exit traversal loop with intersection. Terminated after {depth} iterations. Reached {reached} leaves" );
					return bestW;
				}
								
				bestW = float.MaxValue; //just in case

				//pop from stack
				entIdx = extIdx; //signed distance intervals are adjacent
				currNode = stack[extIdx].kdNode; //retrieve next node, possible that ray traversal stops

				extIdx = stack[entIdx].prev;
			} // end outer search whi;e

			//Console.WriteLine( $"Exited tab loop with no intersection. Terminated after {depth} iterations" );
			return bestW; //found no intersection
		}


		//private Point recomputeS( int axis, float sCoord, LightRay ray )
		//{
		//	Point newS = new Point( 0, 0, 0 );
		//	newS.setAxisCoord( axis, sCoord );
		//}

		//print general PM stats
		public String pmPrint()
		{
			return $" PM with {this.photonNum} photons and {this.kdSize} nodes with {this.kdLevels} levels";
		}

		//print the max heap we use for gathering photons
		public void pmHeapPrint( bool showInt = true, bool showLeaf = true)
		{
			int heapEl = 1;
			foreach( KdNode kd in pmHeap)
			{
				ptKdInteriorNode ptIn;
				ptKdLeafNode pLeaf;
				if ( (showInt) && (ptIn = kd as ptKdInteriorNode) != null)
				{
					Console.WriteLine( $"Heap element {heapEl}:" );
					ptIn.debugPrint();
				}
				else if ( (showLeaf) && (pLeaf = kd as ptKdLeafNode) != null)
				{
					Console.WriteLine( $"Heap element {heapEl}:" );
					pLeaf.debugPrint();
				}
				heapEl++;
			}
		}

		public override string ToString()
		{
			return $" PM with {this.photonNum} photons and {this.kdSize} nodes with {this.kdLevels} levels\n" + this.root.ToString();
		}
	}
}

/* Misunderstood TAB algo(?)
 		public float travelTAB( LightRay ray, World.World world ,KdNode node = null, Point s = null, int flag = 0  )
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

				// need this to account for N4 and P4, where we recompute s to sub into a or b in children
				if (flag == 1) entryPt = s.copy();
				else if (flag == 2) exitPt = s.copy();

				aCoord = entryPt.getAxisCoord( inner.axis );
				bCoord = exitPt.getAxisCoord( inner.axis ); //the exit point should remain the same...
				splitOffset = inner.axisVal; // - ray.origin.getAxisCoord(inner.axis);

				if( aCoord <= splitOffset)
				{
					if (bCoord < splitOffset) //visit leftnode
						bestW = travelTAB( ray, world, inner.rear ); //N1, N2, N3, P5, Z3
					else
					{
						if (bCoord == splitOffset)
							bestW = travelTAB( ray, world, inner.front ); //traverse arbitrary child node.. Z2
						else
						{ // visit left then right (lower -> upper)
							//compute and store COMPLETE location of splitOffset....this means that the offset's full point is needed...replace a or b?
							//recommends a stack since the newly computer s is used later
							bestW = travelTAB( ray, world, inner.rear, inner.partitionPt, 2 ); //tried: 12 both, 21 both, 21 12, 12 21

							if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

							bestW = travelTAB( ray, world, inner.front, inner.partitionPt, 1 ); //N4
						}
					}
				}
				else // aCoord > splitOffset
				{
					if (bCoord > splitOffset) //visit right
						bestW = travelTAB( ray, world, inner.front ); // P1, P2, P3, N5, Z1
					else
					{ //visit right then left (upper -> lower)
						//compute and store location of splitOffset....I believe this is already done(?)
						bestW = travelTAB( ray, world,  inner.front, inner.partitionPt, 2 );

						if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

						bestW = travelTAB( ray, world, inner.rear, inner.partitionPt, 1 ); //P4
					}
				}
			}

			return bestW; //error
		}
 
// using same ray entry and exit whole time
		public float travelTAB( LightRay ray, World.World world , int depth, KdNode node = null, Point a = null, Point b = null)
		{
			// a[coord] = proper coord of entry pt
			// b[coord] = proper coord of exit point
			// s = splitting plane offset... from splitting-plane ray intersection
			//ray must intersect whole scene bounding box prior
			Point entryPt = null;
			Point exitPt = null;
			Point sPlanePt;
			Vector sVec;
			float aCoord;
			float bCoord;
			float splitOffset;

			ptKdLeafNode leaf;
			ptKdInteriorNode inner;

			float bestW = float.MaxValue; //initialize to error per convention so far

			leaf = node as ptKdLeafNode;
			inner = node as ptKdInteriorNode;

			if (node == null && leaf == null && inner == null) //this works
			{
				leaf = this.root as ptKdLeafNode;
				inner = this.root as ptKdInteriorNode;

				if (inner != null)
				{
					inner.selfAABB.intersect( ray ); // update entry and exit points
					entryPt = ray.entryPt.copy();
					exitPt = ray.exitPt.copy();
					if (ray.entryPt == null || ray.exitPt == null)
						return bestW;
				}
			}

			if (leaf != null)
				return leaf.stored.rayPhotonIntersect( ray ) ;// test ray photon intersection;
			
			// N = negative, P = positive, Z = along splitting plane
			else if( inner != null)
			{
				// need this to account for N4 and P4, where we recompute s to sub into a or b in children
				if( entryPt == null)
					entryPt = a.copy();
				if( exitPt == null)
					exitPt = b.copy();
				float originOffset = ray.origin.getAxisCoord( inner.axis );
				aCoord = entryPt.getAxisCoord( inner.axis );
				bCoord = exitPt.getAxisCoord( inner.axis ); //the exit point should remain the same...

				if (aCoord > bCoord) //a must be smaller than b
				{
					(aCoord, bCoord) = (bCoord, aCoord); // tuples let me swap variables w/o temps
					//Console.WriteLine( $" a is not smaller than b here for a = {entryPt} , b = {exitPt} " );
				}
				splitOffset = inner.axisVal; // - ray.origin.getAxisCoord(inner.axis);

				if( aCoord <= splitOffset)
				{
					if (bCoord < splitOffset) //visit leftnode
						bestW = travelTAB( ray, world, depth + 1, inner.rear, entryPt, exitPt ); //N1, N2, N3, P5, Z3
					else
					{
						if (bCoord == splitOffset)
							bestW = travelTAB( ray, world, depth + 1, inner.front, entryPt, exitPt ); //traverse arbitrary child node.. Z2
						else
						{ // visit left then right (lower -> upper)
							//compute and store COMPLETE location of splitOffset....this means that the offset's full point is needed...replace a or b?
							//recommends a stack since the newly computer s is used later
							bestW = travelTAB( ray, world, depth + 1, inner.rear, entryPt, inner.partitionPt ); //tried

							if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

							bestW = travelTAB( ray, world, depth + 1, inner.front, inner.partitionPt, exitPt ); //N4.. front has s as entry, b same
						}
					}
				}
				else // aCoord > splitOffset
				{
					if (bCoord > splitOffset) //visit right
						bestW = travelTAB( ray, world, depth + 1, inner.front, entryPt, exitPt ); // P1, P2, P3, N5, Z1
					else
					{ //visit right then left (upper -> lower)
						//compute and store location of splitOffset....I believe this is already done(?)
						bestW = travelTAB( ray, world, depth + 1,  inner.front, entryPt, inner.partitionPt );

						if ( intersectGood( bestW ) ) return bestW; //return here since we know first check will def be closer

						bestW = travelTAB( ray, world, depth + 1, inner.rear, inner.partitionPt, exitPt ); //P4
					}
				}
			}

			return bestW; //error
		}
 */