using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;

namespace PuppyMod.Common.Utils;

public sealed class SoundPad
{
    private readonly List<SoundStyle> _sounds;

    private SoundPad()
    {
        _sounds = new List<SoundStyle>();
    }

    private SoundPad(string category, string[] names, float pitch, float variance, float volume) : this()
    {
        AppendSpecific(category, names, pitch, variance, volume);
    }

    private static SoundStyle LoadSound(string category, string name, float pitch = 0.5f, float variance = 0.5f, float volume = 1f) =>
        new($"PuppyMod/Assets/Sounds/{category}/{name}") { Pitch = pitch, PitchVariance = variance, Volume = volume };

    private static string[] ScanCategory(string category)
    {
        var candidates = new[]
        {
            Path.Combine("Assets", "Sounds", category),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Sounds", category),
            Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", category),
            Path.Combine(Path.GetDirectoryName(typeof(SoundPad).Assembly.Location) ?? "", "Assets", "Sounds", category),
        };
        var modSources = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Terraria", "tModLoader", "ModSources", "PuppyMod", "Assets", "Sounds", category);
        candidates = candidates.Append(modSources).ToArray();
        foreach (var c in candidates)
        {
            if (Directory.Exists(c))
            {
                var files = Directory.EnumerateFiles(c, "*", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToArray();
                if (files.Length > 0) return files;
            }
        }
        var dir = FindAssetsCategory(category);
        if (dir != null)
        {
            return Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
        }
        return Array.Empty<string>();
    }

    private static string FindAssetsCategory(string category)
    {
        try
        {
            var start = Path.GetDirectoryName(typeof(SoundPad).Assembly.Location);
            for (int i = 0; i < 6 && !string.IsNullOrEmpty(start); i++)
            {
                var candidate = Path.Combine(start, "Assets", "Sounds", category);
                if (Directory.Exists(candidate)) return candidate;
                var candidate2 = Path.Combine(start, "ModSources", "PuppyMod", "Assets", "Sounds", category);
                if (Directory.Exists(candidate2)) return candidate2;
                start = Path.GetDirectoryName(start);
            }
        }
        catch { }
        return null;
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
        foreach (var n in names)
        {
            var trimmed = n?.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            _sounds.Add(LoadSound(category, trimmed, pitch, variance, volume));
        }
        return this;
    }

    public SoundStyle GetRandom(int offset = 0)
    {
        if ((uint)offset >= (uint)_sounds.Count)
            throw new ArgumentOutOfRangeException(nameof(offset));
        return _sounds[Main.rand.Next(offset, _sounds.Count)];
    }
}
