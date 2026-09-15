using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace PuppyMod.Common.Utils;

public sealed class SoundPad
{
    private readonly List<SoundStyle> _sounds;

    private SoundPad()
    {
        _sounds = [];
    }

    private SoundPad(string category, string[] names, float pitch, float variance, float volume) : this()
    {
        AppendSpecific(category, names, pitch, variance, volume);
    }

    private static SoundStyle LoadSound(string category, string name, float pitch = 0.5f, float variance = 0.5f, float volume = 1f) =>
        new($"PuppyMod/Assets/Sounds/{category}/{name}") { Pitch = pitch, PitchVariance = variance, Volume = volume };

    private static string[] ScanCategory(string category)
    {
        if (Main.dedServ)
            return [];
        var mod = ModContent.GetInstance<PuppyMod>();
        string prefix = $"Assets/Sounds/{category}/";
        try
        {
            var packed = mod.GetFileNames()
                .Where(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
            if (packed.Length > 0) return packed;
        }
        catch (Exception ex)
        {
            try { mod.Logger.Warn($"ScanCategory GetFileNames failed for {category}: {ex.Message}"); } catch { }
        }

        string assemblyPath;
        try { assemblyPath = Path.GetDirectoryName(typeof(SoundPad).Assembly.Location) ?? ""; }
        catch (Exception ex) { try { mod.Logger.Warn($"ScanCategory assemblyPath failed for {category}: {ex.Message}"); } catch { } return []; }

        var relativePath = Path.Combine("Assets", "Sounds", category);
        var candidates = new[]
        {
            Path.Combine(assemblyPath, "Assets", "Sounds", category),
            relativePath
        };
        foreach (var candidate in candidates)
        {
            try
            {
                if (!Directory.Exists(candidate))
                    continue;
                string[] files;
                try
                {
                    files = Directory.EnumerateFiles(candidate, "*", SearchOption.TopDirectoryOnly)
                        .Select(Path.GetFileNameWithoutExtension)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToArray();
                }
                catch (Exception ex)
                {
                    try { mod.Logger.Warn($"ScanCategory EnumerateFiles failed for {candidate}: {ex.Message}"); } catch { }
                    continue;
                }
                if (files.Length > 0) return files;
            }
            catch (Exception ex)
            {
                try { mod.Logger.Warn($"ScanCategory candidate failed for {candidate}: {ex.Message}"); } catch { }
            }
        }
        return [];
    }

    public static SoundPad LoadCategory(string category, float pitch = 0.5f, float variance = 0.5f, float volume = 1f)
    {
        var names = ScanCategory(category);
        return new SoundPad(category, names, pitch, variance, volume);
    }

    public static SoundPad LoadSpecific(string category, string[] names, float pitch = 0.5f, float variance = 0.5f, float volume = 1f)
    {
        return new SoundPad(category, names, pitch, variance, volume);
    }

    public SoundPad AppendCategory(string category, float pitch = 0.5f, float variance = 0.5f, float volume = 1f)
    {
        var names = ScanCategory(category);
        return AppendSpecific(category, names, pitch, variance, volume);
    }

    public SoundPad AppendSpecific(string category, string[] names, float pitch = 0.5f, float variance = 0.5f, float volume = 1f)
    {
        if (names == null || names.Length == 0) return this;
        foreach (var name in names)
        {
            var trimmed = name?.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            _sounds.Add(LoadSound(category, trimmed, pitch, variance, volume));
        }
        return this;
    }

    public int Count => _sounds.Count;

    public SoundStyle GetByIndex(int index)
    {
        if (_sounds.Count == 0)
            return default;
        int clamped = index % _sounds.Count;
        if (clamped < 0) clamped += _sounds.Count;
        return _sounds[clamped];
    }

    public SoundStyle GetRandom(int minIndex = 0)
    {
        if (_sounds.Count == 0)
            return default;
        if ((uint)minIndex >= (uint)_sounds.Count)
            throw new ArgumentOutOfRangeException(nameof(minIndex));
        return _sounds[Main.rand.Next(minIndex, _sounds.Count)];
    }
}
