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
		private List<Photon> _jensenHeap; //jensen heap

		private AABB bBox;

		private static int MAX_KD_DEPTH = 5 * 4;
		public KdNode root { get => this._root; set => this._root = value; }
		public int maxLeafObjs { get => this._maxLeafObjs; set => this._maxLeafObjs = value; }
		public int kdLevels { get => this._kdLevels; set => this._kdLevels = value; }
		public int kdSize { get => this._kdSize; set => this._kdSize = value; }
		public int photonNum { get => this._photonNum; set => this._photonNum = value; }
		public List<KdNode> pmHeap { get => this._pmHeap;}
		public List<Photon> jensenHeap { get => this._jensenHeap; }

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
/// 
/// /////////////////////////////////////////////////////////ORIGINAL METHODS////////////////////////////////////////////////
/// 
/// 
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

			AABB vox = AABB.boxAroundPoints( points ); 
			int axis = getLongestAxis( vox );

			//use lambda to sort points by longest axis value .. Tried sorting the y axis by descending FAILED.
			points.Sort( ( p1, p2 ) => p1.getAxisCoord( axis ).CompareTo(p2.getAxisCoord( axis )) ); 
	
			int size = points.Count;
			int midIdx = (size - 1) / 2;
			Point medianPt;

			if (size % 2 == 1)
				medianPt = points[midIdx];
			else            //this will give median point of sorted list.. median https://www.statisticshowto.com/probability-and-statistics/statistics-definitions/median/
				medianPt = (points[midIdx] + points[midIdx + 1].toVec()) * .5f;
			
			////... Jensen gets MEDIAN INDEX using an efficient median algo comprisied of 3 methods :/
			int amount = size - (midIdx + 1) ;

			float partitionVal = medianPt.getAxisCoord( axis ); //we split along median...
			List<Point> frontPts = points.GetRange( midIdx + 1 , amount); //RIGHT NODE
			List<Point> rearPts = points.GetRange( 0, amount ); //LEFT NODE

			if (frontPts.Count != rearPts.Count) Console.WriteLine("uneven list sizes in build");

			stored = mapper.grabPhotonByPos( medianPt, sampleList ); //may be null in interor node
			if (stored != null)
				this.photonNum++;

			this._kdSize += 2;
			int frontIdx = (heapIdx * 2) + 1; //right child at 2i + 1... with i >= 1
			int rearIdx = (heapIdx * 2);  // left child at 2i... with i >=1

			// the rightside of the tree is being built first....!!!!!!!! 4/24 LAST
			ptKdInteriorNode ptInt = new ptKdInteriorNode( axis, partitionVal, vox,
				balance( frontPts, depth + 1, mapper, axis, sampleList, frontIdx),
				balance( rearPts, depth + 1, mapper, axis, sampleList, rearIdx ), medianPt, stored ); //swapping rear and front pts didn't seem to work... tried 2+ times

			this.pmHeap[heapIdx - 1] = ptInt;
			return ptInt;
		}
		
		//helper for unused traversal algo...
		private bool intersectGood( float currW )
		{
			if (currW < 0)
				Console.WriteLine( " Negative distance evaluated in ptKdTree intersect..." );
			return (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue); //the distance cannot be negative, must be positive(?)
		}

		// https://slideplayer.com/slide/4991637/
		//https://dcgi.fel.cvut.cz/home/havran/DISSVH/dissvh.pdf ... C psuedocode on p. 171. It's good reference for iterative implementation. typo in thesis orginally "bCoord == splitOffset" Z1
		//https://github.com/dragonbook/mitsuba-ctcloth/blob/master/include/mitsuba/render/sahkdtree2.h
		// if you do read through my code, I think this might be a helpful addition to the kdSlides. It's a faster alternative approach since it doesn't use the call stack
		public float travelTAB( LightRay ray, World.World world, bool debug = false)
		{
			// a[coord] = proper coord of entry pt
			// b[coord] = proper coord of exit point
			// s = splitting plane offset... from splitting-plane ray intersection
			//ray must intersect whole scene bounding box prior
			Point entryPt;
			Point exitPt;

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

			KdNode currNode = this.root;

			float bestW = float.MaxValue; //initialize to error per convention so far

			ptKdInteriorNode inner = currNode as ptKdInteriorNode;

			if (inner == null) return bestW;
	
			inner.selfAABB.intersect( ray ); // update entry and exit points
			if (ray.entryPt == null || ray.exitPt == null)
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
							currNode = currInner.rear; //N1, N2, N3, P5, Z3, Z2
							continue;
						}
						if (aCoord == splitOffset) //typo in thesis orginally "bCoord == splitOffset" Z1
						{
							currNode = currInner.front;
							continue;
						}
						// visit left then right (lower -> upper)
						currNode = currInner.rear;
						farChild = currInner.front; //getRight //N4
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
			} // end outer search while

			//Console.WriteLine( $"Exited tab loop with no intersection. Terminated after {depth} iterations" );
			return bestW; //found no intersection
		}



