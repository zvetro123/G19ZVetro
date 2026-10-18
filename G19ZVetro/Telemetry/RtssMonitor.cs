using System.IO.MemoryMappedFiles;
using System.Text;

namespace G19ZVetro.Telemetry;

internal class RtssMonitor
{
    public bool IsAvailable()
    {
        try
        {
            using var mmf = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void DumpApps()
    {
        try
        {
            using var mmf = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            using var view = mmf.CreateViewAccessor();

            uint appEntrySize = view.ReadUInt32(8);
            uint appArrOffset = view.ReadUInt32(12);
            uint appArrSize = view.ReadUInt32(16);

            for (int i = 0; i < Math.Min((int)appArrSize, 20); i++)
            {
                long baseOffset = appArrOffset + ((long)i * appEntrySize);

                uint processId = view.ReadUInt32(baseOffset);

                if (processId == 0)
                    continue;

                byte[] nameBytes = new byte[260];

                for (int j = 0; j < 260; j++)
                {
                    nameBytes[j] = view.ReadByte(baseOffset + 4 + j);
                }

                string name = Encoding.ASCII
                    .GetString(nameBytes)
                    .TrimEnd('\0');

                uint time0 = view.ReadUInt32(baseOffset + 268);
                uint time1 = view.ReadUInt32(baseOffset + 272);
                uint frames = view.ReadUInt32(baseOffset + 276);
                uint frameTime = view.ReadUInt32(baseOffset + 280);

                Console.WriteLine(
                    $"{i}: PID={processId} NAME={name}");

                Console.WriteLine(
                    $"    Time0={time0} Time1={time1} Frames={frames} FrameTime={frameTime}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public int GetFps()
    {
        try
        {
            using var mmf = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            using var view = mmf.CreateViewAccessor();

            uint appEntrySize = view.ReadUInt32(8);
            uint appArrOffset = view.ReadUInt32(12);
            uint appArrSize = view.ReadUInt32(16);

            for (int i = 0; i < appArrSize; i++)
            {
                long baseOffset = appArrOffset + ((long)i * appEntrySize);

                uint processId = view.ReadUInt32(baseOffset);

                if (processId == 0)
                    continue;

                uint frameTime = view.ReadUInt32(baseOffset + 280);

                if (frameTime > 0)
                {
                    return (int)(1000000.0 / frameTime);
                }
            }
        }
        catch
        {
        }

        return 0;
    }
}