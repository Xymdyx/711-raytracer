/*
 * author: stf8464
 * desc: class that represents an axially-aligned bounding box to contain an object
 * date : 3/10/22
 * https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/aabb.html
*/

using System;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;

namespace RayTracer_App.Voxels
{
	public class AABB : Voxel
	{
		// https://computergraphics.stackexchange.com/questions/6064/aabb-bounding-boxes
		//need to compute after camera transform

		private int _axis;
		private float _tNear; // dist where ray enters box
		private float _tFar; //dist where ray exits box
		private Point _center;
		private Vector _extents;
		public int axis { get => this._axis; set => this._axis = value; }
		public float tNear { get => this._tNear; set => this._tNear = value; }
		public float tFar { get => this._tFar; set => this._tFar = value; }

		public Point center { get => this._center; set => this._center = value; }
		public Vector extents { get => this._extents; set => this._extents = value; }

		//get center point
		public void findCenter()
		{
			Vector vMax = this.max.toVec();
			this._center =  (this.min + vMax) * .5f;
		}

		//find distance vector from center to two opposite corners
		public void findExtents()
		{
			if( center != null)
			{
				Vector extents = this.center.ptSub(this.min);
				Vector extents2 = this.max.ptSub( this.center );

				if (extents != extents2)
					//Console.WriteLine( "Error in find extents" );

				extents.v1 = Math.Abs( extents.v1 );
				extents.v2 = Math.Abs( extents.v2 );
				extents.v3 = Math.Abs( extents.v3 );
				this._extents = extents;
			}
			return;
		}

		public AABB()
		{
			this._shape = 0; //for box
			this._max = null;
			this._min = null;
			this.axis = 0;
			this._center = null;
			this._extents = null;
		}

		public AABB( Point p1, Point p2, int axis ) //given two opposite vertices of the AABB, calculate the minimum and Max points
		{
			this._shape = 0;
			this._max = new Point( Math.Max(p1.x, p2.x), Math.Max(p1.y, p2.y), Math.Max(p1.z, p2.z) );
			this._min = new Point( Math.Min( p1.x, p2.x ), Math.Min( p1.y, p2.y ), Math.Min( p1.z, p2.z ) ); //this allows us to keep track of the true minimum and maxes
			this.axis = axis;
			findCenter();
			findExtents();
		}


		// intersects with ray
		public override bool intersect( LightRay ray )
		{
		//https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection

			//if ray's parallel, it won't intersect...These wouldn't have solns
			float minT = (this.min.x - ray.origin.x) / ray.direction.v1;
			float maxT = (this.max.x - ray.origin.x) / ray.direction.v1;
			float minY = (this.min.y - ray.origin.y) / ray.direction.v2;
			float maxY = (this.max.y - ray.origin.y) / ray.direction.v2;
			float minZ = (this.min.z - ray.origin.z) / ray.direction.v3;
			float maxZ = (this.max.z - ray.origin.z) / ray.direction.v3;

			//must have vals be true to name
			if (minT > maxT)
				(minT, maxT) = (maxT, minT); // tuples let me swap variables w/o temps

			if (minY > maxY)
				(minY, maxY) = (maxY, minY);

			if (minZ > maxZ)
				(minZ, maxZ) = (maxZ, minZ);

			Point bEntry = new Point( minT, minY, minZ ); //make these now in case we pass later
			Point bExit = new Point( maxT, maxY, maxZ );

			ray.clearObjPts();
			
			//we don't hit the 2d box on the xy plane
			if ((minT > maxY) || (minY > maxT)) 
				return false;

			//update for test below on xz plane
			if (minY > minT)
				minT = minY;

			if (maxY < maxT)
				maxT = maxY;

			//we don't hit the 2d box on the xz plane
			if ((minT > maxZ) || (minZ > maxT)) 
				return false;

			//update for posterity
			if (minZ > minT)
				minT = minZ;

			if (maxZ < maxT)
				maxT = maxZ;

			//so the ray knows where it went for later
			//if (minT > maxT)
			//(minT, maxT) = (maxT, minT); // tuples let me swap variables w/o temps

			if (minT > maxT)
				return false;
			this.tNear = minT;
			this.tFar = maxT;

			ray.entryPt = ray.findPtAlong( minT ) ;
			ray.exitPt = ray.findPtAlong( maxT );

		//	if( ray.origin.distance( ray.entryPt) > ray.origin.distance( ray.exitPt) )
		//		(ray.entryPt, ray.exitPt) = (ray.exitPt, ray.entryPt); // tuples let me swap variables w/o temps

			return true;
		}