// locate the photons given a ray's intersection position with an object
//given radius squared...Make sure to be consistent...
		public unsafe void locatePhotons( int loc, int k, Point pos, float* radPtr, MaxHeap<Photon> heap )
		{
			float rad = *radPtr;
			ptKdInteriorNode inside = pmHeap[loc - 1] as ptKdInteriorNode;
			ptKdLeafNode leaf = pmHeap[loc - 1] as ptKdLeafNode;
			if (((loc * 2) + 1) < this.pmHeap.Count)
			{
				//examine children via recursion
				if (inside != null)
				{
					float dist1 = pos.getAxisCoord( inside.axis ) - inside.partitionPt.getAxisCoord(inside.axis); //switching this around seems to help...
					if (dist1 < 0)
					{
						locatePhotons( 2 * loc, k, pos, radPtr, heap ); //visit the left/rear child
						if (dist1 * dist1 < *radPtr)
							locatePhotons( (2 * loc) + 1, k, pos, radPtr, heap ); //then visit the right/front child
					}
					else
					{
						locatePhotons( (2 * loc) + 1, k, pos, radPtr, heap ); //visit the right/front child
						if (dist1 * dist1 < *radPtr)
							locatePhotons( 2 * loc, k, pos, radPtr, heap ); //then visit the left/rear child
					}
				}
			}

			//must check since a few interior nodes may NOT have photons
			rad = *radPtr;
			Photon phot = null;
			if (inside != null &&  inside.stored != null) phot = inside.stored;
			else if (leaf != null) phot = leaf.stored;

			if (phot != null) //know this works... 
			{
				Point pLeaf = phot.pos;
				float xD = (pLeaf.x - pos.x);
				float yD = (pLeaf.y - pos.y);
				float zD = (pLeaf.z - pos.z);
				float dist2 = xD * xD + yD * yD + zD * zD;

				//insert into max heap and update search squared radius
				if (dist2 < rad )
				{
					heap.InsertElementInHeap( dist2, phot ); //tested this...4/24
					*radPtr = (float) heap.doubleMazHeap[1]; //the search radius is the distance to root node in max heap -_-. I was assigning it to dist2
				}
			}

			return;
		}


