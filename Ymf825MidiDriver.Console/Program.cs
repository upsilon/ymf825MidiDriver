using MidiUtils.IO;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using Ymf825;
using Ymf825.Driver;
using Ymf825.IO;

namespace Ymf825MidiDriver
{
    public class Program
    {
        public static void Main()
        {
            var pinCS0 = GpioPin.FromNumber(25); // Pin 22 (BCM 25)
            var pinCS1 = GpioPin.FromNumber(26); // Pin 37 (BCM 26)
            var pinRST = GpioPin.FromNumber(16); // Pin 36 (BCM 16)

            var spiSettings = new SpiConnectionSettings(busId: 0, chipSelectLine: 0);
            var gpioPinMap = new Dictionary<TargetChip, ChipPinConfig>
            {
                [TargetChip.Board0] = new ChipPinConfig(csPin: pinCS0, resetPin: pinRST),
                [TargetChip.Board1] = new ChipPinConfig(csPin: pinCS1, resetPin: pinRST),
            };

            using (var spiDevice = new UnixSpiDevice(spiSettings))
            using (var gpioController = new GpioController(PinNumberingScheme.Logical))
            using (var ymf825Device = new NativeSpi(spiDevice, gpioController, gpioPinMap))
            {
                var driver = new Ymf825Driver(ymf825Device);

                using (var midiIn = new AlsaMidiIn("ymf825 midi driver"))
                {
                    var toneItems = Array.Empty<ToneItem>();
                    var equalizerItems = Array.Empty<EqualizerItem>();
                    var midiDriver = new MidiDriver(toneItems, equalizerItems, midiIn, driver);
                    midiDriver.Start();

                    Console.ReadLine();
                }
            }
        }
    }
}
