using System;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using EvokerEngine.Core;

namespace EvokerEngine.Graphics;

/// <summary>
/// Vulkan renderer context
/// </summary>
public unsafe class VulkanContext
{
    private readonly Vk _vk;
    private Instance _instance;
    private PhysicalDevice _physicalDevice;
    private Device _device;
    private Queue _graphicsQueue;
    private SurfaceKHR _surface;
    private uint _graphicsQueueFamilyIndex;

    public Vk Vk => _vk;
    public Instance Instance => _instance;
    public PhysicalDevice PhysicalDevice => _physicalDevice;
    public Device Device => _device;
    public Queue GraphicsQueue => _graphicsQueue;
    public SurfaceKHR Surface => _surface;
    public uint GraphicsQueueFamilyIndex => _graphicsQueueFamilyIndex;

    public VulkanContext()
    {
        _vk = Vk.GetApi();
    }

    public void Initialize(IWindow window)
    {
        CreateInstance();
        CreateSurface(window);
        PickPhysicalDevice();
        CreateLogicalDevice();
    }

    private void CreateInstance()
    {
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            ApplicationVersion = Vk.MakeVersion(1, 0, 0),
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        var appNameBytes = System.Text.Encoding.UTF8.GetBytes("Evoker Engine Application\0");
        var engineNameBytes = System.Text.Encoding.UTF8.GetBytes("Evoker Engine\0");
        
        fixed (byte* appNamePtr = appNameBytes)
        fixed (byte* engineNamePtr = engineNameBytes)
        {
            appInfo.PApplicationName = appNamePtr;
            appInfo.PEngineName = engineNamePtr;

            var extensionNames = GetRequiredExtensions();
            
            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint)extensionNames.Length,
            };

            var extPointers = stackalloc byte*[extensionNames.Length];
            for (int i = 0; i < extensionNames.Length; i++)
            {
                var nameBytes = System.Text.Encoding.UTF8.GetBytes(extensionNames[i] + "\0");
                fixed (byte* namePtr = nameBytes)
                {
                    extPointers[i] = namePtr;
                }
            }
            createInfo.PpEnabledExtensionNames = extPointers;

            fixed (Instance* instancePtr = &_instance)
            {
                if (_vk.CreateInstance(&createInfo, null, instancePtr) != Result.Success)
                {
                    throw new Exception("Failed to create Vulkan instance");
                }
            }
        }
    }

    private string[] GetRequiredExtensions()
    {
        return new[] { "VK_KHR_surface", Platform.GetVulkanSurfaceExtension() };
    }

    private void CreateSurface(IWindow window)
    {
        _surface = window.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();
    }

    private void PickPhysicalDevice()
    {
        uint deviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, null);

        if (deviceCount == 0)
        {
            throw new Exception("Failed to find GPUs with Vulkan support");
        }

        var devices = new PhysicalDevice[deviceCount];
        fixed (PhysicalDevice* devicesPtr = devices)
        {
            _vk.EnumeratePhysicalDevices(_instance, &deviceCount, devicesPtr);
        }

        _physicalDevice = devices[0]; // Pick first device for simplicity

        PhysicalDeviceProperties properties;
        _vk.GetPhysicalDeviceProperties(_physicalDevice, &properties);
        
        var deviceName = Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName);
        EvokerEngine.Core.Logger.Info($"Selected GPU: {deviceName}");
    }

    private void CreateLogicalDevice()
    {
        _graphicsQueueFamilyIndex = FindQueueFamily();

        float queuePriority = 1.0f;
        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _graphicsQueueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        var deviceFeatures = new PhysicalDeviceFeatures();

        var swapchainExtBytes = System.Text.Encoding.UTF8.GetBytes("VK_KHR_swapchain\0");
        fixed (byte* swapchainExtPtr = swapchainExtBytes)
        {
            var extPtr = swapchainExtPtr;
            
            var createInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,
                PEnabledFeatures = &deviceFeatures,
                EnabledExtensionCount = 1,
                PpEnabledExtensionNames = &extPtr
            };

            fixed (Device* devicePtr = &_device)
            {
                if (_vk.CreateDevice(_physicalDevice, &createInfo, null, devicePtr) != Result.Success)
                {
                    throw new Exception("Failed to create logical device");
                }
            }
        }

        fixed (Queue* queuePtr = &_graphicsQueue)
        {
            _vk.GetDeviceQueue(_device, _graphicsQueueFamilyIndex, 0, queuePtr);
        }
    }

    private uint FindQueueFamily()
    {
        uint queueFamilyCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, queueFamiliesPtr);
        }

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                return i;
            }
        }

        throw new Exception("Failed to find suitable queue family");
    }

    public void Cleanup()
    {
        if (_device.Handle != 0)
        {
            _vk.DestroyDevice(_device, null);
        }

        if (_surface.Handle != 0)
        {
            var khrSurface = new KhrSurface(_vk.Context);
            khrSurface.DestroySurface(_instance, _surface, null);
        }

        if (_instance.Handle != 0)
        {
            _vk.DestroyInstance(_instance, null);
        }
    }
}
