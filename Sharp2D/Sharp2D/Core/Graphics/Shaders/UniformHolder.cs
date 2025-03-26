using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;

namespace Sharp2D.Core.Graphics.Shaders
{
    public class UniformHolder
    {
        internal Dictionary<string, int> locations = new Dictionary<string, int>();
        private Dictionary<int, object> values = new Dictionary<int, object>();
        private Dictionary<Type, MethodInfo> method_cache = new Dictionary<Type, MethodInfo>();
        private int program_id;

        internal UniformHolder(int program_id)
        {
            this.program_id = program_id;
        }

        public int this[string var]
        {
            get
            {
                int id;
                bool found = locations.TryGetValue(var, out id);
                if (found) return id;
                
                id = GL.GetUniformLocation(program_id, var);
                if (id == -1)
                    throw new System.IO.FileNotFoundException("The variable was not found!", var);
                locations.Add(var, id);

                return id;
            }
        }

        [UniformAttribute]
        public void SetUniform(float value, int id)
        {
            GL.Uniform1(id, value);
        }

        [UniformAttribute]
        public void SetUniform(double value, int id)
        {
            GL.Uniform1(id, value);
        }

        [UniformAttribute]
        public void SetUniform(int value, int id)
        {
            GL.Uniform1(id, value);
        }

        [UniformAttribute]
        public void SetUniform(Vector2 vector, int id)
        {
            GL.Uniform2(id, vector);
        }

        [UniformAttribute]
        public void SetUniform(Vector3 vector, int id)
        {
            GL.Uniform3(id, vector);
        }

        [UniformAttribute]
        public void SetUniform(Vector4 vector, int id)
        {
            GL.Uniform4(id, vector.X, vector.Y, vector.Z, vector.W);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2 matrix, int id)
        {
            GL.UniformMatrix2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2d matrix, int id)
        {
            GL.UniformMatrix2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2x3 matrix, int id)
        {
            GL.UniformMatrix2x3(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2x3d matrix, int id)
        {
            GL.UniformMatrix2x3(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2x4 matrix, int id)
        {
            GL.UniformMatrix2x4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix2x4d matrix, int id)
        {
            GL.UniformMatrix2x4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3 matrix, int id)
        {
            GL.UniformMatrix3(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3d matrix, int id)
        {
            GL.UniformMatrix3(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3x2 matrix, int id)
        {
            GL.UniformMatrix3x2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3x2d matrix, int id)
        {
            GL.UniformMatrix3x2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3x4 matrix, int id)
        {
            GL.UniformMatrix3x4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix3x4d matrix, int id)
        {
            GL.UniformMatrix3x4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix4 matrix, int id)
        {
            GL.UniformMatrix4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix4d matrix, int id)
        {
            GL.UniformMatrix4(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix4x2 matrix, int id)
        {
            GL.UniformMatrix4x2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix4x2d matrix, int id)
        {
            GL.UniformMatrix4x2(id, true, ref matrix);
        }

        [UniformAttribute]
        public void SetUniform(Matrix4x3 matrix, int id)
        {
            GL.UniformMatrix4x3(id, true, ref matrix);
        }
        
        [UniformAttribute]
        public void SetUniform(Matrix4x3d matrix, int id)
        {
            GL.UniformMatrix4x3(id, true, ref matrix);
        }

        private void _setUniform(object obj, int id)
        {
            Type target = obj.GetType();

            if (!method_cache.ContainsKey(target))
            {
                //Utils.Logger.Debug("Searching for method valid for target type " + target.Name);

                MethodInfo[] methods = GetType().GetMethods();

                foreach (MethodInfo method in methods)
                {
                    if (method.GetCustomAttributes(typeof(UniformAttribute), false).Any())
                    {
                        UniformAttribute atr = (UniformAttribute)method.GetCustomAttribute(typeof(UniformAttribute), false);
                        if (method.GetParameters()[0].ParameterType == target)
                        {
                            method_cache.Add(target, method);
                            break;
                        }
                    }
                }
            }

            MethodInfo _method = method_cache[target];

            //Utils.Logger.Debug("Invoking " + _method.Name + " with first parameter of type " + _method.GetParameters()[0].ParameterType + " with object " + obj + " of type " + obj.GetType().Name);

            _method.Invoke(this, new object[] { obj, id });
            if (values.ContainsKey(id))
                values[id] = obj;
            else
                values.Add(id, obj);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class UniformAttribute : Attribute
    {
    }
}
