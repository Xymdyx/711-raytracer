using System;
using System.Numerics;
using System.Diagnostics;
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
			this.camTransformMat = Matrix4x4.Identity;
		}

// parameter constructor...
		public Camera( Vector up, Point eyePoint, Point lookAt )
		{
			this._up = up;
			this._eyePoint = eyePoint;
			this._lookAt = lookAt;
			this.camTransformMat = Matrix4x4.Identity;
		}

		//METHODS
		//http://www.songho.ca/opengl/gl_camera.html
		// https://github.com/sgorsten/linalg/issues/29 ... sanity checks that I am doing this correctly

		//gives the forward = target - camPos
		private Vector calculateN() { return lookAt - eyePoint; } 

		//gives the left = norm(cross(up, forward))
		private Vector calculateU(Vector forward) { return this.up.crossProduct( forward ); }

		//gives the y-axis (up-axis) = cross( forward, left)
		private Vector calculateV(Vector forward, Vector left) { return forward.crossProduct( left ); } //should be normalized now... 

		private void makeCamMat()
		{
			//use identity if world origin
			Matrix4x4 camCoordMat = Matrix4x4.Identity; //row major
		
			//https://www.geertarien.com/blog/2017/07/30/breakdown-of-the-lookAt-function-in-OpenGL/... from LHS

			Vector zAxis = calculateN(); // camera FORWARD direction
			Vector xAxis = calculateU( zAxis );
			Vector yAxis = calculateV( zAxis, xAxis );
			Vector eyeVec = eyePoint.toVec();

			/*	camCoordMat = new Matrix4x4( xAxis.v1, yAxis.v1, zAxis.v1, 0,
							xAxis.v2, yAxis.v2, zAxis.v2, 0,
							xAxis.v3, yAxis.v3, zAxis.v3, 0,
		-(eyeVec.dotProduct( xAxis)), -(eyeVec.dotProduct( yAxis)), -(eyeVec.dotProduct( zAxis)), 1);*/
			float eyeXDP = -(eyeVec.dotProduct( xAxis ));
			float eyeYDP = -(eyeVec.dotProduct( yAxis ));
			float eyeZDP = -(eyeVec.dotProduct( zAxis ));

			camCoordMat = new Matrix4x4
							( xAxis.v1, xAxis.v2, xAxis.v3,	eyeXDP,
							yAxis.v1, yAxis.v2, yAxis.v3, eyeYDP,
							zAxis.v1, zAxis.v2, zAxis.v3, eyeZDP,
							0, 0, 0, 1 );

			this.camTransformMat = camCoordMat;
			
		}

		//attempt at supersampling with only doing 4 basic corners of a pixel
		public Color superSamplePixel( Point centerPoint, float pixHeight, float pixWidth, World.World world )
		{
			Color averageHitColor = null;
			Color[] hitColors = new Color[4]; //for super-sampling
			Point[] hitPoints = new Point[4] {
				new Point( centerPoint.x - pixWidth / 2f, centerPoint.y + pixHeight / 2f, centerPoint.z), //top-left
				new Point( centerPoint.x + pixWidth / 2f, centerPoint.y + pixHeight / 2f, centerPoint.z ), //top-right
				new Point( centerPoint.x - pixWidth / 2f, centerPoint.y - pixHeight / 2f, centerPoint.z ), //bottom-left
				new Point( centerPoint.x + pixWidth /2f, centerPoint.y - pixHeight / 2f, centerPoint.z ), //bottom-right
			};
			
			for( int hitIdx = 0; hitIdx < 4; hitIdx ++ )
			{
				LightRay hitRay = new LightRay( hitPoints[hitIdx] - this.eyePoint, this.eyePoint );
				hitColors[hitIdx] = world.spawnRay( hitRay, 1 ); //cp5
			}

			//average the 4 colors
			for (int colNum = 0; colNum < 4; colNum++)
			{
				if (hitColors[colNum] != null)
				{
					if (averageHitColor == null)
						averageHitColor = hitColors[colNum];
					else
						averageHitColor += hitColors[colNum];
				}
			}

			if ( averageHitColor != null )
				averageHitColor = averageHitColor.scale( .25f );

			return averageHitColor;
		}

		// runs tone reproduction on the irradiance triplet retrieved from an intersection
		// just ternaries for now. If the sum of energy is >1, we max it at 1.
		public Color runTR( Color irradiance ) 
		{
			Color trColor = new Color( 0, 0, 0 );
			trColor.r = irradiance.r >= 1.0f ? trColor.r = 1.0f : trColor.r = irradiance.r;
			trColor.g = irradiance.g >= 1.0f ? trColor.g = 1.0f : trColor.g = irradiance.g;
			trColor.b = irradiance.b >= 1.0f ? trColor.b = 1.0f : trColor.b = irradiance.b;

			return trColor;
		}


