using System.Drawing;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.OpenGLAPI;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Graphics;

public class GraphicsEngine
{
    public event Action<IWindow>? OnWindowInitialized;
    public event Action<GL>? OnApiInitialized;
    public event Action<double>? OnUpdateLoopTick;
    public event Action<Vector2D<int>>? OnViewportResized;
    public event Action OnWindowClosed;
    public Vector2 WindowResolution => new Vector2(_windowContext.Size.X, _windowContext.Size.Y);
    private IWindow _windowContext;
    private SpriteRendererSystem _spriteRendererSystem;
    public static GL Api { get; private set; }
    public void Initialize()
    {
        InitializeWindow();
        InitializeEngineEvents();
    }

    public void InjectSpriteRendererSystem(SpriteRendererSystem spriteRendererSystem)
    {
        _spriteRendererSystem = spriteRendererSystem;
    }
    public void Start()
    {
        _windowContext.Run();
        _windowContext.Dispose();
    }
    private  void InitializeWindow()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        _windowContext = Window.Create(options);
    }
    private  void InitializeEngineEvents()
    {
        _windowContext.FramebufferResize += OnViewportResize;
        _windowContext.Load += OnWindowLoad;
        _windowContext.Update += OnUpdate;
        _windowContext.Render += OnRender;
        _windowContext.Closing += OnClose;
    }

    private  void OnClose()
    {
        OnWindowClosed?.Invoke();
    }

    private  void OnWindowLoad()
    {
        try
        {
            Api = GL.GetApi(_windowContext);
            Api.ClearColor(Color.Black);
            OnApiInitialized?.Invoke(Api);
            OnWindowInitialized?.Invoke(_windowContext);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error initializing Graphics Engine.");
            throw;
        }
    }
    private  void OnUpdate(double dt)
    {
        OnUpdateLoopTick?.Invoke(dt);
    }
    private void OnRender(double deltaTime = 0)
    {
        Api.Clear(ClearBufferMask.ColorBufferBit);
        _spriteRendererSystem.Render((float)deltaTime);
        _windowContext.Title = $"Lunar Engine FPS: {(int)(1 / deltaTime)}";
    }
    private  void OnViewportResize(Vector2D<int> viewport)
    {
        Api.Viewport(viewport);
        OnViewportResized?.Invoke(viewport);
    }
}


public struct NeedsInitialization
{
}