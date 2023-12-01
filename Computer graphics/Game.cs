using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Computer_graphics
{
    internal class Game : GameWindow
    {
        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // Handles to OpenGL objects are integers representing the object's location on the graphics card.
        // Similar to pointers, they are used in OpenGL functions that require them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;

        private int _vertexArrayObject;

        // This class is a wrapper around a shader, which helps us manage it.
        private Shader _shader;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            // Set the background color after clearing in normalized colors, ranging from 0.0 (black) to 1.0 (maximum channel value).
            // This color is deep green.
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Transmit vertices to the graphics card for OpenGL use by creating a Vertex Buffer Object (VBO).
            // VBOs efficiently upload data to a buffer, facilitating simultaneous transmission of all vertices.

            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.
            _vertexBufferObject = GL.GenBuffer();

            // Bind the buffer in OpenGL to make it the active state for VBO modifications.
            // Subsequent calls affecting the VBO will apply to this buffer until another is bound.
            // Use the enum ArrayBuffer for the buffer type (VBO) and the buffer handle as the second argument.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Upload vertices to the buffer with details on the buffer, data size, the vertices, and the usage pattern.
            // BufferUsageHints include StaticDraw (rare updates), DynamicDraw (frequent updates), and StreamDraw (updates every frame).
            // Ensure the right usage for your specific case, typically StaticDraw is preferred.
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // The buffer lacks structure; it's just a collection of floats (essentially bytes).
            // OpenGL introduces Vertex Array Object (VAO) to manage data interpretation and division into vertices.
            // In this example, we set up the VAO to interpret 12 bytes as 3 floats, dividing the buffer into vertices.
            // Generate and bind a VAO for this purpose. Note that creating and binding a VAO, despite similarities, differs from VBO.
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Configure the vertex shader interpretation of VBO data. Various C datatypes (and a few non-C ones) can be sent,
            // requiring explicit mapping to the shader's input variables for flexibility.

            // Use GL.VertexAttribPointer to inform OpenGL about data format and associate the current array buffer with the VAO.
            // Specify location (0), elements per vertex (3 floats), data type (float), no normalization, stride (3 * sizeof(float)),
            // and offset (0). Stride and offset details will be covered more when discussing texture coordinates.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);

            // In modern OpenGL, vertex-to-pixel transformation offers flexibility,
            // achieved through two additional programs called shaders.
            // These tiny GPU-residing programs handle the conversion.
            // Explore the Shader class in Common for shader creation and an in-depth understanding.
            // Check shader.vert and shader.frag for actual shader code.
            _shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");

            // Now, enable the shader.
            // Just like the VBO, this is global, so every function that uses a shader will modify this one until a new one is bound instead.
            _shader.Use();

            // Setup is now complete! Now we move to the OnRenderFrame function to finally draw the triangle.
        }

        // Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Clear the image using GL.ClearColor, targeting ColorBufferBit for clearing the color buffer in OpenGL.
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw an object in OpenGL by binding the shader, setting uniforms (future tutorial),
            // binding the VAO, and calling a rendering function.

            // Bind the shader
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // Call our drawing function using GL.DrawArrays for rendering a simple triangle
            // with primitive type Triangles, starting index 0, and drawing 3 vertices.
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


            // OpenTK windows use double buffering to prevent screen tearing.
            // Call this function after drawing to swap the buffers and display the rendered content.
            SwapBuffers();

            // And that's all you have to do for rendering! You should now see a yellow triangle on a black screen.
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, Size.X, Size.Y);
        }


        // OpenGL resource cleanup is typically unnecessary during application exit;
        // the driver and OS handle it. Delete resources when needed, e.g., unused textures to free VRAM.
        // The upcoming chapters exclude this cleanup code.
        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }
    }

    public class Shader
    {
        public int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            int VertexShader, FragmentShader;
            string VertexShaderSource = File.ReadAllText(vertexPath);

            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success1);
            if (success1 == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success2);
            if (success2 == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success3);
            if (success3 == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