//tried list of float[] and float[]...
		public byte[] render( World.World world, int imageHeight, int imageWidth, float focalLen, bool makeKd = false, bool doPM = false )
		{
			// this converts everything to camera coords
			makeCamMat();
			world.transformAll( this.camTransformMat );

			if (makeKd)
			{
				world.findBB(); //advanced cp 1
				world.buildKd();
			}

			else if(doPM)
			{
				world.findBB(); //advanced cp 2
				//world.beginPM();
			}

			//time the render here...
			Stopwatch renderTimer = new Stopwatch();
			renderTimer.Start();

			float fpHeight = 6f; //smaller the more zoomed in
			float fpWidth = fpHeight;

			//pixel info
			float pixHeight = fpHeight / imageHeight;
			float pixWidth = fpWidth / imageWidth;

			//initialize default background color at all pixels to begin with
			Color bgColor = Color.bgColor;
			byte[] bgArr = bgColor.asByteArr();
			byte[] pixColors = new byte[imageHeight * imageWidth * 3];

			// originally had (-fpHeight/2 + pixHeight/2.. was positive y upward...
			//focalLen + eyePoint.z if I want to move relative to my z
			Point fpPoint = new Point ( (-fpWidth / 2) + (pixWidth / 2), (fpHeight / 2) - (pixHeight / 2), focalLen); //gldrawPixels starts drawing lower-left corner at raster positions
			LightRay fire = new LightRay( fpPoint - this.eyePoint , this.eyePoint );
			Color hitColor = null;
			byte[] hitColorArr = null;
			bool isSuperSampling = false;

			int hits = 0;
			for ( int y = 0; y < imageHeight; y++) // positive x ->, positive y V
			{
				for ( int x = 0; x < imageWidth; x++)
				{
					//supersample branch here... have an array of hitcolors... average them then pass to TR below
					if (!isSuperSampling)
					{
						fire.direction = fpPoint - this.eyePoint;
						hitColor = world.spawnRay( fire, 1 ); //this will be irradiance.... CP5	
					}
					else
						hitColor = superSamplePixel( fpPoint, pixHeight, pixWidth, world );

					if (hitColor != null)
					{
						//run tone reproduction function on hitColor and then do the following
						hitColor = runTR( hitColor );
						hitColorArr = hitColor.asByteArr();
						int pos = (x + (y * imageWidth) ) * 3;
						pixColors[pos] = hitColorArr[0]; //try 0-1.0 floats instead of 255
						pixColors[pos + 1] = hitColorArr[1]; //try 0-1.0 floats instead of 255
						pixColors[pos + 2] = hitColorArr[2]; //try 0-1.0 floats instead of 255

						if( hitColor != Color.bgColor) hits++;
					}

					fpPoint.x += pixWidth;
				}
				//reset x to default position
				fpPoint.x = (-fpWidth / 2) + (pixWidth / 2);
				fpPoint.y -= pixHeight; // positive y is down
			}

			renderTimer.Stop();
			Console.WriteLine( "Rendering the scene took " + (renderTimer.ElapsedMilliseconds) + " milliseconds" );
			Console.WriteLine( $" There are {hits} non-background colors/ {imageHeight * imageWidth} colors total" );
			return pixColors ;
		}

	}
}