/////////////////////////////////jensen version of methods... ONE BIG SANITY CHECK/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Jensen building method for quasi-heap rep of left-balanced kd tree
		// postcondition: photons will be re-arranged and assigned to Jensen heap
		public void balanceJensen( List<Photon> photons )
		{
			int totalPhots = photons.Count;
			if (totalPhots > 1)
			{
				//temp arrays for balancing
				List<Photon> temp1 = new List<Photon>();
				for (int i = 0; i < totalPhots + 1; i++)
					temp1.Add( null );

				List<Photon> temp2 = photons.ConvertAll( phot => phot.copy() );
				List<Point> photPts = photons.ConvertAll( phot => phot.pos.copy() );

				//we purely need this because Jensen uses ptr arithmetic to determine offset between photons in memory.
				// It's like a blank template memory block for the C# equivalent
				List<Photon> memList = photons.ConvertAll( phot => phot.copy() ); 

				//placeholders to match Jensen's 1-indexing
				temp2.Insert( 0, null ); 
				photons.Insert( 0, null );
				memList.Insert( 0, null );

				//make a bounding box for this tree
				this.bBox = AABB.boxAroundPoints( photPts );

				//this builds a balanced kd tree
				balanceSegment( temp1, temp2, 1, 1, totalPhots );

				//tracks balanced photon in temp1 location in the original photons list
				int origIdx;

				//tracker 1 and 2
				int j = 1;
				int foo = 1;
				Photon fooPhoton = photons[j]; //photon that matches foo tracker
				for (int i = 1; i <= totalPhots; i++)
				{
					origIdx = memList.IndexOf( temp1[j] ); //find the corresponding index of the balanced el in memory... from his book: "pbal[j] - photons"
					temp1[j] = null; // we've reordered this photon so set to null to ensure we don't use it again
					if (origIdx != foo)
						photons[j] = photons[origIdx];
					else
					{
						photons[j] = fooPhoton; //if in same location as the balanced tree and the original list
						if (i < totalPhots)
						{
							// WEEEEEEEEEEE until we find a new kd photon to reorder
							for (; foo <= totalPhots; foo++) 
								if (temp1[foo] != null)
									break;

							fooPhoton = photons[foo];
							j = foo;
						}
						continue;
					}
					j = origIdx;
				}
			}

			this._jensenHeap = photons; //we've made the heap in Jensen's quirky way
			this.photonNum = _jensenHeap.Count - 1;
			return;
		}

		//swap using tuples
		private void swap( List<Photon> photonList, int a, int b )
		{
			(photonList[a] , photonList[b]) = (photonList[b] , photonList[a]);
		}

		//split list along index median... organizing them into the left-plane and right-plane
		private void medianSplit( List<Photon> temp2, int start, int end, int median, int axis )
		{
			int left = start;
			int right = end;

			while( right > left)
			{
				float pivot = temp2[right].pos.getAxisCoord( axis );
				int i = left - 1;
				int j = right;

				//twirling loops to get from point a to b
				for(; ; )
				{
					while (temp2[++i].pos.getAxisCoord( axis ) < pivot)
						;
					while (temp2[--j].pos.getAxisCoord( axis ) > pivot && j > left)
						;
					if (i >= j)
						break; //break out of inner for
					swap( temp2, i, j ); //move points around median
				}

				swap( temp2, i, right );
				if (i >= median)
					right = i - 1;
				if (i <= median)
					left = i + 1;
			} //end outer while

			return;
		}

		//helper method for Jensen's building of a left-balanced kdtree
		// he basically inserts all the left-plane nodes first and then the right-plane nodes
		//he's a fan of 1-indexing
		//temp1 becomes a balanced kdTree and uses temp2 as original reference
		private void balanceSegment( List<Photon> temp1, List<Photon> temp2, int index, int start, int end )
		{
			// compute new median. 
			int median = 1;
			while ((4 * median) <= (end - start + 1))
				median += median;

			if(  (3 * median) <= ( end - start + 1))
			{
				median += median;
				median += start - 1;
			}
			else
			{
				median = end - median + 1;
			}

			//find split axis and partition photon block
			int axis = getLongestAxis( this.bBox );
			medianSplit( temp2, start, end, median, axis );

			temp1[index] = temp2[median];
			temp1[index].kdFlag = axis; //kdflags store split axis

			// recursively balance left and right block
			if( median > start)
			{
				//balance left seg
				if( start < median - 1)
				{
					float holdMax = this.bBox.max.getAxisCoord( axis );
					float tempMax = temp1[index].pos.getAxisCoord( axis );
					this.bBox.max.setAxisCoord( axis, tempMax );
					balanceSegment( temp1, temp2, 2 * index, start, median - 1 );
					this.bBox.max.setAxisCoord( axis, holdMax ); //double-check
				}
				else
					temp1[2 * index] = temp2[start];
			}

			//balance right segment
			if( median < end)
			{
				if (median + 1 < end)
				{
					float holdMin = this.bBox.min.getAxisCoord( axis );
					float tempMin = temp1[index].pos.getAxisCoord( axis );
					this.bBox.min.setAxisCoord( axis, tempMin );
					balanceSegment( temp1, temp2, 2 * index + 1, median + 1, end );
					this.bBox.min.setAxisCoord( axis, holdMin ); //double-check
				}
				else
					temp1[2 * index + 1] = temp2[end];
			}
			return;
		}

		// locate the photons given a ray's intersection position with an object
		//given radius squared via radPtr so we omit squaring operations. Uses kdFlags for split axis
		public unsafe void locatePhotonsJensen( int loc, int k, Point pos, float* radPtr, MaxHeap<Photon> heap )
		{
			float rad = *radPtr;
			Photon phot = jensenHeap[loc];
			int axis = (int)phot.kdFlag; //here we use the kdFlag to store the split plane value
			if (((loc * 2) + 1) < this.jensenHeap.Count)
			{
				//examine children via recursion
				if (phot != null)
				{
					float dist1 = pos.getAxisCoord( axis ) - phot.pos.getAxisCoord( axis ); //switching this around seems to help...
					if (dist1 < 0)
					{
						locatePhotonsJensen( 2 * loc, k, pos, radPtr, heap ); //visit the left/rear child
						if (dist1 * dist1 < *radPtr)
							locatePhotonsJensen( (2 * loc) + 1, k, pos, radPtr, heap ); //then visit the right/front child
					}
					else
					{
						locatePhotonsJensen( (2 * loc) + 1, k, pos, radPtr, heap ); //visit the right/front child
						if (dist1 * dist1 < *radPtr)
							locatePhotonsJensen( 2 * loc, k, pos, radPtr, heap ); //then visit the left/rear child
					}
				}
			}

			rad = *radPtr;
			Point pLeaf = phot.pos;
			float xD = (pLeaf.x - pos.x);
			float yD = (pLeaf.y - pos.y);
			float zD = (pLeaf.z - pos.z);
			float dist2 = xD * xD + yD * yD + zD * zD;

			//insert into max heap and update search squared radius
			if (dist2 < rad)
			{
				heap.InsertElementInHeap( dist2, phot ); //tested this...4/24
				*radPtr = (float)heap.doubleMazHeap[1]; //the search radius is the distance to root node in max heap -_-. I was assigning it to dist2
			}

			return;
		}


