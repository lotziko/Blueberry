using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Drawing;

namespace BlueberryOpenTK
{
    public class ShaderProgram
    {
        internal int fShader, vShader, Program, positionAttribute, colorAttribute, textureCoordAttribute;
        protected string vsSource, fsSource;
        public readonly UniformList Params;

        public ShaderProgram()
        {
            Initialize();
            Params = new UniformList(Program);
        }

        ~ShaderProgram()
        {
            //GL.DeleteShader(vShader);
            //GL.DeleteShader(fShader);
        }

        protected virtual void Initialize() { }

        public class UniformList
        {
            protected int program;
            protected Dictionary<string, Uniform> uniforms;

            public UniformList(int program)
            {
                this.program = program;
                this.uniforms = new Dictionary<string, Uniform>();
            }

            public Uniform this[string name]
            {
                get
                {
                    if (uniforms.ContainsKey(name))
                        return uniforms[name];
                    else
                    {
                        var uniform = new Uniform(program, GL.GetUniformLocation(program, name));
                        uniforms.Add(name, uniform);
                        return uniform;
                    }
                }
            }
        }

        public class Uniform
        {
            protected int program;
            protected int location;

            public Uniform(int program, int location)
            {
                this.program = program;
                this.location = location;
            }

            public void SetValue(Matrix4 matrix)
            {
                GL.ProgramUniformMatrix4(program, location, false, ref matrix);
            }

            public void SetValue(int value)
            {
                GL.ProgramUniform1(program, location, value);
            }

            public void SetValue(Color4 color)
            {
                GL.ProgramUniform4(program, location, color);
            }
        }

        public static ShaderProgram BasicPrimitive = new BasicPrimitiveShader();

        private class BasicPrimitiveShader : ShaderProgram
        {
            protected override void Initialize()
            {
                vsSource =
                @"
                attribute vec2 in_Position;
                attribute vec4 in_Colour;

                uniform mat4 projection; 
                uniform mat4 transform;

                varying vec4 v_vColour;

                void main()
                {
	                gl_Position = projection * transform * vec4(in_Position, 1.0, 1.0);
	                v_vColour = in_Colour;
                }
                ";

                fsSource =
                @"
                varying vec4 v_vColour;

                void main()
                {
	                gl_FragColor = v_vColour;
                }
                ";

                vShader = GL.CreateShader(ShaderType.VertexShader);

                GL.ShaderSource(vShader, vsSource);
                GL.CompileShader(vShader);

                fShader = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(fShader, fsSource);
                GL.CompileShader(fShader);

                Program = GL.CreateProgram();

                GL.AttachShader(Program, vShader);
                GL.AttachShader(Program, fShader);

                GL.LinkProgram(Program);

                positionAttribute = GL.GetAttribLocation(Program, "in_Position");
                colorAttribute = GL.GetAttribLocation(Program, "in_Colour");
            }
        }

        public static ShaderProgram BasicTexture = new BasicTextureShader();

        private class BasicTextureShader : ShaderProgram
        {
            protected override void Initialize()
            {
                vsSource = 
                @"
                attribute vec2 in_Position;
                attribute vec4 in_Colour;
                attribute vec2 in_TextureCoord;

                uniform mat4 projection; 
                uniform mat4 transform;

                varying vec2 v_vTexcoord;
                varying vec4 v_vColour;

                void main()
                {
	                gl_Position = projection * transform * vec4(in_Position, 1.0, 1.0);

	                v_vColour = in_Colour;
	                v_vTexcoord = in_TextureCoord;
                }
                ";

                fsSource =
                @"
                uniform sampler2D tex;

                varying vec2 v_vTexcoord;
                varying vec4 v_vColour;

                void main()
                {
	                gl_FragColor = v_vColour * texture2D(tex, v_vTexcoord);
                }
                ";

                vShader = GL.CreateShader(ShaderType.VertexShader);

                GL.ShaderSource(vShader, vsSource);
                GL.CompileShader(vShader);

                fShader = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(fShader, fsSource);
                GL.CompileShader(fShader);

                Program = GL.CreateProgram();

                GL.AttachShader(Program, vShader);
                GL.AttachShader(Program, fShader);

                GL.LinkProgram(Program);
                
                positionAttribute = GL.GetAttribLocation(Program, "in_Position");
                colorAttribute = GL.GetAttribLocation(Program, "in_Colour");
                textureCoordAttribute = GL.GetAttribLocation(Program, "in_TextureCoord");
            }
        }
    }
}
