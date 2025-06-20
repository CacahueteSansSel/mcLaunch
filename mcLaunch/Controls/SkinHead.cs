using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace mcLaunch.Controls;

public class SkinHead : Control
{
    public IImage? Texture { get; set; }

    public override void Render(DrawingContext context)
    {
        if (Texture == null) return;

        base.Render(context);

        float multiplier = (float)Math.Max(Bounds.Width / 64, Bounds.Height / 64) * 2f;
        float sourceImageSizeMultiplier = (float)Texture.Size.Width / 64f;
        int startX = 4;
        float topLayerOffset = 0.5f;

        using (DrawingContext.PushedState state = context.PushRenderOptions(new RenderOptions()
                   { BitmapInterpolationMode = BitmapInterpolationMode.None }))
        {
            // Head
            context.DrawImage(Texture, new Rect(8, 8, 8, 8) * sourceImageSizeMultiplier,
                new Rect(2, 2, Bounds.Width - 4, Bounds.Height - 4));
            // Head Top Layer
            context.DrawImage(Texture, new Rect(40, 8, 8, 8) * sourceImageSizeMultiplier,
                new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
    }
}