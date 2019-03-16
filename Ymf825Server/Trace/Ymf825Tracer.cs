using System;
using System.Threading;
using Ymf825;

namespace Ymf825Server.Trace
{
    public class Ymf825Tracer : IYmf825
    {
        /// <summary>
        /// レジスタに書き込んだバイト数を取得します。
        /// </summary>
        public long WriteBytes
            => this.writeBytes;

        /// <summary>
        /// レジスタに BurstWrite コマンドで書き込んだバイト数を取得します。
        /// </summary>
        public long BurstWriteBytes
            => this.burstWriteBytes;

        /// <summary>
        /// レジスタを読み込んだバイト数を取得します。
        /// </summary>
        public long ReadBytes
            => this.readBytes;

        /// <summary>
        /// レジスタに書き込んだ回数を取得します。
        /// </summary>
        public long WriteCommandCount
            => this.writeCommandCount;

        /// <summary>
        /// レジスタに BurstWrite コマンドで書き込んだ回数を取得します。
        /// </summary>
        public long BurstWriteCommandCount
            => this.burstWriteCommandCount;

        /// <summary>
        /// レジスタを読み込んだ回数を取得します。
        /// </summary>
        public long ReadCommandCount
            => this.readCommandCount;

        public event EventHandler<DataTransferedEventArgs> DataWrote;

        public event EventHandler<DataBurstWriteEventArgs> DataBurstWrote;

        public event EventHandler<DataTransferedEventArgs> DataRead;

        public TargetChip AvailableChips
            => this.innerDevice.AvailableChips;

        public TargetChip CurrentTargetChips
            => this.innerDevice.CurrentTargetChips;

        public bool AutoFlush
        {
            get => this.innerDevice.AutoFlush;
            set => this.innerDevice.AutoFlush = value;
        }

        public bool SupportReadOperation
            => this.innerDevice.SupportReadOperation;

        public bool SupportHardwareReset
            => this.innerDevice.SupportHardwareReset;

        private readonly IYmf825 innerDevice;

        private long writeBytes = 0;
        private long burstWriteBytes = 0;
        private long readBytes = 0;
        private long writeCommandCount = 0;
        private long burstWriteCommandCount = 0;
        private long readCommandCount = 0;

        public Ymf825Tracer(IYmf825 innerDevice)
            => this.innerDevice = innerDevice;

        public void Write(byte address, byte data)
        {
            var targetChip = this.CurrentTargetChips;
            this.innerDevice.Write(address, data);

            this.AddCount(ref this.writeCommandCount, 1);
            this.AddCount(ref this.writeBytes, 2);
            this.DataWrote?.Invoke(this, new DataTransferedEventArgs(targetChip, address, data));
        }

        public void BurstWrite(byte address, byte[] data, int offset, int count)
        {
            var targetChip = this.CurrentTargetChips;
            this.innerDevice.BurstWrite(address, data, offset, count);

            this.AddCount(ref this.writeBytes, 1);
            this.AddCount(ref this.burstWriteBytes, count);
            this.AddCount(ref this.burstWriteCommandCount, 1);
            this.DataBurstWrote?.Invoke(this, new DataBurstWriteEventArgs(targetChip, address, data, offset, count));
        }

        public byte Read(byte address)
        {
            var targetChip = this.CurrentTargetChips;
            var ret = this.innerDevice.Read(address);

            this.AddCount(ref this.writeBytes, 1);
            this.AddCount(ref this.readBytes, 1);
            this.AddCount(ref this.readCommandCount, 1);
            this.DataRead?.Invoke(this, new DataTransferedEventArgs(targetChip, address, ret));

            return ret;
        }

        public void Flush()
            => this.innerDevice.Flush();

        public void InvokeHardwareReset()
            => this.innerDevice.InvokeHardwareReset();

        public void SetTarget(TargetChip chips)
            => this.innerDevice.SetTarget(chips);

        public void Dispose()
            => this.innerDevice.Dispose();

        private void AddCount(ref long counter, long add)
        {
            if (add == 1)
            {
                Interlocked.Increment(ref counter);
                return;
            }

            long currentValue, newValue;
            do
            {
                currentValue = counter;
                newValue = currentValue + add;
            }
            while (Interlocked.CompareExchange(ref counter, newValue, currentValue) != currentValue);
        }
    }
}
