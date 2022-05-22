using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace caj2pdf;

public static class Caj2PdfConverter
{
    #region Private 字段

    /// <summary>
    /// 大概看了一下代码 caj2pdf 这个python库理论上不能并行的，它会在工作目录下生成固定名称的临时文件。。。（有条件的时候使用C#进行重写）
    /// </summary>
    private static readonly SemaphoreSlim s_convertSemaphoreSlim = new(1, 1);

    private static string s_binEntryFilePath = null!;
    private static string s_binRootPath = null!;
    private static string s_defaultPythonEntryFilePath = null!;
    private static bool s_isInited = false;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// Python的入口文件路径，为null时使用自动检测的值
    /// </summary>
    public static string? PythonEntryFilePath { get; set; }

    #endregion Public 属性

    #region Public 方法

    public static async Task ConvertAsync(string sourceFilePath, string targetFilePath, CancellationToken cancellationToken)
    {
        Caj2PdfConverter.Init();

        if (!File.Exists(s_binEntryFilePath))
        {
            throw new FileNotFoundException("Not found entry file.", s_binEntryFilePath);
        }

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Not found source file.", sourceFilePath);
        }

        if (!Path.IsPathFullyQualified(sourceFilePath))
        {
            sourceFilePath = Path.Combine(Environment.CurrentDirectory, sourceFilePath);
        }

        if (!Path.IsPathFullyQualified(targetFilePath))
        {
            targetFilePath = Path.Combine(Environment.CurrentDirectory, targetFilePath);
        }

        await s_convertSemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var targetDir = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDir)
                && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            var startInfo = new ProcessStartInfo(PythonEntryFilePath ?? s_defaultPythonEntryFilePath, $"{s_binEntryFilePath} convert {sourceFilePath} -o {targetFilePath}")
            {
                WorkingDirectory = s_binRootPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            using var process = Process.Start(startInfo);

            if (process is null)
            {
                throw new Caj2PdfWrapperException("process start fail.");
            }

            await process.WaitForExitAsync(cancellationToken);

            var errorOutput = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(errorOutput))
            {
                throw new Caj2PdfWrapperException($"convert fail. Raw error content: {{{{{errorOutput}}}}}");
            }
            if (process.ExitCode != 0)
            {
                throw new Caj2PdfWrapperException($"convert fail. Process exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            CheckTmpFile($"{targetFilePath}.tmp");
            CheckTmpFile(targetFilePath);
            CheckTmpFile(Path.Combine(targetFilePath, "pdf.tmp"));
            CheckTmpFile(Path.Combine(targetFilePath, "pdf_toc.pdf"));
            CheckTmpFile(Path.Combine(s_binRootPath, "pdf.tmp"));
            CheckTmpFile(Path.Combine(s_binRootPath, "pdf_toc.pdf"));

            if (ex is Caj2PdfWrapperException)
            {
                throw;
            }
            throw new Caj2PdfWrapperException("convert fail", ex);
        }
        finally
        {
            s_convertSemaphoreSlim.Release();
        }

        static void CheckTmpFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch { }
            }
        }
    }

    public static void Init()
    {
        if (Volatile.Read(ref s_isInited))
        {
            return;
        }
        if (RuntimeInformation.ProcessArchitecture != Architecture.X86
            && RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException("Only support x64 and x86.");
        }

        var builder = new StringBuilder(256);

        var assemblyPath = Assembly.GetEntryAssembly()?.Location;

        if (!string.IsNullOrWhiteSpace(assemblyPath))
        {
            builder.Append(Path.GetDirectoryName(assemblyPath));
            builder.Append(Path.DirectorySeparatorChar);
        }
        else
        {
            builder.Append(Environment.CurrentDirectory);
            builder.Append(Path.DirectorySeparatorChar);
        }

        builder.Append("caj2pdf");
        builder.Append(Path.DirectorySeparatorChar);

        if (OperatingSystem.IsLinux())
        {
            builder.Append("linux");
        }
        else if (OperatingSystem.IsWindows())
        {
            builder.Append(value: "windows");
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
        builder.Append(Path.DirectorySeparatorChar);

        if (Environment.Is64BitOperatingSystem)
        {
            builder.Append("x64");
        }
        else
        {
            builder.Append("x86");
        }

        builder.Append(Path.DirectorySeparatorChar);

        s_binRootPath = builder.ToString();

        s_binEntryFilePath = Path.Combine(s_binRootPath, "caj2pdf");

        if (!File.Exists(s_binEntryFilePath))
        {
            throw new Caj2PdfWrapperException($"not found the file \"caj2pdf\" at {s_binEntryFilePath}");
        }

        AppendEnvironmentPath(s_binRootPath);

        s_defaultPythonEntryFilePath = TestCommand("python3") ?? TestCommand("python") ?? null!;

        Volatile.Write(ref s_isInited, true);

        static string? TestCommand(string pythonEntryCMD)
        {
            try
            {
                var startInfo = new ProcessStartInfo(pythonEntryCMD, "--version")
                {
                    RedirectStandardOutput = true,
                };
                using var process = Process.Start(startInfo);
                process?.WaitForExit();

                if (process?.ExitCode == 0)
                {
                    return pythonEntryCMD;
                }
            }
            catch { }
            return null;
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static void AppendEnvironmentPath(string path)
    {
        foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
        {
            var key = item.Key as string;
            if (string.Equals(key, "path", StringComparison.OrdinalIgnoreCase)
                && item.Value is string pathValue)
            {
                Environment.SetEnvironmentVariable(key!, $"{pathValue}{Path.PathSeparator}{path}");
            }
        }
    }

    #endregion Private 方法
}
