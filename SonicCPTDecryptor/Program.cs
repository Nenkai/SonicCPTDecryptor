using System.IO;

using csharp_prs;
using Simias.Encryption;

namespace SonicCPTDecryptor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Sega .CPT decryptor/decompressor by Nenkai");
                Console.WriteLine("Usage: <input .cpt file> <output path>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Input file does not exist");
                return;
            }

            try
            {
                byte[] file = File.ReadAllBytes(args[0]);

                // Calculated before decryption starting 0x40
                // Relevant function in Sonic Mega Collection Plus US PS2: 0x103090 (md5init/hash/finish & memcmp into decrypt)
                // May not actually match md5, it's weird
                // Not using for now
                Span<byte> md5 = file.AsSpan(0x00, 0x10);
                byte[] bfKey = file.AsSpan(0x10, 0x10).ToArray();
                // No evidence that the next 0x20 bytes are even used, garbage bytes?

                // Decrypt. Hashing also starts from here
                var blow = new Blowfish(bfKey);
                blow.Decipher(file.AsSpan(0x40), (int)((file.Length - 0x40) & 0xFFFFFFF8));

                // Love it when libs don't expose a span interface :(
                unsafe
                {
                    fixed (byte* ptr = file)
                    {
                        byte[] decompressed = Prs.Decompress(ptr + 0x40, file.Length - 0x40);

                        string dirName = Path.GetDirectoryName(Path.GetFullPath(args[1]));
                        if (!Directory.Exists(dirName))
                            Directory.CreateDirectory(dirName);

                        File.WriteAllBytes(args[1], decompressed);

                        Console.WriteLine($"Decrypted & decompressed file to {args[1]}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process file: {ex}");
            }
        }
    }
}