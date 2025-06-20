using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace mcLaunch.Controls;

public class SkinPreview : Control
{
    public IImage? Texture { get; set; }
    public SkinType Type { get; set; }

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
            switch (Type)
            {
                case SkinType.Default:
                    // Head
                    context.DrawImage(Texture, new Rect(8, 8, 8, 8) * sourceImageSizeMultiplier,
                        new Rect(startX + 8, 0, 8, 8) * multiplier);

                    // Body
                    context.DrawImage(Texture, new Rect(20, 20, 8, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 8, 8, 8, 12) * multiplier);

                    // Arms (Left, Right)
                    context.DrawImage(Texture, new Rect(44, 20, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 4, 8, 4, 12) * multiplier);
                    context.DrawImage(Texture, new Rect(36, 52, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 16, 8, 4, 12) * multiplier);

                    // Legs (Left, Right)
                    context.DrawImage(Texture, new Rect(4, 20, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 8, 20, 4, 12) * multiplier);
                    context.DrawImage(Texture, new Rect(20, 52, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 12, 20, 4, 12) * multiplier);

                    // Head Top Layer
                    context.DrawImage(Texture, new Rect(40, 8, 8, 8) * sourceImageSizeMultiplier,
                        new Rect(startX + 8 - topLayerOffset, 0 - topLayerOffset, 8 + topLayerOffset * 2,
                            8 + topLayerOffset * 2) * multiplier);

                    // Body Top Layer
                    context.DrawImage(Texture, new Rect(20, 36, 8, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 8 - topLayerOffset, 8, 8 + topLayerOffset * 2, 12 + topLayerOffset * 2) *
                        multiplier);

                    // Arms Top Layer (Left, Right)
                    context.DrawImage(Texture, new Rect(44, 36, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 4 - topLayerOffset, 8, 4 + topLayerOffset * 2, 12 + topLayerOffset * 2) *
                        multiplier);
                    context.DrawImage(Texture, new Rect(52, 52, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 16 - topLayerOffset, 8, 4 + topLayerOffset * 2, 12 + topLayerOffset * 2) *
                        multiplier);

                    // Legs Top Layer (Left, Right)
                    context.DrawImage(Texture, new Rect(4, 36, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 8 - topLayerOffset, 20, 4 + topLayerOffset * 2, 12 + topLayerOffset * 2) *
                        multiplier);
                    context.DrawImage(Texture, new Rect(4, 52, 4, 12) * sourceImageSizeMultiplier,
                        new Rect(startX + 12 - topLayerOffset, 20, 4 + topLayerOffset * 2, 12 + topLayerOffset * 2) *
                        multiplier);
                    break;
                case SkinType.Slim:
                    break;
            }
        }
    }

    public enum SkinType
    {
        Default,
        Slim
    }
}