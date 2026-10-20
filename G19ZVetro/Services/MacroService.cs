using System.Windows.Input;
using Forms = System.Windows.Forms;

namespace G19ZVetro.Services;

public sealed class MacroService
{
    public bool IsRecording { get; private set; }

    public string? RecordingTarget { get; private set; }

    private readonly List<Key> _recordedKeys = new();

    public List<string> EndRecording()
    {
        IsRecording = false;

        var result = _recordedKeys
            .Select(k => k.ToString())
            .ToList();

        RecordingTarget = null;
        _recordedKeys.Clear();

        return result;
    }
    public void StartRecording()
    {
        IsRecording = true;
        RecordingTarget = null;
        _recordedKeys.Clear();
    }

    public void SetTarget(string target)
    {
        RecordingTarget = target;
    }

    public void CaptureKey(Key key)
    {
        if (!IsRecording)
        {
            return;
        }

        _recordedKeys.Add(key);
    }

    public void Play(IEnumerable<string> keys)
    {
        Thread.Sleep(1000);

        foreach (var key in keys)
        {
            Forms.SendKeys.SendWait(key);
        }
    }
}