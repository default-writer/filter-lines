using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace filter_lines
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var md5 = MD5.Create();
            var hashes = new HashSet<long>();
            (long, string) Hash(string input, Encoding encoding = null)
            {
                var bytes = (Span<byte>)stackalloc byte[encoding.GetByteCount(input)];
                var destination = (Span<byte>)stackalloc byte[md5.HashSize / 8];
                encoding.GetBytes(input, bytes);
                return md5.TryComputeHash(bytes, destination, out int _bytesWritten)
                    ? (BitConverter.ToInt64(destination.ToArray()), input) : (0, null);
            }
            async Task<IEnumerable<string>> ReadFileAsync(string fileName) =>
                await File.ReadAllLinesAsync(fileName).ConfigureAwait(false);
            var dict1 = await ReadFileAsync(args[0]).ConfigureAwait(false);
            var dict2 = await ReadFileAsync(args[1]).ConfigureAwait(false);
            var hashes1 = dict1.Select(_ => Hash(_, Encoding.UTF8));
            var hashes2 = dict2.Select(_ => Hash(_, Encoding.UTF8));
            var keys1 = new HashSet<long>(hashes1.Select(_ => _.Item1));
            var keys2 = new HashSet<long>(hashes2.Select(_ => _.Item1));
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            File.WriteAllLines(args[0], hashes1
                .Where(_ => !keys2.Contains(_.Item1))
                .Select(_ => _.Item2));
            stopwatch.Watch();
            Console.WriteLine("Hello World!");
        }
    }

    public static class StopwatchExtensions
    {
        public static void Watch(this Stopwatch stopwatch, string message = "",
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) =>
        Console.WriteLine(
            $"{stopwatch.Elapsed} " +
            $"{message} " +
            $"{memberName} " +
            $"{sourceFilePath}:{sourceLineNumber}");
    }
}
