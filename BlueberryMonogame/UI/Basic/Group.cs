using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public partial class Group : Element, ICullable
    {
        /// <summary>
        /// Returns the transform for this group's coordinate system
        /// </summary>
        /// <returns>The transform.</returns>
        protected Mat ComputeTransform()
        {
            var mat = Matrix.Identity;

            if (originX != 0 || originY != 0)
                mat = Matrix.Multiply(mat, Matrix.CreateTranslation(-originX, -originY, 0));

            if (rotation != 0)
                mat = Matrix.Multiply(mat, Matrix.CreateRotationX(MathHelper.ToRadians(rotation)));

            if (scaleX != 1 || scaleY != 1)
                mat = Matrix.Multiply(mat, Matrix.CreateScale(scaleX, scaleY, 1));

            mat = Matrix.Multiply(mat, Matrix.CreateTranslation(x + originX, y + originY, 0));

            // Find the first parent that transforms
            Group parentGroup = parent;
            while (parentGroup != null)
            {
                if (parentGroup.transform)
                    break;
                parentGroup = parentGroup.parent;
            }

            if (parentGroup != null)
                mat = Matrix.Multiply(mat, parentGroup.ComputeTransform().m);

            return new Mat(mat);
        }

        /// <summary>
        /// Set the batch's transformation matrix, often with the result of {@link #computeTransform()}. Note this causes the batch to 
        /// be flushed. {@link #resetTransform(Batch)} will restore the transform to what it was before this call.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        /// <param name="transform">Transform.</param>
        protected void ApplyTransform(Graphics graphics, Mat transform)
        {
            //previousBatcherTransform = graphics.TransformMatrix;
            //graphics.End();
            //graphics.Begin(transform);
        }

        /// <summary>
		/// Restores the batch transform to what it was before {@link #applyTransform(Batch, Matrix4)}. Note this causes the batch to
		/// be flushed
		/// </summary>
		/// <param name="batch">Batch.</param>
		protected void ResetTransform(Graphics graphics)
        {
            //graphics.End();
            //graphics.Begin(previousBatcherTransform);
        }
    }
}
