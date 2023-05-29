using DynamicData;
using ReactiveUI;
using Splat.ModeDetection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasmX86App.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? InputHexString { get; set; }
        public string? OutputAsm { get; set; }
        public bool Is16bit { get; set; }
        public bool Is32bit { get; set; } = true;
        public bool Is64bit { get; set; }
        public string? BaseAddress { get; set; }

        public MainViewModel()
        {
            this.WhenAnyValue(x => x.InputHexString)
                .Subscribe(x => DoDisasm());
            this.WhenAnyValue(x => x.Is16bit)
                .Subscribe(x => DoDisasm());
            this.WhenAnyValue(x => x.Is32bit)
                .Subscribe(x => DoDisasm());
            this.WhenAnyValue(x => x.Is64bit)
                .Subscribe(x => DoDisasm());
            this.WhenAnyValue(x => x.BaseAddress)
                .Subscribe(x => DoDisasm());
        }

        private void DoDisasm()
        {
            try
            {
                // Determine the architecture mode or us 32-bit by default
                SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_32;
                if (Is16bit)
                {
                    mode = SharpDisasm.ArchitectureMode.x86_16;
                }
                if (Is32bit)
                {
                    mode = SharpDisasm.ArchitectureMode.x86_32;
                }
                if (Is64bit)
                {
                    mode = SharpDisasm.ArchitectureMode.x86_64;
                }
                var baseAddress = HexStringToUlong(BaseAddress);
                //SharpDisasm.Disassembler.Translator.ResolveRip = true;
                // Configure the translator to output instruction addresses and instruction binary as hex
                SharpDisasm.Disassembler.Translator.IncludeAddress = true;
                SharpDisasm.Disassembler.Translator.IncludeBinary = true;
                // Create the disassembler
                var disasm = new SharpDisasm.Disassembler(
                    HexStringToByteArray(InputHexString),
                    mode, baseAddress, true);
                // Disassemble each instruction and output to console
                var result = "";
                foreach (var insn in disasm.Disassemble())
                {
                    result += insn.ToString() + Environment.NewLine;
                }
                OutputAsm = result;
            }
            catch (Exception ex)
            {
                OutputAsm = ex.ToString();
            }
        }

        private ulong HexStringToUlong(string? baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress))
            {
                return 0;
            }
            return Convert.ToUInt64(baseAddress, 16);
        }

        static byte[] HexStringToByteArray(string? hex)
        {
            if (hex == null)
            {
                return new byte[0];
            }
            hex = hex.Replace(" ", "");
            hex = hex.Replace("\r", "");
            hex = hex.Replace("\n", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
