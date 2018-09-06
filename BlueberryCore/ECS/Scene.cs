
namespace BlueberryCore
{
    public class Scene
    {
        public Camera camera;
        
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

        public virtual void Initialize() { }

        public void Update()
        {
            entities.Update();
        }

        public virtual void Render()
        {
            camera.ForceCalculate();
            Graphics.instance.Begin(camera.TransformMatrix);
            renderables.Render(Graphics.instance, camera);
            Graphics.instance.End();
        }

        public void Begin() { }
        public void End() { }

        public Scene()
        {
            entities = new EntityList();
            renderables = new RenderableList();

            var cameraEntity = CreateEntity("camera");
            camera = cameraEntity.AddComponent(new Camera());

            OnGraphicsDeviceReset();
        }

        public void OnGraphicsDeviceReset()
        {
            camera.SetSize(Screen.Width, Screen.Height);
            camera.SetPosition(Screen.Width / 2, Screen.Height / 2);
        }
    }
}
