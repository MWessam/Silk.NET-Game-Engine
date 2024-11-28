using System.Drawing;
using LunarEngine.ECS.Systems;
using LunarEngine.UI;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Engine.Graphics;
public class Renderer
{
    public event Action<IWindow>? OnWindowLoad;
    public event Action<GL>? OnApiInitialized;
    public event Action<double>? OnUpdateLoopTick;
    public event Action<Vector2D<int>>? OnViewportResized;
    public event Action OnWindowClosed;
    public IWindow WindowContext { get; private set; }
    public GL Api { get; private set; }
    public static Renderer? Instance;
    private List<RenderCommand> _renderQueue = new();
    public static Renderer CreateRenderer()
    {
        if (Instance is not null) return Instance;
        var renderer = new Renderer();
        renderer.Initialize();
        Instance = renderer;
        return renderer;
        
    }
    private void Initialize()
    {
        InitializeWindow();
        InitializeEngineEvents();
    }
    public void Start()
    {
        WindowContext.Run();
        WindowContext.Dispose();
    }

    #region INTERNAL

    private Renderer()
    {
        
    }

    private void InitializeWindow()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        WindowContext = Window.Create(options);
    }
    private void InitializeEngineEvents()
    {
        WindowContext.FramebufferResize += OnViewportResize;
        WindowContext.Load += _OnWindowLoad;
        WindowContext.Update += OnUpdate;
        WindowContext.Closing += OnClose;
    }

    private void OnClose()
    {
        OnWindowClosed?.Invoke();
    }

    private void _OnWindowLoad()
    {
        try
        {
            Api = GL.GetApi(WindowContext);
            Api.ClearColor(Color.Black);
            Api.Enable(GLEnum.Blend);
            Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            OnApiInitialized?.Invoke(Api);
            OnWindowLoad?.Invoke(WindowContext);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error initializing Graphics Engine.");
            throw;
        }
    }
    private void OnUpdate(double dt)
    {
        OnUpdateLoopTick?.Invoke(dt);
    }

    private void OnViewportResize(Vector2D<int> viewport)
    {
        Api.Viewport(viewport);
        OnViewportResized?.Invoke(viewport);
    }
    internal void SetRenderTarget(FrameBuffer sceneFrameBuffer)
    {
        
    }

    #endregion

    public void BeginFrame()
    {
        Api.ClearColor(Color.Black);
        Api.Clear((uint)(GLEnum.DepthBufferBit | GLEnum.ColorBufferBit));
    }
    
    public void Render(double deltaTime = 0)
    {

        BeginFrame();
        WindowContext.Title = $"Lunar Engine FPS: {(int)(1 / deltaTime)}";
        foreach (var renderCommand in _renderQueue)
        {
            // Resolve command type. Better performance than reflection
            switch (renderCommand.Type)
            {
                case RenderCommand.CommandType.SpriteDraw:
                    var spriteDrawCommand = (SpriteDrawCommand)renderCommand;
                    spriteDrawCommand.Sprite.Render(spriteDrawCommand.SpriteData);
                    break;
            }
        }
        EndFrame();
    }
    public void EndFrame()
    {
        Clean();
    }

    public void Clear()
    {
        Api.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));

    }
    public void SubmitRenderCommand(SpriteDrawCommand spriteDrawCommand)
    {
        _renderQueue.Add(spriteDrawCommand);
    }
    private void Clean()
    {
        _renderQueue.Clear();
    }


}