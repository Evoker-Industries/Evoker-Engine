using System;
using System.Linq;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace EvokerEngine.Graphics;

/// <summary>
/// Vulkan swapchain for presenting images
/// </summary>
public unsafe class VulkanSwapchain
{
    private readonly VulkanContext _context;
    private SwapchainKHR _swapchain;
    private Image[] _images = Array.Empty<Image>();
    private ImageView[] _imageViews = Array.Empty<ImageView>();
    private Format _imageFormat;
    private Extent2D _extent;
    private KhrSwapchain _khrSwapchain;

    public SwapchainKHR Swapchain => _swapchain;
    public Image[] Images => _images;
    public ImageView[] ImageViews => _imageViews;
    public Format ImageFormat => _imageFormat;
    public Extent2D Extent => _extent;

    public VulkanSwapchain(VulkanContext context)
    {
        _context = context;
        _khrSwapchain = new KhrSwapchain(_context.Vk.Context);
    }

    public void Create(uint width, uint height)
    {
        var surfaceCapabilities = GetSurfaceCapabilities();
        var surfaceFormat = ChooseSurfaceFormat();
        var presentMode = ChoosePresentMode();
        var extent = ChooseExtent(surfaceCapabilities, width, height);

        uint imageCount = surfaceCapabilities.MinImageCount + 1;
        if (surfaceCapabilities.MaxImageCount > 0 && imageCount > surfaceCapabilities.MaxImageCount)
        {
            imageCount = surfaceCapabilities.MaxImageCount;
        }

        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _context.Surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = surfaceCapabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true,
            OldSwapchain = default
        };

        fixed (SwapchainKHR* swapchainPtr = &_swapchain)
        {
            if (_khrSwapchain.CreateSwapchain(_context.Device, &createInfo, null, swapchainPtr) != Result.Success)
            {
                throw new Exception("Failed to create swapchain");
            }
        }

        _imageFormat = surfaceFormat.Format;
        _extent = extent;

        // Get swapchain images
        uint swapchainImageCount = 0;
        _khrSwapchain.GetSwapchainImages(_context.Device, _swapchain, &swapchainImageCount, null);
        _images = new Image[swapchainImageCount];
        fixed (Image* imagesPtr = _images)
        {
            _khrSwapchain.GetSwapchainImages(_context.Device, _swapchain, &swapchainImageCount, imagesPtr);
        }

        // Create image views
        CreateImageViews();
    }

    private SurfaceCapabilitiesKHR GetSurfaceCapabilities()
    {
        SurfaceCapabilitiesKHR capabilities;
        var khrSurface = new KhrSurface(_context.Vk.Context);
        khrSurface.GetPhysicalDeviceSurfaceCapabilities(_context.PhysicalDevice, _context.Surface, &capabilities);
        return capabilities;
    }

    private SurfaceFormatKHR ChooseSurfaceFormat()
    {
        uint formatCount = 0;
        var khrSurface = new KhrSurface(_context.Vk.Context);
        khrSurface.GetPhysicalDeviceSurfaceFormats(_context.PhysicalDevice, _context.Surface, &formatCount, null);

        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* formatsPtr = formats)
        {
            khrSurface.GetPhysicalDeviceSurfaceFormats(_context.PhysicalDevice, _context.Surface, &formatCount, formatsPtr);
        }

        foreach (var format in formats)
        {
            if (format.Format == Format.B8G8R8A8Srgb && format.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return format;
            }
        }

        return formats[0];
    }

    private PresentModeKHR ChoosePresentMode()
    {
        uint presentModeCount = 0;
        var khrSurface = new KhrSurface(_context.Vk.Context);
        khrSurface.GetPhysicalDeviceSurfacePresentModes(_context.PhysicalDevice, _context.Surface, &presentModeCount, null);

        var presentModes = new PresentModeKHR[presentModeCount];
        fixed (PresentModeKHR* presentModesPtr = presentModes)
        {
            khrSurface.GetPhysicalDeviceSurfacePresentModes(_context.PhysicalDevice, _context.Surface, &presentModeCount, presentModesPtr);
        }

        foreach (var mode in presentModes)
        {
            if (mode == PresentModeKHR.MailboxKhr)
            {
                return mode;
            }
        }

        return PresentModeKHR.FifoKhr; // Always available
    }

    private Extent2D ChooseExtent(SurfaceCapabilitiesKHR capabilities, uint width, uint height)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        var actualExtent = new Extent2D
        {
            Width = Math.Clamp(width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
            Height = Math.Clamp(height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height)
        };

        return actualExtent;
    }

    private void CreateImageViews()
    {
        _imageViews = new ImageView[_images.Length];

        for (int i = 0; i < _images.Length; i++)
        {
            var createInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _images[i],
                ViewType = ImageViewType.Type2D,
                Format = _imageFormat,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            fixed (ImageView* imageViewPtr = &_imageViews[i])
            {
                if (_context.Vk.CreateImageView(_context.Device, &createInfo, null, imageViewPtr) != Result.Success)
                {
                    throw new Exception($"Failed to create image view {i}");
                }
            }
        }
    }

    public void Cleanup()
    {
        foreach (var imageView in _imageViews)
        {
            _context.Vk.DestroyImageView(_context.Device, imageView, null);
        }

        if (_swapchain.Handle != 0)
        {
            _khrSwapchain.DestroySwapchain(_context.Device, _swapchain, null);
        }
    }
}
