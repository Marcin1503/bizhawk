using System;
using BizHawk.Bizware.BizwareGL;


namespace BizHawk.Client.Common
{
	/// <summary>
	/// This singleton class manages OpenGL contexts, in an effort to minimize context changes.
	/// </summary>
	public class GLManager
	{
		private GLManager()
		{

		}

		public static GLManager Instance { get; private set; }

		public static void CreateInstance()
		{
			if (Instance != null) throw new InvalidOperationException("Attempt to create more than one GLManager");
			Instance = new GLManager();
		}

		public ContextRef CreateGLContext()
		{
			var ret = new ContextRef
			{
				gl = new Bizware.BizwareGL.Drivers.OpenTK.IGL_TK()
			};
			return ret;
		}

		public ContextRef GetContextForGraphicsControl(GraphicsControl gc)
		{
			return new ContextRef
			{
				gc = gc
			};
		}

		/// <summary>
		/// This might not be a GL implementation. If it isnt GL, then setting it as active context is just NOP
		/// </summary>
		public ContextRef GetContextForIGL(IGL gl)
		{
			return new ContextRef
			{
				gl = gl
			};
		}

		ContextRef ActiveContext;

		public void Invalidate()
		{
			ActiveContext = null;
		}

		public void Activate(ContextRef cr)
		{
			if (cr == ActiveContext)
				return;
			ActiveContext = cr;
			if (cr.gc != null)
			{
				//TODO - this is checking the current context inside to avoid an extra NOP context change. make this optional or remove it, since we're tracking it here
				 cr.gc.Begin();
			}
			if (cr.gl != null)
			{
				if(cr.gl is BizHawk.Bizware.BizwareGL.Drivers.OpenTK.IGL_TK)
					((BizHawk.Bizware.BizwareGL.Drivers.OpenTK.IGL_TK)cr.gl).MakeDefaultCurrent();
			}
		}

		public void Deactivate()
		{
			//this is here for future use and tracking purposes.. however.. instead of relying on this, we should just make sure we always activate what we need before we use it
		}

		public class ContextRef
		{
			public IGL gl;
			public GraphicsControl gc;
		}
	}
}