////////////////////////////////////.........................../DEBUG METHODS
		public override string ToString()
		{
			return $" PM with {this.photonNum} photons and {this.kdSize} nodes with {this.kdLevels} levels\n" + this.root.ToString();
		}

		//print general PM stats
		public String pmPrint()
		{
			return $" PM with {this.photonNum} photons and {this.kdSize} nodes with {this.kdLevels} levels";
		}

		//print general PM stats
		public String jensenPrint()
		{
			return $" PM with {this.photonNum} photons organized via Jensen's method ";
		}

		//print the max heap we use for gathering photons
		public void pmHeapPrint( bool showInt = true, bool showLeaf = true )
		{
			int heapEl = 1;
			foreach (KdNode kd in pmHeap)
			{
				ptKdInteriorNode ptIn;
				ptKdLeafNode pLeaf;
				if ((showInt) && (ptIn = kd as ptKdInteriorNode) != null)
				{
					Console.WriteLine( $"Heap element {heapEl}:" );
					ptIn.debugPrint();
				}
				else if ((showLeaf) && (pLeaf = kd as ptKdLeafNode) != null)
				{
					Console.WriteLine( $"Heap element {heapEl}:" );
					pLeaf.debugPrint();
				}
				heapEl++;
			}

			return;
		}

		//print the jensen heap we use for gathering photons
		public void jensenHeapPrint( bool showInt = true, bool showLeaf = true )
		{
			int heapEl = 1;
			foreach (Photon phot in jensenHeap)
			{
				Console.WriteLine( $" Jensen Photon {heapEl}" );
				heapEl++;
			}
			return;
		}

		//choose proper heap debug
		public String heapPrint()
		{
			if (this.pmHeap.Count > 0)
				return pmPrint();
			else if (this.jensenHeap.Count > 0)
				return jensenPrint();
			else
				return "Lol there's no heap here";
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
 
 */