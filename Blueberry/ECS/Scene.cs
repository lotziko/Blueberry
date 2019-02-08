
namespace Blueberry
{
    public class Scene
    {
        protected int width, height;

        public Camera camera;
        public Graphics graphics;

        /// <summary>
        /// Entities on scene
        /// </summary>
        public readonly EntityList entities;

        /// <summary>
        /// Renderable entities on scene
        /// </summary>
        public readonly RenderableList renderables;

        public Entity CreateEntity(string name)
        {
            var entity = new Entity(name);
            entities.Add(entity);
            entity.scene = this;
            return entity;
        }

        public T AddEntity<T>(T entity) where T : Entity
        {
            entity.scene = this;
            entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            entity.components.Clear();
            entities.Remove(entity);
        }


        public void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public virtual void Initialize() { }

        public void Update(float delta)
        {
            entities.Update(delta);
        }

        public virtual void Render()
        {
            camera.ForceCalculate();
            var pT = graphics.Transform;
            var pP = graphics.Projection;
            graphics.Transform = camera.TransformMatrix;
            graphics.Projection = camera.ProjectionMatrix;
            graphics.Begin();
            renderables.Render(graphics, camera);
            graphics.End();
            graphics.Transform = pT;
            graphics.Projection = pP;
        }

        public void Begin() { }
        public void End() { }

        public Scene(int width, int height)
        {
            entities = new EntityList();
            renderables = new RenderableList();

            var cameraEntity = CreateEntity("camera");
            camera = cameraEntity.AddComponent(new Camera());

            this.width = width;
            this.height = height;

            OnResize(width, height);
        }

        public Scene()
        {
            entities = new EntityList();
            renderables = new RenderableList();

            var cameraEntity = CreateEntity("camera");
            camera = cameraEntity.AddComponent(new Camera());

            OnResize(Screen.Width, Screen.Height);
        }

        public virtual void OnResize(int width, int height)
        {
            camera.SetSize(width, height);
            camera.SetPosition(width / 2, height / 2);
        }
    }
}
