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
		imageWidth = 1920;
		imageHeight = 1080;

		//initialize objects
		double s1Depth = 5.0;
		double s2Depth = 6.5;
		double floorDept = 40.0;
		List<Point> triVerts1 = new List<Point> { new Point( -50, 2, floorDept ), new Point( -50, -50, floorDept ), new Point( 50, 2, floorDept ) }; //this must be fixed, goofs up in direction of 3nd point
		//List<Point> triVerts2 = new List<Point> { new Point( 3, -1, floorDept ), new Point( 3, 0, floorDept ), new Point( 2, -1, floorDept ) }; //this must be fixed, goofs up in direction of 3nd point
		List<Point> triVerts2 = new List<Point> { new Point( -50, -50, floorDept ), new Point( 50, -50, floorDept ), new Point( 50, 2, floorDept ) } ; //this must be fixed, goofs up in direction of 3nd point

		Polygon triangle1 = new Polygon( triVerts1 );
		Polygon triangle2 = new Polygon( triVerts2 );

		Sphere sphere1 = new Sphere( new Point( 0, 1.5, s1Depth) ,2.5 );
		Sphere sphere2 = new Sphere( new Point( 3.5, 1.75, s2Depth ), 2.0 );

		World world = new World();
		world.add( triangle1 );
		world.add( triangle2 );
		world.add( sphere1 );
		world.add( sphere2 );

		// initialize camera and render world
		Camera cam = new Camera( new Vector( 0, 1, 0 ), new Point( 0, 0.0, 0.0 ), new Point( 0, 0, 5.0 ) );

		// ditto with floats from 0-1 and 0-255, uint, now try byte
		byte[] pixColors = cam.render( world, imageHeight, imageWidth );

		unsafe
		{
			fixed (byte* colArrPtr = pixColors) { colsPtr = new IntPtr( (void*)colArrPtr ); }
		}

		return;

	}
	public static void display()
	{
		//put in display function
		GL.ClearColor( 0, 0, 0, 1 );
		GL.Clear( GL.GL_COLOR_BUFFER_BIT );

		GL.PixelStorei( GL.GL_UNPACK_ALIGNMENT, 1);
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		GL.GetFloatv( GL.GL_CURRENT_RASTER_POSITION, rat );
		Console.WriteLine( $" {rat[0]} , {rat[1]}, {rat[2]} is valid: {valid[0]}" ); //print out default { 0,0,0}"
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
		FG.InitDisplayMode( GLUT.GLUT_RGB | GLUT.GLUT_SINGLE | GLUT.GLUT_DEPTH);
		FG.InitWindowSize( 1920, 1080 );
		FG.InitWindowPosition( 0, 0 );
		FG.CreateWindow( "RayTracing CheckPoint 2" );
		GL.Init( true );			//I forgot to call this...
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		FG.DisplayFunc( display ); //white screen without this
		FG.MainLoop(); //end
		return 0;
	}
}