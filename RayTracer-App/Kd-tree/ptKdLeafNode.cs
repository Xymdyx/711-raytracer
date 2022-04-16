/*
desc: kdTree leaf that stores photons
date started: 4/4
date due: 4/29
 */

using System;
using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.Photon_Mapping;
using RayTracer_App.World;

namespace RayTracer_App.Kd_tree
{
	public class ptKdLeafNode : KdNode
	{
		private Photon _stored;

		public Photon stored { get => this._stored; set => this._stored = value; }

		public ptKdLeafNode()
		{
			this._stored = null;
		}

		public ptKdLeafNode( Photon stored, float splitAxisVal )
		{
			this._stored = stored;
			stored.kdFlag = splitAxisVal;

		}

		public override string ToString()
		{
			String info = $"PM Kd-Leaf with {stored}: ";
			int objNum = 1;


			return info;
		}
	}
}
