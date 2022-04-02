using System;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
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