		public bool intersectAABB( AABB box )
		{
			/*
			for each axis x, y, z
		if (Aaxis.min > Baxis.max) or
		(Baxis.min > Aaxis.max)
			return FALSE
		return TRUE;
		*/
			for (int axis = 0; axis < 3; axis++)
			{
				if ( (this.min.getAxisCoord( axis ) > box.max.getAxisCoord( axis )) ||
					(box.min.getAxisCoord( axis ) > this.max.getAxisCoord( axis )) )
					return false;
			}

				return true;
		}

		//from slide deck on kd-tree implementation
		// try this one too: https://stackoverflow.com/questions/4578967/cube-sphere-intersection-test
		public bool sphereIntersect( Point center, float radius )
		{
			float d = 0;
			float e;

			for (int axis = 0; axis < 3; axis++)
			{
				
				if ((e = center.getAxisCoord( axis ) - this.min.getAxisCoord( axis )) < 0)
				{
					if (e < -radius) return false;
					d += (e * e);
				}
				else if ( (e = center.getAxisCoord( axis ) - this.max.getAxisCoord( axis )) > 0)
				{
					if (e > radius) return false;
					d += (e * e);
				}
			}

			if (d > (radius * radius))
				return false;
		
			return true;
		}

		//helper for doing a Separating Axis Theorem Test for a given test Axis
		private bool testSAT( Vector v0, Vector v1, Vector v2,
			Vector u0, Vector u1, Vector u2, Vector bExts, Vector tAxis )
		{
			// Testing axis: axis_u0_f0
			// Project all 3 vertices of the triangle onto the Seperating axis
			float p0 = v0.dotProduct( tAxis );
			float p1 = v1.dotProduct( tAxis );
			float p2 = v2.dotProduct( tAxis );
			// Project the AABB onto the seperating axis
			// We don't care about the end points of the prjection
			// just the length of the half-size of the AABB
			// That is, we're only casting the extents onto the 
			// seperating axis, not the AABB center. We don't
			// need to cast the center, because we know that the
			// aabb is at origin compared to the triangle!
			float r = bExts.v1 * Math.Abs( u0.dotProduct( tAxis ) ) +
						bExts.v2 * Math.Abs( u1.dotProduct( tAxis ) ) +
						bExts.v3 * Math.Abs( u2.dotProduct( tAxis ) );
			// Now do the actual test, basically see if either of
			// the most extreme of the triangle points intersects r
			// You might need to write Min & Max functions that take 3 arguments
			if (Math.Max( -Math.Max( Math.Max( p0, p1 ), p2 ), Math.Min( Math.Min( p0, p1 ), p2 ) ) > r)
			{
				// This means BOTH of the points of the projected triangle
				// are outside the projected half-length of the AABB
				// Therefore the axis is seperating and we can exit
				return false;
			}

			return true;
		}

