using System;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.World;

namespace RayTracer_App.Kd_tree
{
	public class KdLeafNode : KdNode
	{
		private List<SceneObject> _objectPtrs;

		public List<SceneObject> objectPtrs { get => this._objectPtrs; set => this._objectPtrs = value; }

		public KdLeafNode()
		{
			this._objectPtrs = new List<SceneObject>();
		}

		public KdLeafNode( List<SceneObject> objectPtrs )
		{
			this._objectPtrs = objectPtrs;
		}

		//leaf intersection test
		public float leafIntersect( LightRay ray, World.World world, float minW = float.MinValue, float maxW = float.MaxValue, bool debug = true )
		{
			float bestW = world.findRayIntersect( ray, this.objectPtrs );// test all intersections with objects and return the closest;

			if (bestW >= minW && bestW <= maxW)
				return bestW;
			else if (bestW != float.MaxValue && debug) //debug
				Console.WriteLine( $"Failed to find intersection w kd" );

			return float.MaxValue; //not in bounds of the box;
		}
		
		//debug
		public override string ToString()
		{
			String info = $"Kd-Leaf with {objectPtrs.Count} objects: ";
			int objNum = 1;
			foreach( SceneObject obj in objectPtrs)
				info += $"Object {objNum}:" + obj.ToString() + "\t";

			return info;
		}
	}
}
