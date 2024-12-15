using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Utils;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Physics;

namespace ComponentFactories
{
    /// <summary>
    /// Handles lifetime of components in the engine.
    /// This is used for when you want to create components dynamically.
    /// </summary>
    class ComponentFactoryManager : Singleton<ComponentFactoryManager>, ISingletonObject, IDisposable
    {
        private Dictionary<Type, IDefaultComponentFactory> _defaultComponentFactories = new();
        internal IComponent GetDefaultComponent(Type componentType)
        {
            if (_defaultComponentFactories.TryGetValue(componentType, out var factory))
            {
                return factory.Produce();
            }
            if (typeof(IComponent).IsAssignableFrom(componentType))
            {
                var component = Activator.CreateInstance(componentType);
                return (IComponent)component;
            }
            throw new ArgumentException($"Component type given {componentType.Name} is not a valid component. Make sure it implement IComponent");
        }

        public void InitSingleton()
        {
            _defaultComponentFactories = new()
            {
                {typeof(Camera), new DefaultCameraComponentFactory()},
                {typeof(RigidBody2D), new DefaultRigidBody2DComponentFactory()},
                {typeof(BoxCollider2D), new DefaultAABBComponentFactory()},
                {typeof(Scale), new DefaultScaleComponentFactory()}
            };
        }
        public void Dispose()
        {
            _defaultComponentFactories.Clear();
        }
    }
    internal interface IDefaultComponentFactory
    {
        IComponent Produce();
    }
    internal class DefaultScaleComponentFactory : IDefaultComponentFactory
    {
        public IComponent Produce()
        {
            return new Scale()
            {
                UserValue = Vector3.One,
                BaseValue = Vector3.One,
            };
        }
    }
    internal class DefaultCameraComponentFactory : IDefaultComponentFactory
    {
        public IComponent Produce()
        {
            return new Camera()
            {
                Width = 5,
                Height = 5,
                Near = 0.1f,
                Far = 1000.0f,
            };
        }
    }
    internal class DefaultRigidBody2DComponentFactory : IDefaultComponentFactory
    {
        public IComponent Produce()
        {
            return new RigidBody2D()
            {
                Mass = 1.0f,
                GravityScale = 1.0f,
                IsInterpolating = false,
            };
        }
    }

    internal class DefaultAABBComponentFactory : IDefaultComponentFactory
    {
        public IComponent Produce()
        {
            return new BoxCollider2D()
            {
                Width = 1.0f,
                Height = 1.0f,
            };
        }
    }

    internal class EntityFactory
    {
        public Entity CreateEntity(CommandBuffer commandBuffer)
        {
            var entity = commandBuffer.Create([typeof(Name), typeof(Transform), typeof(IsInstantiating)]);
            commandBuffer.Set(in entity, new Name
            {
                Value = "Entity"
            });
            return entity;
        }
        public Entity CreateEntity(World world)
        {
            var entity = world.Create();
            world.Set(entity, new Name
            {
                Value = "Entity"
            });
            world.Set(entity, new Transform());
            world.Set(entity, new IsInstantiating());
            return entity;
        }
    }
}
