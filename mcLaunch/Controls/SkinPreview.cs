using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace mcLaunch.Controls;

public class SkinPreview : Control
{
    public IImage? Texture { get; set; }
    public SkinType Type { get; set; }
    
    public override void Render(DrawingContext context)
    {
        if (Texture == null) return;
        
        base.Render(context);
        
        float multiplier = (float)Math.Max(Bounds.Width / 64, Bounds.Height / 64)*2f;
        int startX = 4;

        using (DrawingContext.PushedState state = context.PushRenderOptions(new RenderOptions() {BitmapInterpolationMode = BitmapInterpolationMode.None, EdgeMode = EdgeMode.Aliased, RequiresFullOpacityHandling = true}))
        {
            // Head
            context.DrawImage(Texture, new Rect(8, 8, 8, 8), new Rect(startX + 8, 0, 8, 8) * multiplier);
        
            // Body
            context.DrawImage(Texture, new Rect(20, 20, 8, 12), new Rect(startX + 8, 8, 8, 12) * multiplier);
        
            // Arms (Left, Right)
            context.DrawImage(Texture, new Rect(44, 20, 4, 12), new Rect(startX + 4, 8, 4, 12) * multiplier);
            context.DrawImage(Texture, new Rect(36, 52, 4, 12), new Rect(startX + 16, 8, 4, 12) * multiplier);
        
            // Legs (Left, Right)
            context.DrawImage(Texture, new Rect(4, 20, 4, 12), new Rect(startX + 8, 20, 4, 12) * multiplier);
            context.DrawImage(Texture, new Rect(20, 52, 4, 12), new Rect(startX + 12, 20, 4, 12) * multiplier);
        }

        switch (Type)
        {
            case SkinType.Default:
                break;
            case SkinType.Slim:
                break;
        }
    }
    
    public enum SkinType 
    {
        Default,
        Slim
    }
}