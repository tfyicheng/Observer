using LibreHardwareMonitor.Hardware;
using System;
using System.Linq;

namespace Observer
{
    public class SystemMonitor : IDisposable
    {

        //使用示例
        //using (var monitor = new SystemMonitor())
        //{
        //    while (true)
        //    {
        //        Console.WriteLine($"CPU 使用率: {monitor.GetCpuUsage():F1}%");
        //        Console.WriteLine($"CPU 温度: {monitor.GetCpuTemperature():F1}°C");
        //        Console.WriteLine($"GPU 温度: {monitor.GetGpuTemperature():F1}°C");
        //        Console.WriteLine($"内存使用率: {monitor.GetMemoryUsage():F1}%");
        //        Console.WriteLine("-------------------------------------------------");
        //        System.Threading.Thread.Sleep(2000);
        //    }
        //}


        private readonly Computer _computer;

        public SystemMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };
            _computer.Open();
        }

        /// <summary>
        /// 获取 CPU 使用率（总核平均 %）
        /// </summary>
        public float GetCpuUsage()
        {
            float cpuUsage = 0;
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Total"));
                    if (loadSensor != null && loadSensor.Value.HasValue)
                        cpuUsage = loadSensor.Value.Value;
                }
            }
            return cpuUsage;
        }

        /// <summary>
        /// 获取 CPU 温度（取第一个可用温度传感器）
        /// </summary>
        public float? GetCpuTemperature()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                    if (tempSensor != null && tempSensor.Value.HasValue)
                        return tempSensor.Value.Value;
                }
            }
            return null; // 无法获取
        }

        /// <summary>
        /// 获取 GPU 温度（第一个 GPU）
        /// </summary>
        public float? GetGpuTemperature()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    hardware.Update();
                    var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                    if (tempSensor != null && tempSensor.Value.HasValue)
                        return tempSensor.Value.Value;
                }
            }
            return null; // 无法获取
        }

        /// <summary>
        /// 获取内存使用率（百分比）
        /// </summary>
        public float? GetMemoryUsage()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Memory)
                {
                    hardware.Update();
                    var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
                    if (loadSensor != null && loadSensor.Value.HasValue)
                        return loadSensor.Value.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 手动更新硬件数据（可选）
        /// </summary>
        public void Update()
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
            }
        }

        public void Dispose()
        {
            _computer.Close();
        }
    }
}
