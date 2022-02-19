using System;
using System.Numerics;
using RayTracer_App.World;

//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.Camera
{
	public class Camera
	{
		//field-plane
		private Vector _up;
		//eyepoint
		private Point _eyePoint;
		//lookat
		private Point _lookAt;
		//cameraTransform
		private Matrix4x4 _camTransformMat;


		public Vector up { get => this._up; set => this._up = value; } 
		public Point eyePoint { get => this._eyePoint; set => this._eyePoint = value; }
		public Point lookAt { get => this._lookAt; set => this._lookAt = value; }
		public Matrix4x4 camTransformMat { get => this._camTransformMat; set => this._camTransformMat = value; }



		//default constructor... TODO define world origin as default
		public Camera()
		{
			this._up = new Vector( 0, 1, 0 );
			this._eyePoint = new Point( 0, 0, 0 ); // world origin is the default
			this._lookAt = new Point( 0, 0, 10 ); // default lookat position
		}

// parameter constructor...
		public Camera( Vector up, Point eyePoint, Point lookAt )
		{
			this._up = up;
			this._eyePoint = eyePoint;
			this._lookAt = lookAt;
		}

//METHODS
		//http://www.songho.ca/opengl/gl_camera.html
		// https://github.com/sgorsten/linalg/issues/29 ... sanity checks that I am doing this correctly

		//TODO DOUBLE-CHECK WHAT HAPPENS IN THE CAMERA AND VERYIFY MS 4X4MATRIX

		//gives the forward = target - camPos
		private Vector calculateN() { return lookAt - eyePoint; } 

		//gives the left = norm(cross(up, forward))
		private Vector calculateU(Vector forward) { return this.up.crossProduct( forward ); }

		//gives the y-axis (up-axis) = cross( forward, left)
		private Vector calculateV(Vector forward, Vector left) { return forward.crossProduct( left ); } //should be normalized now... 

		private void makeCamMat() //TODO FIZX THIS TO CONFORM WITH LHS
		{
			//use identity if world origin
			Matrix4x4 camCoordMat = Matrix4x4.Identity; //row major
		
			//https://www.geertarien.com/blog/2017/07/30/breakdown-of-the-lookAt-function-in-OpenGL/... from LHS i

			Vector zAxis = calculateN(); // camera FORWARD direction
			Vector xAxis = calculateU( zAxis );
			Vector yAxis = calculateV( zAxis, xAxis );
			Vector eyeVec = eyePoint.toVec();

			/*	camCoordMat = new Matrix4x4( xAxis.v1, yAxis.v1, zAxis.v1, 0,
							xAxis.v2, yAxis.v2, zAxis.v2, 0,
							xAxis.v3, yAxis.v3, zAxis.v3, 0,
		-(eyeVec.dotProduct( xAxis)), -(eyeVec.dotProduct( yAxis)), -(eyeVec.dotProduct( zAxis)), 1);*/

			camCoordMat = new Matrix4x4
							( xAxis.v1, xAxis.v2, xAxis.v3,	-(eyeVec.dotProduct( xAxis )),
							yAxis.v1, yAxis.v2, yAxis.v3, -(eyeVec.dotProduct( yAxis )),
							zAxis.v1, zAxis.v2, zAxis.v3, -(eyeVec.dotProduct( zAxis )),
							0, 0, 0, 1 );

			this.camTransformMat = camCoordMat;
			
		}


//tried list of float[] and float[]...
		public byte[] render( World.World world, int imageHeight, int imageWidth, float focalLen )
		{
			// this converts everything to camera coords
			makeCamMat();
			world.transformAll( camTransformMat );

			float fpHeight = 6f; //smaller the more zoomed in
			float fpWidth = fpHeight;

			//pixel info
			float pixHeight = fpHeight / imageHeight;
			float pixWidth = fpWidth / imageWidth;

			//initialize default background color at all pixels to begin with
			Color bgColor = new Color();
			byte[] bgArr = bgColor.asByteArr();
			byte[] pixColors = new byte[imageHeight * imageWidth * 3];


			int modIdx = 0;
			for (int add = 0; add < imageHeight * imageWidth * 3 ; add++)
			{
				modIdx = add % 3;

				if (modIdx == 0) pixColors[add] = bgArr[0]; //red
				else if (modIdx == 1) pixColors[add] = bgArr[1]; //green
				else pixColors[add] = bgArr[2]; //blue
			}

			// originally had (-fpHeight/2 + pixHeight/2.. was positive y upward...
			Point fpPoint = new Point ( (-fpWidth / 2) + (pixWidth / 2), (fpHeight / 2) - (pixHeight / 2), focalLen); //gldrawPixels starts drawing lower-left corner at raster positions
			LightRay fire = new LightRay( fpPoint - this.eyePoint , this.eyePoint );
			Color hitColor = null;
			byte[] hitColorArr = null;

			// for x = 0; x < x pixels; x+= pixelwidth
			//	for y = 0; y < y-pixels; y-= pixelHeight
			//		world.spawnRay()... see what it hits
			//		whatever it hits... rgbs.add( rgb float triplet)
			//start top-left -> bottom-right
			int hits = 0;
			for ( int y = 0; y < imageHeight; y++) // positive x ->, positive y V
			{
				for ( int x = 0; x < imageWidth; x++)
				{
					fire.direction = fpPoint - this.eyePoint;
					hitColor = world.spawnRay( fire );

					if (hitColor != null) // I assume some distortion happens since I do not check if intersection happens beyond film plane
					{
						hitColorArr = hitColor.asByteArr();
						int pos = (x + (y * imageWidth) ) * 3;
						pixColors[pos] = hitColorArr[0]; //try 0-1.0 floats instead of 255
						pixColors[pos + 1] = hitColorArr[1]; //try 0-1.0 floats instead of 255
						pixColors[pos + 2] = hitColorArr[2]; //try 0-1.0 floats instead of 255
						hits++;
					}

					fpPoint.x += pixWidth;
				}
				//reset x to default position
				fpPoint.x = (-fpWidth / 2) + (pixWidth / 2);
				fpPoint.y -= pixHeight; // positive y is down
			}

			Console.WriteLine( $" There are {hits} non-background colors/ {imageHeight * imageWidth} colors total" );
			return pixColors ;
		}

	}
}
