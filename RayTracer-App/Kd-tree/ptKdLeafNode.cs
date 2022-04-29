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

		public float leafIntersect( LightRay ray, float minW = float.MinValue, float maxW = float.MaxValue, bool debug = false )
		{
			float bestW = this.stored.rayPhotonIntersect( ray );
			if (bestW >= minW && bestW <= maxW)
				return bestW;
			else if( bestW != float.MaxValue && debug) //debug
				Console.WriteLine( $"Rejected photon with distance {bestW} betw {minW} & {maxW}" );

			return float.MaxValue; //not in bounds of the box;
		}

		public void debugPrint()
		{
			Console.WriteLine($"PM Kd-Leaf with {stored} on prevAxis {stored.kdFlag} ");
		}

		public override string ToString()
		{
			String info = $"PM Kd-Leaf with {stored} ";
			return info;
		}
	}
}
