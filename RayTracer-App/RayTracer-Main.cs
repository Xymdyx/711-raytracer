using System;
using OpenGLDotNet;
using OpenGLDotNet; // for draw pixels
using System.Runtime.InteropServices; //for GCHandle
using RayTracer_App.World;
using RayTracer_App.Camera;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;

public class RayTracerMain
{
	static int imageWidth;
	static int imageHeight;
	static IntPtr colsPtr;
	//static GCHandle colsHandle;
	static bool[] valid = new bool[1];
	static float[] rat = new float[4];


	public static void doRayTracing() 
	{
		// drawPixels in the RGB array with glDrawPixels();... put this in main
		imageWidth = 500;
		imageHeight = 500;

		//initialize objects
		//List<Point> triVerts = new List<Point> { new Point( 1, 0, 0 ), new Point( 1, 1, 0 ), new Point( 0, 1, 0 ) };
		//Polygon triangle = new Polygon( triVerts );
		Sphere sphere = new Sphere( new Point( 0, 0, 0) , 1.0 );

		World world = new World();
		//world.add( triangle );
		world.add( sphere );

		// initialize camera and render world
		Camera cam = new Camera( new Vector( 0, 1, 0 ), new Point( 0, 0.0, 3.0 ), new Point( 0, 0, 0 ) );

		//List<float[]> pixColors = cam.render( world, imageHeight, imageWidth );
		// ditto with floats from 0-1 and 0-255
		uint[] pixColors = cam.render( world, imageHeight, imageWidth );

		unsafe
		{
			fixed (uint* colArrPtr = pixColors) { colsPtr = new IntPtr( (void*)colArrPtr ); }
		}

		//colsHandle = GCHandle.Alloc( pixColors );
		//colsPtr = GCHandle.ToIntPtr( colsHandle );
		return;
	}
	public static void display()
	{
		//put in display function
		GL.ClearColor( 255, 0, 0, 1 );
		GL.Clear( GL.GL_COLOR_BUFFER_BIT );

		//GL.PixelStoref( GL.GL_UNPACK_ALIGNMENT, GL.GL_INT );
		//GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		//GL.GetFloatv( GL.GL_CURRENT_RASTER_POSITION, rat );
		//Console.WriteLine( $" {rat[0]} , {rat[1]}, {rat[2]} " ); //print out default { 0,0,0}"
		GL.DrawPixels( imageWidth, imageHeight, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, colsPtr );

		FG.SwapBuffers();
		return;
	}

	static int Main( string [] args )
	{
		//	//camera lookat(position, center, up): [ 0.0, 2.0, -7.2], [0, -1.5, 0], [0, 1, 0]
		//	//camera perspective projection(vertical fov, aspect ratio, near, far)): [radians(90.0), 1.0, 1.0, 300.0]

		//	//foreground - sphere size:[5,5,5]
		//	//foreground - sphere location:[1,1.0, -1.0]

		//	//background - sphere size:[5,5,5]
		//	//background - sphere location:[-2.5, -1.75 , 2.0]

		//	//floor - cube size:[25, 2.5, 60]
		//	//floor - cube location:[ -2.0, -6.5, -0.5]
		doRayTracing();

		//begin OpenGl
		int[] argc = new int[1]; argc[0] = 0; string[] argv = null;
		FG.Init( argc, argv );
		FG.InitDisplayMode( GLUT.GLUT_RGB | GLUT.GLUT_DOUBLE );
		FG.InitWindowSize( imageWidth, imageHeight );
		FG.InitWindowPosition( 0, 0 );
		FG.CreateWindow( "RayTracing CheckPoint 2" );
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		FG.DisplayFunc( display ); //white screen without this
		FG.MainLoop(); //end
		return 0;
	}
}