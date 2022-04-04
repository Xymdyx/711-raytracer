/*
desc: kdTree leaf that stores photons
date started: 4/4
date due: 4/29
 */

using System;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.World;

namespace RayTracer_App.Kd_tree
{
	public class ptKdLeafNode : KdNode
	{
		private List<SceneObject> _objectPtrs;

		public List<SceneObject> objectPtrs { get => this._objectPtrs; set => this._objectPtrs = value; }

		public ptKdLeafNode()
		{
			this._objectPtrs = new List<SceneObject>();
		}

		public ptKdLeafNode( List<SceneObject> objectPtrs )
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
