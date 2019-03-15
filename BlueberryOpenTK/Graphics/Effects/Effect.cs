using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Blueberry.OpenGL
{
    public class Effect
    {
        internal readonly int fShader, vShader, Program;
        public readonly UniformList Uniforms;
        public readonly AttributeList Attributes;

        protected GraphicsDevice GraphicsDevice { get; }

        public Effect(GraphicsDevice device, string vertex, string fragment)
        {
            vShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vShader, vertex);
            GL.CompileShader(vShader);

            fShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(fShader, fragment);
            GL.CompileShader(fShader);

            Program = GL.CreateProgram();

            GL.AttachShader(Program, vShader);
            GL.AttachShader(Program, fShader);

            GL.LinkProgram(Program);

            Uniforms = new UniformList(Program);
            Attributes = new AttributeList(Program);
            GraphicsDevice = device;
        }

        public virtual void Apply()
        {
            GraphicsDevice.Shader = this;
        }

        ~Effect()
        {
            //GL.DeleteShader(vShader);
            //GL.DeleteShader(fShader);
        }

        internal int GetAttributeLocation(VertexElementUsage usage, int usageIndex)
        {
            Attributes.TryGetValue(usage.SemanticName() + usageIndex, out EffectAttribute attr);
            if (attr == null)
                return -1;
            return attr.Location;
        }
        
        public class AttributeList : Dictionary<string, EffectAttribute>
        {
            protected int program;

            public AttributeList(int program)
            {
                this.program = program;

                GL.GetProgram(program, GetProgramParameterName.ActiveAttributes, out int n);

                for (int i = 0; i < n; i++)
                {
                    GL.GetActiveAttrib(program, i, 32, out int length, out int size, out ActiveAttribType type, out string name);
                    var l = GL.GetAttribLocation(program, name);
                    Add(name, new EffectAttribute(l, program, name, type));
                }
            }
        }

        public class UniformList : Dictionary<string, EffectUniform>
        {
            protected int program;

            public UniformList(int program)
            {
                this.program = program;

                GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out int n);

                for (int i = 0; i < n; i++)
                {
                    GL.GetActiveUniform(program, i, 32, out int length, out int size, out ActiveUniformType type, out string name);
                    
                    int l = GL.GetUniformLocation(program, name);
                    Add(name, new EffectUniform(l, program, name));
                }
            }
        }

        public class EffectAttribute
        {
            public int Location { get; }
            public int Program { get; }
            public string Name { get; }
            public ActiveAttribType Type { get; }

            public EffectAttribute(int location, int program, string name, ActiveAttribType type)
            {
                Location = location;
                Program = program;
                Name = name;
                Type = type;
            }
        }

        public class EffectUniform
        {
            public int Location { get; }
            public int Program { get; }
            public string Name { get; }

            public EffectUniform(int location, int program, string name)
            {
                Location = location;
                Program = program;
                Name = name;
            }

            public void SetValue(Matrix4 matrix)
            {
                GL.ProgramUniformMatrix4(Program, Location, false, ref matrix);
            }

            public void SetValue(int value)
            {
                GL.ProgramUniform1(Program, Location, value);
            }

            public void SetValue(Color4 color)
            {
                GL.ProgramUniform4(Program, Location, color);
            }
        }
    }
}
