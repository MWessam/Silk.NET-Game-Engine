// using LunarEngine.Events;
// using Silk.NET.Input;
// using Silk.NET.Maths;
// using Silk.NET.OpenGL;
// using Silk.NET.Windowing;
//
// namespace LunarEngine.GameEngine;
//
// public class WindowManager : IDisposable
// {
//     public Vector2D<int> Viewport { get; private set; }
//     private IWindow _window;
//     private IInputContext _inputContext;
//
//     public IWindow CurrentWindow => _window;
//     public void Initialize()
//     {
//         WindowOptions options = WindowOptions.Default;
//         options.Size = new Vector2D<int>(800, 600);
//         options.Title = "Engine";
//         _window = Window.Create(options);
//         _window.Load += OnWindowLoaded;
//     }
//
//     private void OnWindowLoaded()
//     {
//         var api = GL.GetApi(_window);
//         EventBus<WindowInitializedEvent>.Raise(new(_window, api));
//     }
//
//     public void Dispose()
//     {
//         _inputContext.Dispose();
//         _window.Dispose();
//     }
// }