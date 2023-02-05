﻿using System;
using System.Threading.Tasks;
using Automatica.Core.Base.IO;
using Automatica.Core.Driver;
using Microsoft.Extensions.Logging;
using P3.Driver.ModBusDriver;
using P3.Driver.ModBusDriver.Slave;
using P3.Driver.ModBusDriverFactory.Attributes;

namespace P3.Driver.ModBusDriverFactory.Slave
{

    public class ModBusSlaveAttribute : DriverBase
    {
        public IModBusSlaveDriver Driver { get; }
        private readonly ModBusSlaveDevice _parent;
        private readonly ModBusAttribute _attribute;

        public ModBusAttribute Attribute => _attribute;

        public ModBusSlaveAttribute(IDriverContext driverContext, ModBusSlaveDevice parent, IModBusSlaveDriver driver, ModBusAttribute attribute) : base(driverContext)
        {
            Driver = driver;
            _parent = parent;
            _attribute = attribute;
        }

        public override async Task WriteValue(IDispatchable source, object value)
        {
            await Task.CompletedTask;
            var shortValue = _attribute.ConvertValueToBus(source, value, out var convertedValue);

            DriverContext.Logger.LogInformation(
                $"Get value ({value} - {String.Join("-", shortValue)}) from {source.Id} to {_parent.Name + $"(-{_parent.DeviceId}-)" + Name} (Register: {_attribute.Register}, Lenght: {_attribute.RegisterLength}, Table: {_attribute.Table})");
            switch (_attribute.Table)
            {
                case ModBusTable.Coil:
                    Driver.SetCoil(_parent.DeviceId, _attribute.Register, shortValue[0] == 1);
                    return;
                case ModBusTable.DiscreteInput:
                    Driver.SetDiscreteInput(_parent.DeviceId, _attribute.Register, shortValue[0] == 1);
                    return;
            }

            for (int i = 0; i < _attribute.RegisterLength; i++)
            {
                var registerAddress = (ushort)(_attribute.Register + i);
                switch (_attribute.Table)
                {
                    case ModBusTable.HoldingRegister:
                        Driver.SetHoldingRegister(_parent.DeviceId, registerAddress, shortValue[i]);
                        break;
                    case ModBusTable.InputRegister:
                        Driver.SetInputRegister(_parent.DeviceId, registerAddress, shortValue[i]);
                        break;
                }
            }

            DispatchValue(convertedValue);
        }

        public override IDriverNode CreateDriverNode(IDriverContext ctx)
        {
            return null;
        }
    }
}