		// 13 axis triangle tests
		public bool triangleIntersect( Scene_Objects.Polygon triangle ) 
		{
			// https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/aabb-triangle.html
			Vector v0 = triangle.vertices[0].toVec();
			Vector v1 = triangle.vertices[1].toVec();
			Vector v2 = triangle.vertices[2].toVec();

			Vector bCenter = this.center.toVec();
			Vector bExts = this.extents;

			//translate triangle as conceptually moving aabb to origin
			// Translate the triangle as conceptually moving the AABB to origin
			// This is the same as we did with the point in triangle test
			v0 = v0.subVec( bCenter );
			v1 = v1.subVec( bCenter );
			v2 = v2.subVec( bCenter );


			// Compute the edge vectors of the triangle  (ABC)
			// That is, get the lines between the points as vectors
			Vector f0 = v1.subVec( v0 ); // B - A
			Vector f1 = v2.subVec( v1 ); // C - B
			Vector f2 = v0.subVec( v2 ); // A - C

			// Compute the face normals of the AABB, because the AABB
			// is at center, and of course axis aligned, we know that 
			// it's normals are the X, Y and Z axis.
			Vector u0 = new Vector( 1.0f, 0.0f, 0.0f );
			Vector u1 = new Vector( 0.0f, 1.0f, 0.0f );
			Vector u2 = new Vector( 0.0f, 0.0f, 1.0f );

			// There are a total of 13 axis to test!

			// We first test against 9 axis, these axis are given by
			// cross product combinations of the edges of the triangle
			// and the edges of the AABB. You need to get an axis testing
			// each of the 3 sides of the AABB against each of the 3 sides
			// of the triangle. The result is 9 axis of seperation
			// https://awwapp.com/b/umzoc8tiv/

			// Compute the 9 axis
			Vector axis_u0_f0 = u0.crossProduct( f0 );
			Vector axis_u0_f1 = u0.crossProduct( f1 );
			Vector axis_u0_f2 = u0.crossProduct( f2 );

			Vector axis_u1_f0 = u1.crossProduct( f0 );
			Vector axis_u1_f1 = u1.crossProduct( f1 );
			Vector axis_u1_f2 = u1.crossProduct( f2 );

			Vector axis_u2_f0 = u2.crossProduct( f0 );
			Vector axis_u2_f1 = u2.crossProduct( f1 );
			Vector axis_u2_f2 = u2.crossProduct( f2 );

			//the 9 separating axes
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u0_f0 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u0_f1 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u0_f2 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u1_f0 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u1_f1 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u1_f2 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u2_f0 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u2_f1 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, axis_u2_f2 ) ) return false;

			//the 3 AABB normals
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, u0 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, u1 ) ) return false;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, u2 ) ) return false;


			// the face normal of the triangle
			Vector triangleNormal = f0.crossProduct( f1 ) ;
			if ( !testSAT( v0, v1, v2, u0, u1, u2, bExts, triangleNormal ) ) return false;

			// passed all 13 SATs, therefore the triangle intersects this AABB
			return true;
		}

		// create a box from a list of points by finding max and min extents
		public static AABB boxAroundPoints( List<Point> points )
		{
			float[] max = new float[3];
			float[] min = new float[3];

			foreach (Point pt in points)
			{
				//compare point values to max and min so far
				for (int axis = 0; axis < 3; axis++)
				{
					float axisVal = pt.getAxisCoord( axis );
					//update scene min and max points if applicable
					if (axisVal > max[axis])
						max[axis] = axisVal;
					if (axisVal < min[axis])
						min[axis] = axisVal;
				}

			}

			//make box around points
			Point minPt = new Point( min[0], min[1], min[2] );
			Point maxPt = new Point( max[0], max[1], max[2] );

			return new AABB( minPt, maxPt, -1 );	
		}

		//sanity check
		public bool ptInBox( Point pt )
		{
			if ((pt.x <= this.max.x && pt.x >= this.min.x)
			&& (pt.y <= this.max.y && pt.y >= this.min.y)
			&& (pt.z <= this.max.z && pt.z >= this.min.z))
			{
				return true;
			}

			Console.WriteLine( $"{pt} not inside box with max {this.max}, min {this.min} " );
			return false;
		}
	}
}

/* to split an AABB ( the biggest points on the cube we find):
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