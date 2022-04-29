using System;
using System.Collections.Generic;
using RayTracer_App.World;
using RayTracer_App.Voxels;
using RayTracer_App.Scene_Objects;

namespace RayTracer_App.Kd_tree
{
	public class KdTree
	{
		//fields
		private KdNode _root;

		private int _maxLeafObjs;

		private static int MAX_KD_DEPTH = 999;
		public KdNode root { get => this._root; set => this._root = value; }
		public int maxLeafObjs { get => this._maxLeafObjs; set => this._maxLeafObjs = value; }

		// constructors
		public KdTree() 
		{
			this._root =  null;
			this._maxLeafObjs = 2;
		}

		public KdTree( KdNode root )
		{
			this._root = root;
			this._maxLeafObjs = 2;
		}

		//methods

		//helper for finding Vector to translate points along split plane for an AABB
		// currently zeroes out the splitting axis for shifting the center instead... likely where the bug is
		private Vector findSplitVec( AABB vox, int axis )
		{
			Vector splitVec = vox.extents.copy();
			switch (axis)
			{
				case 0:
					splitVec.v2 = 0;
					splitVec.v3 = 0;
					break;
				case 1:
					splitVec.v1 = 0;
					splitVec.v3 = 0;

					break;
				case 2:
					splitVec.v1 = 0;
					splitVec.v2 = 0;

					break;
			}

			return splitVec;
		}

		public bool terminal( List<SceneObject> objects, AABB vox, int depth )
		{
			return ( objects.Count <= maxLeafObjs || depth >= MAX_KD_DEPTH ); //the spheres are in the same voxel...
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

			//AABB vFront = new AABB( vox.max, vox.center - splitVec , axis );
			//AABB vRear = new AABB( vox.min, vox.center + splitVec, axis );
			AABB vFront = new AABB( vox.max, vox.min + splitVec, axis );
			AABB vRear = new AABB( vox.min, vox.max - splitVec, axis );
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

			return new KdInteriorNode( axis, partitionVal, vox,
				getNode(frontObjs, vFront, depth + 1), getNode( rearObjs, vRear, depth + 1 ) );
		}

		////helper for traverseTAB
		//private Point splitPlaneRayIntersection( LightRay ray, Point center, Vector sVec )
		//{
		//	//this is the plane defined by a fixed axis value on a major axis. The other two points can have different values that ultimately evaluate to normal vector of 1 in the appropriate axis

		//	//https://web.ma.utexas.edu/users/m408m/Display12-5-4.shtml
		//	//https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
		//	return new Point();
		//}
		
		private bool intersectGood( float currW )
		{
			if (currW < 0)
				Console.WriteLine( " Negative distance evaluated in kdTree intersect..." );

			return (currW != float.MinValue) && (currW != float.NaN) &&
					(currW != float.MaxValue); //should this intersection be negative?
		}

		// https://slideplayer.com/slide/4991637/
		//https://dcgi.fel.cvut.cz/home/havran/DISSVH/dissvh.pdf ... C psuedocode on p. 171. It's good reference for iterative implementation. typo in thesis orginally "bCoord == splitOffset" Z1
		//https://github.com/dragonbook/mitsuba-ctcloth/blob/master/include/mitsuba/render/sahkdtree2.h
		// if you do read through my code, I strongly recommend adding this to the kdSlides since I misunderstood this algorithm immensely. It's a faster alternative approach
		public float travelTAB( LightRay ray, World.World world, bool debug = false )
		{
			//WORKS...photon shooting is jank
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

			KdInteriorNode inner = currNode as KdInteriorNode;

			if (inner == null)
				return bestW;

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
				KdLeafNode leaf;
				while ((leaf = currNode as KdLeafNode) == null)
				{
					depth++;
					KdInteriorNode currInner = currNode as KdInteriorNode;
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
				bestW = leaf.leafIntersect( ray, world );// test ray photon intersection... turning off the distance checks did result in more visible photons in map

				reached++;
				if (intersectGood( bestW ))
				{   //found it, stop
					if (debug)
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

// a given ray traverses the tree and gets the closest intersection
//public float travelTAB( LightRay ray, World.World world, KdNode node = null )
//{
//	// a[coord] = proper coord of entry pt
//	// b[coord] = proper coord of exit point
//	// s = splitting plane offset... from splitting-plane ray intersection
//	//ray must intersect whole scene bounding box prior
//	Point entryPt;
//	Point exitPt;
//	Point sPlanePt;
//	Vector sVec;
//	float aCoord;
//	float bCoord;
//	float splitOffset;

//	float bestW = float.MaxValue; //initialize to error per convention so far

//	if (node == null)
//		node = this.root;

//	KdLeafNode leaf = node as KdLeafNode;
//	KdInteriorNode inner = node as KdInteriorNode;

//	if (leaf != null)
//		return world.findRayIntersect( ray, leaf.objectPtrs );// test all intersections with objects and return the closest;

//	// N = negative, P = positive, Z = along splitting plane
//	else if (inner != null)
//	{
//		inner.selfAABB.intersect( ray ); // update entry and exit points

//		if (ray.entryPt == null || ray.exitPt == null)
//			return bestW;

//		entryPt = ray.entryPt.copy();
//		exitPt = ray.exitPt.copy();
//		aCoord = entryPt.getAxisCoord( inner.axis );
//		bCoord = exitPt.getAxisCoord( inner.axis );
//		splitOffset = inner.axisVal;

//		if (aCoord <= splitOffset)
//		{
//			if (bCoord < splitOffset)
//				bestW = travelTAB( ray, world, inner.rear ); //N1, N2, N3, P5, Z3
//			else
//			{
//				if (bCoord == splitOffset)
//					bestW = travelTAB( ray, world, inner.front ); //traverse arbitrary child node.. Z2
//				else
//				{
//					//compute and store location of splitOffset....I believe this is already done(?)
//					bestW = travelTAB( ray, world, inner.rear );

//					if (intersectGood( bestW )) return bestW; //return here since we know first check will def be closer

//					bestW = travelTAB( ray, world, inner.front ); //N4
//				}
//			}
//		}
//		else // aCoord > splitOffset
//		{
//			if (bCoord > splitOffset)
//				bestW = travelTAB( ray, world, inner.front ); // P1, P2, P3, N5, Z1
//			else
//			{
//				//compute and store location of splitOffset....I believe this is already done(?)
//				bestW = travelTAB( ray, world, inner.front );

//				if (intersectGood( bestW )) return bestW; //return here since we know first check will def be closer

//				bestW = travelTAB( ray, world, inner.rear ); //P4
//			}
//		}


//	}

//	return bestW; //error
//}